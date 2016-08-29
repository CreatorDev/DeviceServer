/***********************************************************************************************************************
 Copyright (c) 2016, Imagination Technologies Limited and/or its affiliated group companies.
 All rights reserved.

 Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
 following conditions are met:
     1. Redistributions of source code must retain the above copyright notice, this list of conditions and the
        following disclaimer.
     2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
        following disclaimer in the documentation and/or other materials provided with the distribution.
     3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote
        products derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
 USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
***********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Net.Sockets;
using System.Configuration;
using System.Threading;
using Imagination.Model;

namespace Imagination.BusinessLogic
{
	internal class NotificationOrchestrator 
	{

		private class Notification
		{
			public string Table { get; set; }
			public bool Purge { get; set; }
			public string ID { get; set; }
		}

		private DateTime _LastNotificationDate = DateTime.UtcNow;
        private ConcurrentDictionary<string, List<NotificationTcpClient>> _NotificationClients = new ConcurrentDictionary<string, List<NotificationTcpClient>>();
		private List<Socket> _Clients = new List<Socket>();
		private bool _AlwaysNotify = false;

		private const string MESSAGE_SETUPCOMPLETE = "1|";
		private const string MESSAGE_NOTIFY = "2|";
		private const string MESSAGE_HEARTBEAT = "3|";


		private bool _Terminate = false;
		private ManualResetEvent _TriggerPublishNotifications;
		private Thread _PublishNotificationsThread;
		private Queue<Notification> _NotificationQueue = new Queue<Notification>(1000);


        public void AddNotifcationClient(string table, Socket client)
        {
            List<NotificationTcpClient> queues;
            while (!_NotificationClients.TryGetValue(table, out queues))
            {
                queues = new List<NotificationTcpClient>();
                if (_NotificationClients.TryAdd(table, queues))
                    break;
            }
            bool found = false;
            for (int index = 0; index < queues.Count; index++)
            {
                if (queues[index].ClientSocket == client)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                queues.Add(new NotificationTcpClient() { ClientSocket = client });
            }
            if (!_Clients.Contains(client))
            {
                _Clients.Add(client);
            }
        }


		private void PublishNotifications()
		{
			while (!_Terminate)
			{
				_TriggerPublishNotifications.Reset();
				while (_NotificationQueue.Count > 0)
				{
					Notification notification = null;
					lock (_NotificationQueue)
					{
						if (_NotificationQueue.Count > 0)
							notification = _NotificationQueue.Dequeue();
					}
					if (notification != null)
					{
						SendChangeNotification(notification);
					}
				}
				if (!_Terminate)
					_TriggerPublishNotifications.WaitOne();
			}
		}

        private void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                NotificationTcpClient client = e.UserToken as NotificationTcpClient;
                if (client != null)
                    client.ClientSocket = null;
            }
        }

        private void SendChangeNotification(Notification notification)
        {
            List<NotificationTcpClient> clients;
            if (_NotificationClients.TryGetValue(notification.Table, out clients))
            {
                string messageText;
                messageText = string.Concat(MESSAGE_NOTIFY, notification.Table, ",", notification.Purge.ToString(), ",", notification.ID, "\n");
                byte[] buffer = Encoding.ASCII.GetBytes(messageText);
                for (int index = 0; index < clients.Count; index++)
                {
                    if (clients[index].ClientSocket == null)
                    {
                        clients.RemoveAt(index);
                        index--;
                    }
                    else
                    {
                        try
                        {
                            SocketAsyncEventArgs parameters = new SocketAsyncEventArgs();
                            parameters.SetBuffer(buffer, 0, buffer.Length);
                            parameters.UserToken = clients[index];
                            parameters.Completed += SendCompleted;
                            if (!clients[index].ClientSocket.SendAsync(parameters))
                            {
                                if (parameters.SocketError != SocketError.Success)
                                {
#if DEBUG
                                    ApplicationEventLog.WriteEntry("Flow", string.Format("SendChangeNotification: Error sending notification for table {0} - error ={1}", notification.Table, parameters.SocketError), System.Diagnostics.EventLogEntryType.Error);
                                    Trace.WriteLine(TTracePriority.High, string.Format("SendChangeNotification: Error sending notification for table {0} - error ={1}", notification.Table, parameters.SocketError));
#endif
                                    clients.RemoveAt(index);
                                    index--;
                                }
                            }
                        }
#pragma warning disable 168
                        catch (Exception ex)
#pragma warning restore 168
                        {
#if DEBUG
                            ApplicationEventLog.WriteEntry("Flow", string.Format("SendChangeNotification: Exception sending notification for table {0} Client Count={1}\n{2}", notification.Table, clients.Count, ex), System.Diagnostics.EventLogEntryType.Error);
                            Trace.WriteLine(TTracePriority.High, ex.ToString());
#endif
                            clients.RemoveAt(index);
                            index--;
                        }
                    }
                }
            }
        }

        internal void SendChangeNotification(string table, bool purge, string id)
        {
			Notification notification = new Notification();
			notification.Table = table;
			notification.Purge = purge;
			notification.ID = id;
			lock (_NotificationQueue)
			{
				_NotificationQueue.Enqueue(notification);
			}
			_TriggerPublishNotifications.Set();
        }

		public void SendHeartBeat(string connectionString, bool online)
		{
			if (_Clients.Count > 0)
			{
				string messageText = string.Concat(MESSAGE_HEARTBEAT, connectionString, ",", online.ToString(),"\n");
				byte[] buffer = Encoding.ASCII.GetBytes(messageText);
				SendToAllClients(buffer);
			}
		}

		private void SendToAllClients(byte[] data)
		{
			if (_Clients.Count > 0)
			{
                for (int index = 0; index < _Clients.Count; index++)
				{
					try
					{
                        SocketAsyncEventArgs parameters = new SocketAsyncEventArgs();
                        parameters.SetBuffer(data, 0, data.Length);
                        if (!_Clients[index].SendAsync(parameters))
                        {
                            if (parameters.SocketError != SocketError.Success)
                            {
                                _Clients.RemoveAt(index);
                                index--;
                            }

                        }
                    }
					catch
					{
						_Clients.RemoveAt(index);
						index--;
					}
				}
			}
		}

		public void Start()
		{
			_Terminate = false;
			if (_TriggerPublishNotifications == null)
				_TriggerPublishNotifications = new ManualResetEvent(false);
			if (_PublishNotificationsThread == null)
			{
				_PublishNotificationsThread = new Thread(new ThreadStart(PublishNotifications));
				if (_PublishNotificationsThread.Name == null)
					_PublishNotificationsThread.Name = "PublishServiceNotifications";
				_PublishNotificationsThread.IsBackground = true;
				_PublishNotificationsThread.Start();
			}
            Trace.WriteLine(TTracePriority.High, "Queue StartWaiting");
			string setting = ConfigurationManager.AppSettings["AlwaysNotify"];
			bool alwaysNotify;
			if (bool.TryParse(setting, out alwaysNotify))
				_AlwaysNotify = alwaysNotify;
		}

		public void Stop()
		{
			_Terminate = true;
			_TriggerPublishNotifications.Set();
			try
			{
				if (_PublishNotificationsThread != null)
				{
					if (_PublishNotificationsThread.IsAlive)
					{
						_PublishNotificationsThread.Join();
						_PublishNotificationsThread = null;
					}
				}
			}
			catch
			{

			}
		}

	}

}


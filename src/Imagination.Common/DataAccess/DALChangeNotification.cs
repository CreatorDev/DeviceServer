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
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using Imagination.Model;
using Microsoft.Extensions.Logging;

namespace Imagination.DataAccess
{
    internal class DALChangeNotification
	{
		private class ReceiveStateObject
		{
			public NotificationServer NotificationServer;
			public Socket Client = null;
			public const int BufferSize = 256;
			public byte[] Buffer = new byte[BufferSize];
			public StringBuilder Data = new StringBuilder();
			public DateTime StartReceive;
		}

		private class NotificationClient
		{
			private NotificationEventHandler _EventHandler;
			private string _TableName;

			public bool Enabled;
            public string TableName { get { return _TableName; } set { _TableName = value; SetKey(); } }
            public NotificationEventHandler EventHandler { get { return _EventHandler; } set { _EventHandler = value; SetKey(); } }

            public string Key { get; private set; }

            private void SetKey()
            {
                Key = GetKey(_TableName, _EventHandler);
            }
            public static string GetKey(string table, NotificationEventHandler eventHandler)
			{
				string result;
				if (eventHandler == null)
                    result = string.Concat(table);
				else
                    result = string.Concat(table, "|", eventHandler.Method.DeclaringType.AssemblyQualifiedName);
				return result;
			}
		}

		private class NotificationServer
		{
            public string ServerName;
            public int Port;
            public bool Connected;
			public Socket TcpClient;
			public DateTime AllowConnectionAfter;
			public List<NotificationClient> TableChangeClients = new List<NotificationClient>();

			public NotificationServer()
			{
				AllowConnectionAfter = DateTime.MinValue;
			}

			internal void AddClient(NotificationClient notificationClient)
			{
				TableChangeClients.Add(notificationClient);
			}

			internal void RemoveClient(NotificationClient notificationClient)
			{
				for (int index = 0; index < TableChangeClients.Count; index++)
				{
					if (TableChangeClients[index].Key == notificationClient.Key)
					{
						TableChangeClients.RemoveAt(index);
						break;
					}
				}
			}

            public override string ToString()
            {
                return $"{ServerName}:{Port} {(Connected ? "" : "not")} connected";
            }
        }

		private int _NotificationCount;
		private int _NotificationClientCount;
		private AsyncCallback _ReceiveCallback;
		private bool _Async = true;
		private AsyncCallback _NotifyClientsCallback;
		private AsyncCallback _SendCallback;
        private int _NotificationServerIndex;
        private List<NotificationServer> _NotificationServers;
        private ConcurrentDictionary<string, List<NotificationClient>> _TableChangeClients;

		private ConcurrentDictionary<string, NotificationClient> _NotificationClients;

        private Queue<NotificationClient> _NotificationsToRepair;
        private List<NotificationServer> _NotificationServersToReconnect;
        private ManualResetEvent _TriggerNotificationRepair;
		private Thread _ProcessNotificationRepairThread;
		private bool _Terminate;

		private const string SETUP_NOTIFICATION = "1,";
		private const string BROADCAST_PURGE_NOTIFICATION = "4,";
		private const string BROADCAST_NOTIFICATION = "5,";
		private const string BROADCAST_USER_REQUEST = "6,";

		private const char MESSAGE_SETUPCOMPLETE = '1';
		private const char MESSAGE_NOTIFY = '2';
		private const char MESSAGE_HEARTBEAT = '3';
		private const char MESSAGE_BLACKLISTUSER = '4';

        
#if DEBUG
        private static string _LogFolder;
#endif
        private static ILogger _Logger;

        static DALChangeNotification()
		{
            if (ServiceConfiguration.LoggerFactory != null)
                _Logger = ServiceConfiguration.LoggerFactory.CreateLogger(nameof(DALChangeNotification));
#if DEBUG
            // Trace logging
            string tempFolder = ServiceConfiguration.TempFolder;
            _LogFolder = System.IO.Path.Combine(tempFolder, ServiceConfiguration.Name);
#endif
        }



        public DALChangeNotification(List<Uri> notificationServers)
		{


            _NotificationServersToReconnect = new List<NotificationServer>();
            _NotificationServers = new List<NotificationServer>();
			_TableChangeClients = new ConcurrentDictionary<string, List<NotificationClient>>();
			_NotificationClients = new ConcurrentDictionary<string, NotificationClient>();
            _NotificationsToRepair = new Queue<NotificationClient>(64);
			_NotifyClientsCallback = new AsyncCallback(NotifyClientsCallback);
			_ReceiveCallback = new AsyncCallback(ReceiveCallback);
			_SendCallback = new AsyncCallback(SendCallback);

			_TriggerNotificationRepair = new ManualResetEvent(false);
			_ProcessNotificationRepairThread = new Thread(new ThreadStart(ProcessNotificationsToRepair));
			if (_ProcessNotificationRepairThread.Name == null)
				_ProcessNotificationRepairThread.Name = "ProcessNotificationsToRepair";
			_ProcessNotificationRepairThread.IsBackground = true;
			_ProcessNotificationRepairThread.Start();

            foreach (Uri server in notificationServers)
            {
                NotificationServer notificationServer;
                notificationServer = new NotificationServer();
                notificationServer.ServerName = server.DnsSafeHost;
                if (server.Port == -1)
                    notificationServer.Port = 14050;
                else
                    notificationServer.Port = server.Port;                
                notificationServer.Connected = false;
                _NotificationServers.Add(notificationServer);
                _NotificationServersToReconnect.Add(notificationServer);
            }

            if (_NotificationServersToReconnect.Count > 0)
                _TriggerNotificationRepair.Set();
		}

		~DALChangeNotification()
		{
			_Terminate = true;
			_TriggerNotificationRepair.Set();
			_ProcessNotificationRepairThread.Join();
		}


 
		private void AddNotificationToRepair(NotificationClient notificationClient)
		{
			notificationClient.Enabled = false;
            lock (_NotificationsToRepair)
            {
                _NotificationsToRepair.Enqueue(notificationClient);
            }
            _TriggerNotificationRepair.Set();
        }

		private void AddNotificationToRepair(List<NotificationClient> notificationClients)
		{
			for (int index = 0; index < notificationClients.Count; index++)
			{
				AddNotificationToRepair(notificationClients[index]);
			}
		}

        private bool AddNotificationClient(NotificationClient notificationClient)
		{
            bool result = false;
            if (_NotificationClients.TryAdd(notificationClient.Key, notificationClient))
            {
                List<NotificationClient> handlers;
                if (_TableChangeClients.ContainsKey(notificationClient.TableName))
                    handlers = _TableChangeClients[notificationClient.TableName];
                else
                {
                    handlers = new List<NotificationClient>();
                    _TableChangeClients.TryAdd(notificationClient.TableName, handlers);
                }
                handlers.Add(notificationClient);
                result = true;
            }
            return result;
		}

       

		private void Send(byte[] buffer, int bufferCount)
		{
			foreach (NotificationServer item in _NotificationServers)
			{
				try
				{
					if (item.Connected)
						Send(item.TcpClient, buffer, bufferCount);
				}
				catch
				{

				}
			}
		}

        public void BroadcastTableChange(string tableName, bool purge, string id)
        {
#if UNITTEST_DEBUG
			
			string logFileName = System.IO.Path.Combine(_LogFolder, string.Format("SendMessage_{0:yyyyMMdd}.log", DateTime.Now));
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(string.Format("BroadcastTableChange {0:yyyyMMdd_HHmmss.fffff} 4,{1},{2},{3},{4}| \r\n", DateTime.Now, tenantID,(int)dbCategory, tableName, purge));
			lock (this)
			{
				System.IO.File.AppendAllText(logFileName, sb.ToString());
			}
#endif
            byte[] buffer = Encoding.ASCII.GetBytes(string.Concat(BROADCAST_PURGE_NOTIFICATION, tableName, ",", purge.ToString(), ",", id, "|"));
            foreach (NotificationServer item in _NotificationServers)
            {
                try
                {
                    if (item.Connected)
                        Send(item.TcpClient, buffer);
                }
                catch
                {

                }
            }
        }

        public void BroadcastTableChange(string tableName, string id)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(string.Concat(BROADCAST_NOTIFICATION, tableName, ",1,", id, "|"));
            foreach (NotificationServer item in _NotificationServers)
            {
                try
                {
                    if (item.Connected)
                        Send(item.TcpClient, buffer);
                }
                catch
                {

                }
            }
        }

		private bool Connect(NotificationServer notificationServer, bool autoRepair)
		{
			bool connected = false;
			//NotificationServer result = null;
			DateTime timeOutTime = DateTime.Now.AddSeconds(5);
			while (!connected && DateTime.Now < timeOutTime)
			{
				try
				{
					Socket tcpClient = null;
                    IPAddress[] hostAddresses = Dns.GetHostAddresses(notificationServer.ServerName);
					foreach (IPAddress address in hostAddresses)
					{
						if ((address.AddressFamily == AddressFamily.InterNetwork)) // || (address.AddressFamily == AddressFamily.InterNetworkV6)
						{
							tcpClient = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                            tcpClient.Connect(address, notificationServer.Port);
							tcpClient.NoDelay = true;
							break;
						}
					}
					if (tcpClient != null)
					{
						notificationServer.TcpClient = tcpClient;
						notificationServer.Connected = true;
						if (notificationServer.TableChangeClients.Count > 0)
						{
							StringBuilder tables = new StringBuilder();
							for (int index = 0; index < notificationServer.TableChangeClients.Count; index++)
							{
								tables.Append(SETUP_NOTIFICATION);
                                tables.Append(notificationServer.TableChangeClients[index].TableName);
								tables.Append("|");
							}
							byte[] buffer = Encoding.ASCII.GetBytes(tables.ToString());
							tcpClient.Send(buffer);
						}
						ReceiveStateObject stateObject = new ReceiveStateObject();
						stateObject.Client = tcpClient;
						stateObject.NotificationServer = notificationServer;
						SocketError socketError;
						tcpClient.BeginReceive(stateObject.Buffer, 0, ReceiveStateObject.BufferSize, SocketFlags.None, out socketError, _ReceiveCallback, stateObject);
					}
					connected = true;
				}
				catch
				{

				}
				if (!connected)
					System.Threading.Thread.Sleep(500);
			}
            if (!connected)
            {
                lock (notificationServer)
                {
                    notificationServer.Connected = false;
                    notificationServer.AllowConnectionAfter = DateTime.Now.AddSeconds(10);
                    AddNotificationToRepair(notificationServer.TableChangeClients);
                    notificationServer.TableChangeClients.Clear();
                    if (autoRepair)
                    {
                        lock (_NotificationServersToReconnect)
                        {
                            _NotificationServersToReconnect.Add(notificationServer);
                        }
                        _TriggerNotificationRepair.Set();
                    }
                }
            }
            return connected;
		}


        private NotificationServer GetNextNotificationServer()
        {
            NotificationServer result = null;
            int index;
            lock (this)
            {
                index = _NotificationServerIndex = (_NotificationServerIndex + 1) % _NotificationServers.Count;
            }
            result = _NotificationServers[index];
            return result;
        }

        private void NotifyClients(string tableName, bool purge, string id)
        {
            if (_TableChangeClients.ContainsKey(tableName))
            {
                List<NotificationClient> handlers = _TableChangeClients[tableName];
                NotificationEventArgs e = new NotificationEventArgs();
                e.Purge = purge;
                e.ID = id;
                for (int index = 0; index < handlers.Count; index++)
                {
                    try
                    {
                        if (handlers[index].Enabled)
                        {
                            if (_Async)
                                handlers[index].EventHandler.BeginInvoke(null, e, _NotifyClientsCallback, handlers[index]);
                            else
                                handlers[index].EventHandler.Invoke(null, e);
                        }
#if UNITTEST_DEBUG
						else if (!handlers[index].Enabled)
						{
							ApplicationEventLog.WriteEntry("Flow", string.Format("NotifyClients: Failed to send notification for table {0}.\n{1}\n{2}\n{3}", tableName, handlers[index].Enabled, handlers[index].Server, handlers[index].ConnnectionString), EventLogEntryType.Error);
						}
#endif
                    }
                    catch (Exception ex)
                    {
                        ApplicationEventLog.WriteEntry("Flow", string.Format("NotifyClients: Exception sending notification for table {0}.\n{1}", tableName, ex), EventLogEntryType.Error);
                    }
                }
            }
        }

		private void NotifyClientsCallback(IAsyncResult result)
		{
			NotificationEventHandler notifyDelegate = (NotificationEventHandler)((System.Runtime.Remoting.Messaging.AsyncResult)result).AsyncDelegate;
			notifyDelegate.EndInvoke(result);
		}

		private void ProcessMessage(string messageText)
		{
#if UNITTEST_DEBUG
			string logFileName = System.IO.Path.Combine(_LogFolder, string.Format("ProcessMessage_{0:yyyyMMdd}.log", DateTime.Now));
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(string.Format("RX {0:yyyyMMdd_HHmmss.fffff} {1}\r\n", DateTime.Now, messageText));
			lock (this)
			{
				System.IO.File.AppendAllText(logFileName, sb.ToString());
			}
#endif

			switch (messageText[0])
			{
				case MESSAGE_SETUPCOMPLETE:
					break;
				case MESSAGE_NOTIFY:
					bool purge = false;
					string id = null;
					string[] fields = messageText.Substring(2).Split(',');
					int fieldsLength = fields.Length;
					if (fieldsLength >= 1)
					{
						if (fieldsLength >= 2)
						{
							if (!bool.TryParse(fields[1], out purge))
								purge = false;
							if (fieldsLength >= 3)
								id = fields[2];
						}
                        NotifyClients(fields[0], purge, id);
					}
					break;
				default:
					break;
			}
		}
		
		private void ProcessNotificationsToRepair()
		{
            Dictionary<string, object> servers = new Dictionary<string, object>();
            while (!_Terminate)
            {
                while (!_Terminate && (_NotificationsToRepair.Count > 0))
                {
					try
					{
						NotificationClient notificationSetupInfo;
						lock (_NotificationsToRepair)
						{
							notificationSetupInfo = _NotificationsToRepair.Dequeue();
						}
						bool successful = SetupNotification(1, notificationSetupInfo.TableName, notificationSetupInfo.EventHandler);
						if (!successful)
							Thread.Sleep(100);
					}
					catch (Exception ex)
					{
						ApplicationEventLog.WriteEntry("Flow", string.Format("ProcessNotificationsToRepair:Exception processing notifications {0}", ex), EventLogEntryType.Error);
					}
                }
                int index = 0;
                servers.Clear();
				while (index < _NotificationServersToReconnect.Count)
				{
					bool connected = false;
					if (_NotificationServersToReconnect[index].Connected)
                        connected = true;
					else
					{
						if (!servers.ContainsKey(_NotificationServersToReconnect[index].ServerName))
						{
							servers.Add(_NotificationServersToReconnect[index].ServerName, null);
							Connect(_NotificationServersToReconnect[index], false);
						}
					}
                    if (connected)
                    {
                        lock (_NotificationServersToReconnect)
                        {
                            _NotificationServersToReconnect.RemoveAt(index);
                        }
                    }
                    else
						index++;

				}
                _TriggerNotificationRepair.Reset();
                int timeout = Timeout.Infinite;
                if (_NotificationServersToReconnect.Count > 0)
                    timeout = 5000;
                if (_NotificationsToRepair.Count == 0)
                    _TriggerNotificationRepair.WaitOne(timeout);
            }

		}

		private void ReceiveCallback(IAsyncResult asyncResult)
		{
			ReceiveStateObject stateObject = asyncResult.AsyncState as ReceiveStateObject;
			try
			{
				int readCount = stateObject.Client.EndReceive(asyncResult);
				bool continueReceive = true;
				if (readCount > 0)
				{
					string text = Encoding.ASCII.GetString(stateObject.Buffer, 0, readCount);
#if UNITTEST_DEBUG
						string logFileName = System.IO.Path.Combine(_LogFolder, string.Format("ReceiveMessage_{0:yyyyMMdd}.log", DateTime.Now));
						StringBuilder sb = new StringBuilder();
						sb.AppendFormat(string.Format("RX {0:yyyyMMdd_HHmmss.fffff} {1}\r\n", DateTime.Now, text));
						lock (this)
						{
							System.IO.File.AppendAllText(logFileName, sb.ToString());
						}
#endif
					stateObject.Data.Append(text);
					if (text.Contains("\n"))
					{
						string data = stateObject.Data.ToString();
						int count = 0;
						while (!string.IsNullOrEmpty(data))
						{
							int index = data.IndexOf("\n");
							if (index == -1)
							{
								// partial message - break out and get the rest in the following response 
								//count = 0;
								break;
							}
							else
							{
								count += (index + 1);
								string message = data.Substring(0, index);
								ProcessMessage(message);
								index++;
								if (index < data.Length)
									data = data.Substring(index);
								else
									data = null;
							}
						}
						if (count > 0)
							stateObject.Data.Remove(0, count);
					}
				}
				else
				{
					if (stateObject.Client.ReceiveTimeout <= 0)
						continueReceive = false;
					else
					{
						TimeSpan diff = DateTime.Now.Subtract(stateObject.StartReceive);
						if (diff.TotalMilliseconds < stateObject.Client.ReceiveTimeout)
							continueReceive = false;
					}
				}
				if (continueReceive)
				{
					stateObject.StartReceive = DateTime.Now;
					SocketError socketError;
					stateObject.Client.BeginReceive(stateObject.Buffer, 0, ReceiveStateObject.BufferSize, SocketFlags.None, out socketError, _ReceiveCallback, stateObject);
				}
				else
				{
                    ApplicationEventLog.WriteEntry("Flow", string.Format("DALChangeNotification::ReceiveCallback continueReceive false - Reconnecting to serve {0} port {1}", stateObject.NotificationServer.ServerName, stateObject.NotificationServer.Port), EventLogEntryType.Error);
					stateObject.NotificationServer.Connected = false;
					stateObject.Client.Close();
                    Connect(stateObject.NotificationServer,true);
				}
			}
			catch (Exception ex)
			{
				ApplicationEventLog.WriteEntry("Flow", string.Format("DALChangeNotification::ReceiveCallback Exception\n{0}", ex), EventLogEntryType.Error);
				stateObject.NotificationServer.Connected = false;
				stateObject.Client.Close();
                Connect(stateObject.NotificationServer, true);
			}
		}

		private void Send(Socket tcpClient, byte[] buffer)
		{
			if (tcpClient != null)
			{
				lock (tcpClient)
				{
					tcpClient.Send(buffer);
					//SocketError socketError;
					//tcpClient.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, out socketError, _SendCallback, tcpClient);
				}
			}
		}

		private void Send(Socket tcpClient, byte[] buffer, int bufferCount)
		{
			if (tcpClient != null)
			{
				lock (tcpClient)
				{
					tcpClient.Send(buffer, 0, bufferCount, SocketFlags.None);
					//SocketError socketError;
					//tcpClient.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, out socketError, _SendCallback, tcpClient);
				}
			}
		}

		private void SendCallback(IAsyncResult asyncResult)
		{
			try
			{
				Socket tcpClient = asyncResult.AsyncState as Socket;
				tcpClient.EndSend(asyncResult);
			}
			catch
			{

			}
		}

		public void SetupNotification(string tableName, NotificationEventHandler changeEventHandler)
		{
			Interlocked.Increment(ref _NotificationCount);
            bool success = SetupNotification(2, tableName, changeEventHandler);
            if (!success)
                _Logger?.LogError(string.Format("Failed initial notification setup with dbnotify service.\nNotificationServerList: {0} \nTableName: {1}", string.Join(", ", _NotificationServers), tableName));
        }

		private bool SetupNotification(int attemptsCount, string tableName, NotificationEventHandler changeEventHandler)
		{
			bool result = false;
            string key = NotificationClient.GetKey(tableName, changeEventHandler);
            NotificationClient notificationClient;
            while (!_NotificationClients.TryGetValue(key, out notificationClient))
            {
                notificationClient = new NotificationClient() { TableName = tableName, EventHandler = changeEventHandler };
                if (AddNotificationClient(notificationClient))
                    break;
            }
            result = SetupNotification(notificationClient, attemptsCount);
            if (!result)
			{
                AddNotificationToRepair(notificationClient);
			}
			return result;
		}

        private bool SetupNotification(NotificationClient notificationClient, int attemptsCount)
        {
            bool result = false;
            NotificationServer notificationServer = null;
            int count = 0;
            while ((notificationServer == null) && (count < attemptsCount))
            {
                notificationServer = GetNextNotificationServer();
                if ((notificationServer != null) && (notificationServer.AllowConnectionAfter > DateTime.Now))
                    notificationServer = null;
                else if ((notificationServer == null) || !notificationServer.Connected || !notificationServer.TcpClient.Connected)
                {
                    if (!Connect(notificationServer, true))
                        notificationServer = null;
                }
                if (notificationServer != null)
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(string.Concat(SETUP_NOTIFICATION, notificationClient.TableName, "|"));
                    try
                    {
                        Send(notificationServer.TcpClient, buffer);
                        Interlocked.Increment(ref _NotificationClientCount);
                        notificationClient.Enabled = true;
                        lock (notificationServer)
                        {
                            notificationServer.AddClient(notificationClient);
                        }
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        ApplicationEventLog.WriteEntry("Flow", string.Format("DALChangeNotification::SetupNotification {0}", ex), System.Diagnostics.EventLogEntryType.Error);
                        notificationServer = null;
                    }
                }
                count = (count + 1) % int.MaxValue;
            }
            return result;
        }


		internal void Terminate()
		{
			_Terminate = true;
			_TriggerNotificationRepair.Set();
		}

	}
}

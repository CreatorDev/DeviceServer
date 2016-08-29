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
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.IO;
using CoAP;
using Imagination.Model;
using Microsoft.Extensions.Logging;

namespace Imagination.LWM2M
{
	internal class Clients
	{
		private ConcurrentDictionary<Guid, LWM2MClient> _Clients = new ConcurrentDictionary<Guid, LWM2MClient>(PlatformHelper.DefaultConcurrencyLevel, 10000);
		private ConcurrentDictionary<Guid, LWM2MClient> _ClientByDeviceID = new ConcurrentDictionary<Guid, LWM2MClient>(PlatformHelper.DefaultConcurrencyLevel, 10000);

		private Queue<LWM2MClient> _ClientsToValidate = new Queue<LWM2MClient>(1000);
		private bool _Terminate = false;
		private ManualResetEvent _TriggerProcessRequests = new ManualResetEvent(false);
		private Thread _ProcessRequestsThread;
		private System.Timers.Timer _CheckDeadClientsTimer;


		public Clients()
		{
			_ProcessRequestsThread = new Thread(new ThreadStart(ProcessRequests));
			if (_ProcessRequestsThread.Name == null)
				_ProcessRequestsThread.Name = "ProcessRequestsThread";
			_ProcessRequestsThread.IsBackground = true;
			_ProcessRequestsThread.Start();
			_CheckDeadClientsTimer = new System.Timers.Timer();
			_CheckDeadClientsTimer.Elapsed += new System.Timers.ElapsedEventHandler(_CheckDeadClientsTimer_Elapsed);
			_CheckDeadClientsTimer.AutoReset = true;
			_CheckDeadClientsTimer.Interval = 60000;
			_CheckDeadClientsTimer.Start();
		}

		private void _CheckDeadClientsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			List<LWM2MClient> clientsToRemove = new List<LWM2MClient>();
			try
			{
				ICollection<LWM2MClient> clients = _Clients.Values;
				foreach (LWM2MClient client in clients)
				{
					if (client.Lifetime < DateTime.UtcNow)
						clientsToRemove.Add(client);
				}
			}
			catch
			{

			}

			try
			{
				foreach (LWM2MClient item in clientsToRemove)
				{
                    ApplicationEventLog.Write(LogLevel.Information, string.Concat("Client connection expired: ", item.Name, " address ", item.Address.ToString()));
                    BusinessLogicFactory.Events.ClientConnectionExpired(item);
                    LWM2MClient client;
					item.Cancel();
					_Clients.TryRemove(item.ClientID, out client);
					if (item.ClientID != Guid.Empty)
						_ClientByDeviceID.TryRemove(item.ClientID, out client);
				}
			}
			catch
			{

			}

		}

		public void AddClient(LWM2MClient client)
		{
			_Clients.TryAdd(client.ClientID, client);
			lock (_ClientsToValidate)
			{
				_ClientsToValidate.Enqueue(client);
			}
			_TriggerProcessRequests.Set();
		}

		public void ClientChangedSupportedTypes(LWM2MClient client)
		{
            BusinessLogicFactory.Events.ClientUpdate(client);
            lock (_ClientsToValidate)
			{
				_ClientsToValidate.Enqueue(client);
			}
			_TriggerProcessRequests.Set();
		}

		public void DeleteClient(Guid clientID)
		{
			LWM2MClient client;
			_Clients.TryRemove(clientID, out client);
			if (client.ClientID != Guid.Empty)
			{
				client.Cancel();
				BusinessLogicFactory.Events.ClientDisconnected(client);
			}
		}

		public LWM2MClient GetClient(Guid clientID)
		{
			LWM2MClient result;
			_Clients.TryGetValue(clientID, out result);
			return result;
		}


        public List<Client> GetClients()
        {
            List<Client> result = new List<Client>();
            try
            {
                ICollection<LWM2MClient> clients = _Clients.Values;
                foreach (LWM2MClient client in clients)
                {
                    result.Add(client);
                }
            }
            catch
            {

            }
            return result;
        }

		public Imagination.Model.ObjectDefinitionLookups GetLookups()
		{
			return DataAccessFactory.ObjectDefinitions.GetLookups();
		}

		private void ProcessRequests()
		{
			while (!_Terminate)
			{
				_TriggerProcessRequests.Reset();
                while (_ClientsToValidate.Count > 0)
                {
                    LWM2MClient client = null;
                    lock (_ClientsToValidate)
                    {
                        if (_ClientsToValidate.Count > 0)
                            client = _ClientsToValidate.Dequeue();
                    }
                    if (client != null)
                    {
                        if ((client.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) || (client.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6))
                        {
                            ValidateClient(client);
                        }
                    }
                }
                if (!_Terminate)
					_TriggerProcessRequests.WaitOne();
			}
		}

		public void Stop()
		{
			_Terminate = true;
			_TriggerProcessRequests.Set();
		}

        private void ValidateClient(LWM2MClient client)
        {
            UpdateClientActivity(client);

            if (client.ClientID != Guid.Empty)
            {
                // TODO: for now simply add the client to the DB. In the future we will want to validate the client first.
                DataAccessFactory.Clients.SaveClient(client, TObjectState.Add);
                BusinessLogicFactory.Events.ClientConnected(client);
                LWM2MClient existingClient;
                if (_ClientByDeviceID.TryRemove(client.ClientID, out existingClient))
                {
                    if (existingClient != client)
                        existingClient.Cancel();
                }
                _ClientByDeviceID.TryAdd(client.ClientID, client);
            }
        }

        public static bool ByteArrayCompare(byte[] x, byte[] y)
		{
			bool result = true;
			if ((x == null) && (y == null))
				result = false;
			else if ((x != null) && (y != null))
			{
				if (x.Length == y.Length)
				{
					for (int index = 0; index < x.Length; index++)
					{
						if (x[index] != y[index])
						{
							result = false;
							break;
						}
					}
				}
				else
					result = false;
			}
			return result;
		}

		public void UpdateClientActivity(Client client)
		{
			DateTime activityTime = DateTime.UtcNow;
			TimeSpan diff = activityTime.Subtract(client.LastUpdateActivityTime);
			if (diff.TotalMinutes > 10)
			{
				UpdateClientActivity(client.ClientID, activityTime);
				client.LastUpdateActivityTime = activityTime;
			}
			client.LastActivityTime = activityTime;
		}

		public void UpdateClientActivity(Guid deviceID, DateTime activityTime)
		{
			if (deviceID != Guid.Empty)
			{
				try
				{
					DataAccessFactory.Clients.UpdateClientActivity(deviceID, activityTime);
				}
				catch
				{

				}
			}
		}

        public void UpdateClientLifetime(Guid deviceID, DateTime lifeTime)
        {
            if (deviceID != Guid.Empty)
            {
                try
                {
                    DataAccessFactory.Clients.UpdateClientLifetime(deviceID, lifeTime);
                }
                catch
                {

                }
            }
        }

    }
}

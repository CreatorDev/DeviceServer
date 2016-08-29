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
using Imagination.Model;
using CoAP;
using System.IO;
using Imagination.LWM2M;

namespace Imagination.BusinessLogic
{
	internal class Clients
	{
        public const int REQUEST_TIMEOUT = 20000;
		private Queue<LWM2MClient> _NewClients = new Queue<LWM2MClient>(1000);
		private bool _Terminate = false;
		private ManualResetEvent _TriggerProcessRequests = new ManualResetEvent(false);
		private Thread _ProcessRequestsThread;

		public Clients()
		{
			_ProcessRequestsThread = new Thread(new ThreadStart(ProcessRequests));
			if (_ProcessRequestsThread.Name == null)
				_ProcessRequestsThread.Name = "ProcessRequestsThread";
			_ProcessRequestsThread.IsBackground = true;
			_ProcessRequestsThread.Start();
		}

		public void AddClient(LWM2MClient client)
		{
			lock (_NewClients)
			{
				_NewClients.Enqueue(client);
			}
			_TriggerProcessRequests.Set();
		}

		private byte[] SerialiseObject(ITlvSerialisable item)
		{
			byte[] result = null;
			using (MemoryStream steam = new MemoryStream())
			{
				TlvWriter writer = new TlvWriter(steam);
				item.Serialise(writer);
				result = steam.ToArray();
			}
			return result;
		}

		private byte[] SerialiseObject(ITlvSerialisable item, ushort objectInstanceID)
		{
			byte[] result = null;
			using (MemoryStream steam = new MemoryStream())
			{
				TlvWriter writer = new TlvWriter(steam);
				item.Serialise(writer);
				byte[] objectTLV = steam.ToArray();
				int length = (int)steam.Length;
				steam.SetLength(0);
				writer.WriteType(TTlvTypeIdentifier.ObjectInstance, objectInstanceID, length);
				steam.Write(objectTLV, 0, length);
				result = steam.ToArray();
			}
			return result;
		}
        
		private void ProcessRequests()	
		{
			while (!_Terminate)
			{
				_TriggerProcessRequests.Reset();
				while (_NewClients.Count > 0)
				{
					LWM2MClient client = null;
                    try
                    {
                        lock (_NewClients)
                        {
                            if (_NewClients.Count > 0)
                                client = _NewClients.Dequeue();
                        }
                        if (client != null)
                        {
                            if ((client.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) || (client.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6))
                            {
                                Server server = BusinessLogicFactory.Servers.GetServer();
                                System.Net.IPEndPoint ipEndPoint = client.Address as System.Net.IPEndPoint;
                                CoapClient coapClient = new CoapClient();
                                coapClient.EndPoint = client.EndPoint;
                                coapClient.Timeout = REQUEST_TIMEOUT;
                                ushort objectInstanceID = 1;
                                foreach (Model.Security item in server.EndPoints)
                                {
                                    Request request = new Request(Method.PUT);
                                    request.ContentType = TlvConstant.CONTENT_TYPE_TLV;// (int)MediaType.ApplicationOctetStream;
                                    request.Destination = client.Address;
                                    request.UriPath = "/0";
                                    request.Payload = SerialiseObject(item, objectInstanceID);
                                    objectInstanceID++;
                                    coapClient.SendAsync(request, (response) =>
                                        {
                                            if (response != null && response.StatusCode == StatusCode.Changed)
                                            {
                                                request = new Request(Method.PUT);
                                                request.ContentType = TlvConstant.CONTENT_TYPE_TLV;//(int)MediaType.ApplicationOctetStream;
                                                request.Destination = client.Address;
                                                request.UriPath = "/1";
                                                request.Payload = SerialiseObject(server, 1);
                                                coapClient.SendAsync(request, (response2) =>
                                                    {
                                                        if (response2 != null && response2.StatusCode == StatusCode.Changed)
                                                        {
                                                            request = new Request(Method.POST);
                                                            request.Destination = client.Address;
                                                            request.UriPath = "/bs";
                                                            coapClient.SendAsync(request);
                                                        }
                                                    });
                                            }
                                        }
                                    );
                                }

                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
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
    }
}

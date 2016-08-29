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
using System.Threading;
using CoAP;
using CoAP.Net;
using CoAP.Server;
using Imagination.Model;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.ServiceModel.Description;
using DTLS;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace Imagination.LWM2M
{
	public class Server : IMessageDeliverer
	{
		private enum TRequestType
		{
			NotSet = 0,
			Register = 1,
			Update,
			Deregister,
		}

		private class ClientRequest
		{
			public TRequestType RequestType { get; set; }
			public Exchange Exchange { get; set; }
		}

		private Queue<ClientRequest> _Requests = new Queue<ClientRequest>(1000);
		private bool _Terminate = false;
		private ManualResetEvent _TriggerProcessRequests = new ManualResetEvent(false);
		private Thread _ProcessRequestsThread;
		private CoapServer _CoapServer;
		private NativeIPCServer _NativeServerAPI;
        private NativeIPCServer _NativeServerAPIv6;
        private ServiceHost _ServiceHost;
		private string _ServerEndPoint;
        private FlowSecureChannel _SecureChannel;

        private PSKIdentities _PSKIdentities = new PSKIdentities();

        public int Port { get; set; }

        public PSKIdentities PSKIdentities
        {
            get { return _PSKIdentities; }
        }

        public bool SecureOnly { get; set; }

        public Server()
		{
            SecureOnly = true;
        }

        private void AddRequest(TRequestType request, Exchange exchange)
		{
			lock (_Requests)
			{
				_Requests.Enqueue(new ClientRequest() { RequestType = request, Exchange = exchange });
			}
			_TriggerProcessRequests.Set();
		}

		void IMessageDeliverer.DeliverRequest(Exchange exchange)
		{
			Request request = exchange.Request;			
			TRequestType requestType = TRequestType.NotSet;
			if (request.Method == Method.POST)
			{
				if (request.URI.AbsolutePath == "/rd")
				{
					if ((request.ContentType == (int)MediaType.ApplicationLinkFormat) || (request.ContentType == -1))
					//if (request.ContentType == (int)MediaType.ApplicationLinkFormat)
					{
						requestType = TRequestType.Register;
					}
				}
			}

			if (request.URI.AbsolutePath.StartsWith("/rd/"))
			{
				if ((request.Method == Method.POST) || (request.Method == Method.PUT))
				{
					requestType = TRequestType.Update;
				}
				else if (request.Method == Method.DELETE)
				{
					requestType = TRequestType.Deregister;
				}
			}
			if (requestType == TRequestType.NotSet)
			{
				exchange.SendReject();
			}
			else
			{
				AddRequest(requestType, exchange);
			}
		}

		void IMessageDeliverer.DeliverResponse(Exchange exchange, CoAP.Response response)
		{
			if (exchange.Request == null)
				throw new ArgumentException("Request should not be empty.", "exchange");
			exchange.Request.Response = response;
		}

		private void ProcessRequests()
		{
			while (!_Terminate)
			{
				_TriggerProcessRequests.Reset();
				while (_Requests.Count > 0)
				{
					ClientRequest clientRequest = null;
                    try
                    {
                        lock (_Requests)
                        {
                            if (_Requests.Count > 0)
                                clientRequest = _Requests.Dequeue();
                        }
                        if (clientRequest != null)
                        {
                            switch (clientRequest.RequestType)
                            {
                                case TRequestType.NotSet:
                                    break;
                                case TRequestType.Register:
                                    ProcessRegisterRequest(clientRequest.Exchange);
                                    break;
                                case TRequestType.Update:
                                    ProcessUpdateRequest(clientRequest.Exchange);
                                    break;
                                case TRequestType.Deregister:
                                    ProcessDeregisterRequest(clientRequest.Exchange);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
                    }
				}
				if (!_Terminate)
					_TriggerProcessRequests.WaitOne();
			}
		}

		private void ProcessRegisterRequest(Exchange exchange)
		{
			Request request = exchange.Request;
			LWM2MClient client = new LWM2MClient();
			client.Server = _ServerEndPoint;
			client.Address = request.Source;
			client.Parse(request.UriQueries);
			ObjectTypes objectTypes = new ObjectTypes();
			objectTypes.Parse(request.PayloadString);
			client.SupportedTypes = objectTypes;
			client.EndPoint = exchange.EndPoint;
            if (_SecureChannel != null)
            {
                CertificateInfo certificateInfo = _SecureChannel.GetClientCertificateInfo(client.Address);
                if (certificateInfo == null)
                {
                    string pskIdentity = _SecureChannel.GetClientPSKIdentity(client.Address);
                    if (!string.IsNullOrEmpty(pskIdentity))
                    {
                        Guid clientID;
                        PSKIdentity identity = DataAccessFactory.Identities.GetPSKIdentity(pskIdentity);
                        if (identity != null)
                        {
                            if (StringUtils.GuidTryDecode(pskIdentity, out clientID))
                            {
                                client.ClientID = clientID;
                            }
                            client.OrganisationID = identity.OrganisationID;
                        }
                    }
                }
                else
                {
                    Console.WriteLine(certificateInfo.Subject.CommonName);
                    Console.WriteLine(certificateInfo.Subject.Organistion);
                    Guid clientID;
                    if (Guid.TryParse(certificateInfo.Subject.CommonName,  out clientID))
                    {
                        client.ClientID = clientID;
                    }
                    int organisationID;
                    if (int.TryParse(certificateInfo.Subject.Organistion, out organisationID))
                    {
                        client.OrganisationID = organisationID;
                    }
                }
            }
            if (client.ClientID != Guid.Empty && (client.OrganisationID > 0 || !SecureOnly) && !DataAccessFactory.Clients.IsBlacklisted(client.ClientID))
            {
                BusinessLogicFactory.Clients.AddClient(client);
            }
			
			Response response = Response.CreateResponse(request, StatusCode.Created);			
			//response.AddOption(Option.Create(OptionType.LocationPath, string.Concat("rd/",StringUtils.GuidEncode(client.ClientID))));
			response.AddOption(Option.Create(OptionType.LocationPath, "rd"));
			response.AddOption(Option.Create(OptionType.LocationPath, StringUtils.GuidEncode(client.ClientID)));
			
			exchange.SendResponse(response);

            ApplicationEventLog.Write(LogLevel.Information, string.Concat("Client registered ", client.Name, " address ", client.Address.ToString()));
		}

		private void ProcessUpdateRequest(Exchange exchange)
		{
			Request request = exchange.Request;
			Guid clientID;
			Response response;
			if (StringUtils.GuidTryDecode(request.UriPath.Substring(4), out clientID))
			{
				LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
				if (client == null)
				{
					response = Response.CreateResponse(request, StatusCode.NotFound);
				}
				else
				{
					client.Parse(request.UriQueries);
					BusinessLogicFactory.Clients.UpdateClientActivity(client);
                    client.Address = request.Source;
					client.EndPoint = exchange.EndPoint;
                    bool updatedLifeTime = false;
                    if ((request.ContentType == (int)MediaType.ApplicationLinkFormat) || (request.ContentType == -1))
					{
						if (request.PayloadSize > 0)
						{
							ObjectTypes objectTypes = new ObjectTypes();
							objectTypes.Parse(request.PayloadString);
                            if (ObjectTypes.Compare(client.SupportedTypes, objectTypes) != 0)
                            {
                                client.SupportedTypes = objectTypes;
                                if (client.ClientID != Guid.Empty)
                                {
                                    DataAccessFactory.Clients.SaveClient(client, TObjectState.Add);
                                    updatedLifeTime = true;
                                }
                                BusinessLogicFactory.Clients.ClientChangedSupportedTypes(client);
                            }
						}
					}
                    if (!updatedLifeTime)
                    {
                        BusinessLogicFactory.Clients.UpdateClientLifetime(client.ClientID, client.Lifetime);
                    }
                    response = Response.CreateResponse(request, StatusCode.Changed);

                    ApplicationEventLog.Write(LogLevel.Information, string.Concat("Client update ", client.Name, " address ", client.Address.ToString()));
				}
			}
			else
			{
				ApplicationEventLog.WriteEntry(string.Concat("Invalid update location", request.UriPath));
				response = Response.CreateResponse(request, StatusCode.BadRequest);
			}
			exchange.SendResponse(response);
		}

		private void ProcessDeregisterRequest(Exchange exchange)
		{
			Request request = exchange.Request;
			Guid clientID = StringUtils.GuidDecode(request.UriPath.Substring(4));
			LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
			Response response;
			if (client == null)
			{
				response = Response.CreateResponse(request, StatusCode.NotFound);
			}
			else
			{
				client.Lifetime = DateTime.UtcNow;
                BusinessLogicFactory.Clients.UpdateClientLifetime(client.ClientID, DateTime.UtcNow);
				BusinessLogicFactory.Clients.UpdateClientActivity(client.ClientID, DateTime.UtcNow);
                BusinessLogicFactory.Clients.DeleteClient(clientID);
				response = Response.CreateResponse(request, StatusCode.Deleted);

                ApplicationEventLog.Write(LogLevel.Information, string.Concat("Client deregister ", client.Name, " address ", client.Address.ToString()));
			}
			exchange.SendResponse(response);
		}

		public void Start()
		{
			CoAP.Log.LogManager.Level = CoAP.Log.LogLevel.Error;
			int port;
			string apiPort = System.Configuration.ConfigurationManager.AppSettings["APIPort"];
			if (!int.TryParse(apiPort, out port))
				port = 14080;
			_ProcessRequestsThread = new Thread(new ThreadStart(ProcessRequests));
			if (_ProcessRequestsThread.Name == null)
				_ProcessRequestsThread.Name = "ProcessRequestsThread";
			_ProcessRequestsThread.IsBackground = true;
			_ProcessRequestsThread.Start();
            if (_CoapServer == null)
            {
                _CoapServer = new CoapServer();
                _CoapServer.MessageDeliverer = this;
                if (!SecureOnly)
                    _CoapServer.AddEndPoint(new CoAPEndPoint(new FlowChannel(Port), CoapConfig.Default));
                _SecureChannel = new FlowSecureChannel(Port + 1);
                if (System.IO.File.Exists("LWM2MServer.pem"))
                {
                    _SecureChannel.CertificateFile = "LWM2MServer.pem";
                }
                _SecureChannel.PSKIdentities = _PSKIdentities;
                _SecureChannel.SupportedCipherSuites.Add(TCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8);
                _SecureChannel.SupportedCipherSuites.Add(TCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256);
                _SecureChannel.SupportedCipherSuites.Add(TCipherSuite.TLS_ECDHE_PSK_WITH_AES_128_CBC_SHA256);
                _SecureChannel.SupportedCipherSuites.Add(TCipherSuite.TLS_PSK_WITH_AES_128_CCM_8);
				_SecureChannel.SupportedCipherSuites.Add(TCipherSuite.TLS_PSK_WITH_AES_128_CBC_SHA256);
                _SecureChannel.ValidatePSK += new EventHandler<ValidatePSKEventArgs>(ValidatePSK);
                _CoapServer.AddEndPoint(new CoAPEndPoint(_SecureChannel, CoapConfig.Default));
            }
			_CoapServer.Start();

            ServiceEventMessage message = new ServiceEventMessage();
            Imagination.Model.LWM2MServer lwm2mServer = new Imagination.Model.LWM2MServer();
            lwm2mServer.Url = ServiceConfiguration.ExternalUri.ToString();
            message.AddParameter("Server", lwm2mServer);
            BusinessLogicFactory.ServiceMessages.Publish("LWM2MServer.Start", message, TMessagePublishMode.Confirms);


            _ServerEndPoint = string.Concat("net.tcp://", ServiceConfiguration.Hostname, ":", port.ToString(), "/LWM2MServerService");
			if (_NativeServerAPI == null)
				_NativeServerAPI = new NativeIPCServer(AddressFamily.InterNetwork,port);
			_NativeServerAPI.Start();
            if (_NativeServerAPIv6 == null)
                _NativeServerAPIv6 = new NativeIPCServer(AddressFamily.InterNetworkV6, port);
            _NativeServerAPIv6.Start();
            //if (_ServiceHost != null)
            //    _ServiceHost.Close();
            //_ServiceHost = new ServiceHost(typeof(Imagination.Service.ServerAPI));
            //ServiceThrottlingBehavior throttle = _ServiceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
            //if (throttle == null)
            //{
            //    throttle = new ServiceThrottlingBehavior
            //    {
            //        MaxConcurrentCalls = 100,
            //        MaxConcurrentSessions = 100,
            //        MaxConcurrentInstances = int.MaxValue
            //    };
            //    _ServiceHost.Description.Behaviors.Add(throttle);
            //}
            //else
            //{
            //    throttle.MaxConcurrentCalls = 100;
            //    throttle.MaxConcurrentSessions = 100;
            //    throttle.MaxConcurrentInstances = int.MaxValue;
            //}
            //NetTcpBinding netTcpBinding = new NetTcpBinding();
            //_ServiceHost.AddServiceEndpoint(typeof(Imagination.Service.ILWM2MServerService), netTcpBinding, _ServerEndPoint);
            ////int newLimit = _ServiceHost.IncrementManualFlowControlLimit(100);
            //_ServiceHost.Open();
        }

        public void Stop()
		{
			_Terminate = true;
			if (_ServiceHost != null)
			{
				_ServiceHost.Close();
				_ServiceHost = null;
			}
			if (_NativeServerAPI != null)
			{
				_NativeServerAPI.Stop();
			}
            if (_NativeServerAPIv6 != null)
            {
                _NativeServerAPIv6.Stop();
            }          
            _TriggerProcessRequests.Set();
			_CoapServer.Stop();
			_ProcessRequestsThread.Join();
			_ProcessRequestsThread = null;

		}

        private void ValidatePSK(System.Object sender, ValidatePSKEventArgs args)
        {
#if DEBUG
            Console.WriteLine("Validating PSK identity: " + System.Text.Encoding.UTF8.GetString(args.Identity));
#endif
            PSKIdentity pskIdentity = BusinessLogicFactory.Identities.GetPSKIdentity(System.Text.Encoding.UTF8.GetString(args.Identity));
            if (pskIdentity != null)
                args.Secret = StringUtils.HexStringToByteArray(pskIdentity.Secret);
        }
    }
}

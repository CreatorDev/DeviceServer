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
using DTLS;
using CoAP;
using CoAP.Net;
using CoAP.Server;
using System.Collections.Concurrent;
using Imagination.Model;
using Imagination.BusinessLogic;

namespace Imagination.LWM2M
{
	internal class BootstrapServer : IMessageDeliverer
	{
		private enum TRequestType
		{
			NotSet = 0,
			RequestBootstrap = 1,
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
        private PSKIdentities _PSKIdentities = new PSKIdentities();
		private CoapServer _CoapServer = new CoapServer();
		private int _Port;

		public int Port
		{
			get { return _Port; }
			set	{ _Port = value; }
		}

        public PSKIdentities PSKIdentities
        {
            get { return _PSKIdentities; }
        }

        public bool SecureOnly { get; set; }

        public BootstrapServer()
		{
            _PSKIdentities = new PSKIdentities();
			_Port = Spec.Default.DefaultPort;
			_CoapServer.MessageDeliverer = this;
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
				if (request.URI.AbsolutePath == "/bs")
				{
					requestType = TRequestType.RequestBootstrap;
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
							case TRequestType.RequestBootstrap:
								ProcessRequestBootstrap(clientRequest.Exchange);
								break;
							default:
								break;
						}
					}
				}
				if (!_Terminate)
					_TriggerProcessRequests.WaitOne();
			}
		}

		private void ProcessRequestBootstrap(Exchange exchange)
		{
			Request request = exchange.Request;
			LWM2MClient client = new LWM2MClient();  // TODO: fix warning
			client.Address = request.Source;
			client.EndPoint = exchange.EndPoint;
			client.Parse(request.UriQueries);
            BusinessLogicFactory.Clients.AddClient(client);
			Response response = Response.CreateResponse(request, StatusCode.Changed);
			exchange.SendResponse(response);
		}

		public void Start()
		{
			CoAP.Log.LogManager.Level = CoAP.Log.LogLevel.Error;
			_ProcessRequestsThread = new Thread(new ThreadStart(ProcessRequests));
			if (_ProcessRequestsThread.Name == null)
				_ProcessRequestsThread.Name = "ProcessRequestsThread";
			_ProcessRequestsThread.IsBackground = true;
			_ProcessRequestsThread.Start();
            if (!SecureOnly)
			    _CoapServer.AddEndPoint(new CoAPEndPoint(new FlowChannel(_Port), CoapConfig.Default));
            FlowSecureChannel secure = new FlowSecureChannel(_Port+1);
            if (System.IO.File.Exists("LWM2MBootstrap.pem"))
            {
                secure.CertificateFile = "LWM2MBootstrap.pem";
            }
            secure.PSKIdentities = _PSKIdentities;
            secure.SupportedCipherSuites.Add(TCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8);
            secure.SupportedCipherSuites.Add(TCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256);
            secure.SupportedCipherSuites.Add(TCipherSuite.TLS_ECDHE_PSK_WITH_AES_128_CBC_SHA256);
            secure.SupportedCipherSuites.Add(TCipherSuite.TLS_PSK_WITH_AES_128_CCM_8);
            secure.SupportedCipherSuites.Add(TCipherSuite.TLS_PSK_WITH_AES_128_CBC_SHA256);
            secure.ValidatePSK += new EventHandler<ValidatePSKEventArgs>(ValidatePSK);
            _CoapServer.AddEndPoint(new CoAPEndPoint(secure, CoapConfig.Default));
            _CoapServer.Start();
		}

        public void Stop()
		{
			_Terminate = true;
			_TriggerProcessRequests.Set();
			_CoapServer.Stop();
            BusinessLogicFactory.Clients.Stop();
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
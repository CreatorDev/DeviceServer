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

using DTLS;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using CoAP.Channel;


namespace Imagination.LWM2M
{
    public class FlowClientSecureChannel: IChannel
	{        
        public const int DEFAULT_RECEIVE_PACKET_SIZE = 4096;
        private int _ReceiveBufferSize;
		private int _SendBufferSize;
		private int _ReceivePacketSize = DEFAULT_RECEIVE_PACKET_SIZE;
		private int _Port;
		private System.Net.EndPoint _LocalEndPoint;
        private PSKIdentities _PSKIdentities;
        private DTLS.Client _Client;
        private DTLS.Client _ClientIPv4;
        private List<TCipherSuite> _SupportedCipherSuites;
		private int _Running;
        private string _CertificateFile;
        private string _PSKIdentity;
        private string _PSKSecret;
        private bool _Connected;
        //private int _Writing;

        public event EventHandler<DataReceivedEventArgs> DataReceived;


		public System.Net.EndPoint LocalEndPoint
		{
			get
			{
				return _Client == null
					? (_LocalEndPoint ?? new IPEndPoint(IPAddress.IPv6Any, _Port))
					: _Client.LocalEndPoint;
			}
		}

        public PSKIdentities PSKIdentities
        {
            get { return _PSKIdentities; }
            set { _PSKIdentities = value; }
        }

        public List<TCipherSuite> SupportedCipherSuites
        {
            get
            {
                return _SupportedCipherSuites;
            }
        }

		public int ReceiveBufferSize
		{
			get { return _ReceiveBufferSize; }
			set { _ReceiveBufferSize = value; }
		}

		public int SendBufferSize
		{
			get { return _SendBufferSize; }
			set { _SendBufferSize = value; }
		}

		public int ReceivePacketSize
		{
			get { return _ReceivePacketSize; }
			set { _ReceivePacketSize = value; }
		}

        public string CertificateFile
        {
            get { return _CertificateFile; }
            set { _CertificateFile = value; }
        }

        public string PSKIdentity
        {
            get { return _PSKIdentity; }
            set { _PSKIdentity = value; }
        }

        public string PSKSecret
        {
            get { return _PSKSecret; }
            set { _PSKSecret = value; }
        }

        public FlowClientSecureChannel()
			: this(0)
		{
		}

		public FlowClientSecureChannel(int port)
		{
			_Port = port;
            _SupportedCipherSuites = new List<TCipherSuite>();
        }

        public FlowClientSecureChannel(EndPoint localEP)
		{
			_LocalEndPoint = localEP;
            _SupportedCipherSuites = new List<TCipherSuite>();
		}


        private DTLS.Client CreateClient(EndPoint endPoint)
        {
            DTLS.Client result = new DTLS.Client(endPoint, _SupportedCipherSuites);

            if (!string.IsNullOrEmpty(_PSKIdentity) && !string.IsNullOrEmpty(_PSKSecret))
            {
                _SupportedCipherSuites.Clear();
                _SupportedCipherSuites.Add(TCipherSuite.TLS_PSK_WITH_AES_128_CCM_8);
                _SupportedCipherSuites.Add(TCipherSuite.TLS_PSK_WITH_AES_128_CBC_SHA256);
                _SupportedCipherSuites.Add(TCipherSuite.TLS_ECDHE_PSK_WITH_AES_128_CBC_SHA256);
                Console.WriteLine("Using PSK Identity: " + _PSKIdentity);
                
                result.PSKIdentities.AddIdentity(System.Text.Encoding.UTF8.GetBytes(_PSKIdentity), StringUtils.HexStringToByteArray(_PSKSecret));
            }
            else if (!string.IsNullOrEmpty(_CertificateFile))
            {
                _SupportedCipherSuites.Clear();
                _SupportedCipherSuites.Add(TCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CCM_8);
                _SupportedCipherSuites.Add(TCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA256);
                Console.WriteLine("Using Certificate: " + _CertificateFile);
                result.LoadCertificateFromPem(_CertificateFile);
            }
            else
            {
                Console.WriteLine("No Certificate or PSK supplied.");
            }

            result.DataReceived += new DTLS.Client.DataReceivedEventHandler(FireDataReceived);
            //if (_ReceiveBufferSize > 0)
            //{
            //    result.ReceiveBufferSize = _ReceiveBufferSize;
            //}
            //if (_SendBufferSize > 0)
            //{
            //    result.SendBufferSize = _SendBufferSize;
            //}
            return result;
        }

        public void Start()
		{
			if (System.Threading.Interlocked.CompareExchange(ref _Running, 1, 0) > 0)
				return;

			if (_LocalEndPoint == null)
			{
				try
				{
                    _Client = CreateClient(new IPEndPoint(IPAddress.IPv6Any, _Port));
                    // _ReceivePacketSize + 1); // +1 to check for > ReceivePacketSize
                }
				catch (SocketException e)
				{
					if (e.SocketErrorCode == SocketError.AddressFamilyNotSupported)
						_Client = null;
					else
						throw e;
				}

				if (_Client == null)
				{
					// IPv6 is not supported, use IPv4 instead
                    _Client = CreateClient(new IPEndPoint(IPAddress.Any, _Port));
				}
				else
				{
                    _ClientIPv4 = CreateClient(new IPEndPoint(IPAddress.Any, _Port));
                }
            }
			else
			{
                _Client = CreateClient(_LocalEndPoint);
			}
		}


		public void Stop()
		{
			if (System.Threading.Interlocked.Exchange(ref _Running, 0) == 0)
				return;

			if (_Client != null)
			{
				_Client.Stop();
				_Client = null;
			}
			if (_ClientIPv4 != null)
			{
                _ClientIPv4.Stop();
				_ClientIPv4 = null;
			}
		}

		public void Send(byte[] data, System.Net.EndPoint ep)
		{
            DTLS.Client socket = _Client;
            IPEndPoint remoteEP = (IPEndPoint)ep;
            if (remoteEP.AddressFamily == AddressFamily.InterNetwork)
            {
                if (_ClientIPv4 != null)
                {
                    // use the separated socket of IPv4 to deal with IPv4 conversions.
                    socket = _ClientIPv4;
                }
                else if (_Client.LocalEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    remoteEP = new IPEndPoint(IPAddressExtensions.MapToIPv6(remoteEP.Address), remoteEP.Port);
                }
            }
            if (!_Connected)
            {
                socket.ConnectToServer(remoteEP);
                _Connected = true;
            }

            socket.Send(data);
		}

		public void Dispose()
		{
			Stop();
		}

		private void FireDataReceived(System.Net.EndPoint ep, byte[] data)
		{
			EventHandler<DataReceivedEventArgs> h = DataReceived;
			if (h != null)
				h(this, new DataReceivedEventArgs(data, ep));
		}        
		

	}
}

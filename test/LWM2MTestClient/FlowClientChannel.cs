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
using CoAP.Channel;

namespace Imagination.LWM2M
{
    public class FlowClientChannel : IChannel
    {
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        private int _Port; 
        private System.Net.EndPoint _LocalEndPoint;
        
        private IChannel _InternalChannel;

        public System.Net.EndPoint LocalEndPoint
        {
            get 
            {
                 System.Net.EndPoint result;
                 if (_InternalChannel == null)
                 {
                     if (_LocalEndPoint == null)
                         result = new System.Net.IPEndPoint(System.Net.IPAddress.IPv6Any, _Port);
                     else
                        result = _LocalEndPoint;
                 }
                 else
                     result = _InternalChannel.LocalEndPoint;
                return result; 
            }
        }

        public string CertificateFile { get; set; }
        public string PSKIdentity { get; set; }
        public string PSKSecret { get; set; }

        public bool Secure { get; set; }


        public FlowClientChannel(int port)
		{
			_Port = port;
        }

        public FlowClientChannel(System.Net.EndPoint localEP)
		{
			_LocalEndPoint = localEP;
		}

        private void FireDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (DataReceived != null)
                DataReceived(this, e);
        }

        public void Send(byte[] data, System.Net.EndPoint ep)
        {
            if (_InternalChannel != null)
                _InternalChannel.Send(data, ep);
        }

        public void Start()
        {
            if (_InternalChannel == null)
            {
                if (Secure)
                {
                    if (_LocalEndPoint == null)
                        _InternalChannel = new FlowClientSecureChannel(_Port) { CertificateFile = CertificateFile, PSKIdentity = PSKIdentity, PSKSecret = PSKSecret };
                    else
                        _InternalChannel = new FlowClientSecureChannel(_LocalEndPoint) { CertificateFile = CertificateFile, PSKIdentity = PSKIdentity, PSKSecret = PSKSecret };
                }
                else
                {
                    if (_LocalEndPoint == null)
                        _InternalChannel = new FlowChannel(_Port);
                    else
                        _InternalChannel = new FlowChannel(_LocalEndPoint);
                }
                _InternalChannel.DataReceived += new EventHandler<DataReceivedEventArgs>(FireDataReceived);
                _InternalChannel.Start();
            }
        }

        public void Stop()
        {
            if (_InternalChannel != null)
                _InternalChannel.Stop();
            _InternalChannel = null;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}

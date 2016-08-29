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
using System.Threading.Tasks;

namespace Imagination.Model
{
    public class LWM2MServer
    {
        public string Url { get; set; }
        
        public uint Lifetime { get; set; }
        public uint? DefaultMinimumPeriod { get; set; }
        public uint? DefaultMaximumPeriod { get; set; }
        public uint? DisableTimeout { get; set; }
        public bool NotificationStoringWhenOffline { get; set; }
        public TBindingMode Binding { get; set; }

        public List<PSKIdentity> ServerIdentities { get; set; }
        
        public Certificate ServerCertificate { get; set; }

        private int _ServerIdentityIndex;

        public LWM2MServer()
        {
            Lifetime = 60;
            DefaultMinimumPeriod = 1;
            DefaultMaximumPeriod = 300;
            DisableTimeout = 86400;
            NotificationStoringWhenOffline = true;
            Binding = TBindingMode.UDP;
            _ServerIdentityIndex = (int)DateTime.Now.TimeOfDay.TotalSeconds;
        }

        public void AddServerIdentity(PSKIdentity pskIdentity)
        {
            if (ServerIdentities == null)
                ServerIdentities = new List<PSKIdentity>();
            ServerIdentities.Add(pskIdentity);
        }

        public PSKIdentity GetPSKIdentity()
        {
            PSKIdentity result = null;
            if ((ServerIdentities != null) && ServerIdentities.Count > 0)
            {
                int index;
                lock (this)
                {
                    index = _ServerIdentityIndex = (_ServerIdentityIndex + 1) % ServerIdentities.Count;
                }
                result = ServerIdentities[index];
            }
            return result;

        }
    }
}

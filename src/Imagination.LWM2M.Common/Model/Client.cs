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
using System.IO;

namespace Imagination.Model
{
    public class Client
    {
        public int OrganisationID { get; set; }
        public Guid ClientID { get; set; }
        public string Name { get; set; }
        public DateTime Lifetime { get; set; }
        public Version Version { get; set; }
        public TBindingMode BindingMode { get; set; }
        public string SMSNumber { get; set; }
        public string Server { get; set; }
        public ObjectTypes SupportedTypes { get; set; }

        private static readonly Version DefaultVersion = new Version(1,0);

		public DateTime LastUpdateActivityTime { get; set; }
		public DateTime LastActivityTime { get; set; }

		public const int DEFAULT_LIFETIME = 86400;

		public Client()
		{
#if DEBUG
            // set hardcoded client id to prevent the URL from changing all the time while debugging
            // can access via http://localhost:5000/clients/BNk5iqL3vESZF-XiVbG99w/
            ClientID = new Guid("8a39d904-f7a2-44bc-9917-e5e255b1bdf7"); 
#else
            ClientID = Guid.NewGuid();
#endif
            LastActivityTime = DateTime.UtcNow;
			LastUpdateActivityTime = LastActivityTime;
			Lifetime = DateTime.UtcNow.AddSeconds(DEFAULT_LIFETIME);
			Version = DefaultVersion;
		}

		public bool Parse(IEnumerable<string> queryParameters)
		{
			bool result = false;
			bool lifetimeSupplied = false;
			foreach (string item in queryParameters)
			{
				int index = item.IndexOf('=');
				if (index > 0)
				{
					string name = item.Substring(0, index);
					string value = item.Substring(index + 1);
					if (name == "ep")
                        Name = value;
					else if (name == "lt")
					{
						int seconds;
						if (int.TryParse(value, out seconds))
							Lifetime = DateTime.UtcNow.AddSeconds(seconds);
						lifetimeSupplied = true;
					}
					else if (name == "sms")
					{
						SMSNumber = value;
					}
					else if (name == "lwm2m")
					{
						Version version;
						if (Version.TryParse(value, out version))
							Version = version;
					}
					else if (name == "b")
					{
						value = value.ToUpper();
						TBindingMode bindingMode = TBindingMode.NotSet;
						foreach (char binding in value)
						{
							if (binding == 'U')
								bindingMode = TBindingMode.UDP;
							else if (binding == 'S')
							{
								if (bindingMode == TBindingMode.UDP)
									bindingMode = TBindingMode.UDPSMS;
								else if (bindingMode == TBindingMode.QueuedUDP)
									bindingMode = TBindingMode.QueuedUDPSMS;
								else
									bindingMode = TBindingMode.SMS;
							}
							else if (binding == 'Q')
							{
								if (bindingMode == TBindingMode.UDP)
									bindingMode = TBindingMode.QueuedUDP;
								else if (bindingMode == TBindingMode.SMS)
									bindingMode = TBindingMode.QueuedSMS;
							}
						}
						BindingMode = bindingMode;
					}		
				}
			}
			if (!lifetimeSupplied)
				Lifetime = DateTime.UtcNow.AddSeconds(DEFAULT_LIFETIME);
			return result;
		}


        public void Serialise(Stream stream)
        {
            IPCHelper.Write(stream, ClientID);
            IPCHelper.Write(stream, Name);
            IPCHelper.Write(stream, Lifetime);
            IPCHelper.Write(stream, Version.ToString());
            IPCHelper.Write(stream, BindingMode.ToString());
            IPCHelper.Write(stream, SMSNumber);
            if (SupportedTypes == null)
                IPCHelper.Write(stream, (int)-1);
            else
                SupportedTypes.Serialise(stream);           
            stream.Flush();
        }

        public static Client Deserialise(Stream stream)
        {
            Client result = new Client();
            result.ClientID = IPCHelper.ReadGuid(stream);
            result.Name = IPCHelper.ReadString(stream);
            DateTime? dateTime = IPCHelper.ReadDateTime(stream);
            if (dateTime.HasValue)
                result.Lifetime = dateTime.Value;
            string version = IPCHelper.ReadString(stream);
            result.Version = Version.Parse(version);
            string bindingModeText = IPCHelper.ReadString(stream);
            TBindingMode bindingMode;
            if (Enum.TryParse<TBindingMode>(bindingModeText,true, out bindingMode))
                result.BindingMode = bindingMode;
            result.SMSNumber = IPCHelper.ReadString(stream);
            result.SupportedTypes = ObjectTypes.Deserialise(stream);
            return result;
        }

	}
}

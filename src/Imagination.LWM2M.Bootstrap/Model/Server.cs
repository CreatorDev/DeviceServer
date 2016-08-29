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

using Imagination.LWM2M;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Imagination.Model
{
	internal class Server : ITlvSerialisable
	{
        public int ShortServerID { get; set; }
		public uint Lifetime { get; set; }
		public uint? DefaultMinimumPeriod { get; set; }
		public uint? DefaultMaximumPeriod { get; set; }
		//public uint Disable { get; set; }
		public uint? DisableTimeout { get; set; }
		public bool NotificationStoringWhenOffline { get; set; }
		public TBindingMode Binding { get; set; }
		//public uint RegistrationUpdateTrigger { get; set; }

		public List<Security> EndPoints { get; private set; }

		private enum ResourceID
		{
			ShortServerID = 0,
			Lifetime = 1,
			DefaultMinimumPeriod = 2,
			DefaultMaximumPeriod = 3,
			Disable = 4,
			DisableTimeout = 5,
			NotificationStoringWhenOffline = 6,
			Binding = 7,
			RegistrationUpdateTrigger = 8,
		}

		public Server()
		{
			EndPoints = new List<Security>();
		}

        public Server(LWM2MServer lwm2mServer)
        {
            Lifetime = lwm2mServer.Lifetime;
            DefaultMinimumPeriod = lwm2mServer.DefaultMinimumPeriod;
            DefaultMaximumPeriod = lwm2mServer.DefaultMaximumPeriod;
            DisableTimeout = lwm2mServer.DisableTimeout;
            NotificationStoringWhenOffline = lwm2mServer.NotificationStoringWhenOffline;
            Binding = lwm2mServer.Binding;
            EndPoints = new List<Security>();
            if (!string.IsNullOrEmpty(lwm2mServer.Url))
            {
                Security security = new Security();
                security.ServerURI = lwm2mServer.Url;
                if (lwm2mServer.Url.StartsWith("coaps"))
                {
                    security.SecurityMode = TSecurityMode.Certificate;
                }
                else
                    security.SecurityMode = TSecurityMode.NoSecurity;
                EndPoints.Add(security);
            }

        }

        public static Server Deserialise(XmlReader reader)
		{
			Server result = new Server();
			string value;
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.Name)
					{
						case "ShortServerID":
							value = reader.ReadInnerXml();
							int shortServerID;
							if (int.TryParse(value, out shortServerID))
								result.ShortServerID = shortServerID;
							break;
						case "Lifetime":
							value = reader.ReadInnerXml();
							uint lifetime;
							if (uint.TryParse(value, out lifetime))
								result.Lifetime = lifetime;
							break;
						case "DefaultMinimumPeriod":
							value = reader.ReadInnerXml();
							uint defaultMinimumPeriod;
							if (uint.TryParse(value, out defaultMinimumPeriod))
								result.DefaultMinimumPeriod = defaultMinimumPeriod;
							break;
						case "DefaultMaximumPeriod":
							value = reader.ReadInnerXml();
							uint defaultMaximumPeriod;
							if (uint.TryParse(value, out defaultMaximumPeriod))
								result.DefaultMaximumPeriod = defaultMaximumPeriod;
							break;
						case "DisableTimeout":
							value = reader.ReadInnerXml();
							uint disableTimeout;
							if (uint.TryParse(value, out disableTimeout))
								result.DisableTimeout = disableTimeout;
							break;
						case "NotificationStoringWhenOffline":
							value = reader.ReadInnerXml();
							bool notificationStoringWhenOffline;
							if (bool.TryParse(value, out notificationStoringWhenOffline))
								result.NotificationStoringWhenOffline = notificationStoringWhenOffline;
							break;
						case "Binding":
							value = reader.ReadInnerXml();
							if (!string.IsNullOrEmpty(value))
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
								result.Binding = bindingMode;
							}
							break;
						case "EndPointSecurity":
							Security security = Security.Deserialise(reader.ReadSubtree());
							result.EndPoints.Add(security);
							break;
						default:
							break;
					}
				}
			}
			return result;
		}


		/*

		public bool NotificationStoringWhenOffline { get; set; }
		public TBindingMode Binding { get; set; }
		*/

		public void Serialise(TlvWriter writer)
		{
			writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.ShortServerID, ShortServerID);
			if (Lifetime > 0)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.Lifetime, Lifetime);
			}
			if (DefaultMinimumPeriod.HasValue)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.DefaultMinimumPeriod, DefaultMinimumPeriod.Value);
			}
			if (DefaultMaximumPeriod.HasValue)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.DefaultMaximumPeriod, DefaultMaximumPeriod.Value);
			}
			if (DisableTimeout.HasValue)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.DisableTimeout, DisableTimeout.Value);
			}
			writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.NotificationStoringWhenOffline, NotificationStoringWhenOffline);
			string binding = null;
			switch (Binding)
			{
				case TBindingMode.NotSet:
					break;
				case TBindingMode.UDP:
					binding = "U";
					break;
				case TBindingMode.QueuedUDP:
					binding = "UQ";
					break;
				case TBindingMode.SMS:
					binding = "S";
					break;
				case TBindingMode.QueuedSMS:
					binding = "SQ";
					break;
				case TBindingMode.UDPSMS:
					binding = "US";
					break;
				case TBindingMode.QueuedUDPSMS:
					binding = "UQS";
					break;
				default:
					break;
			}
			writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.Binding, binding);
		}

	}
}

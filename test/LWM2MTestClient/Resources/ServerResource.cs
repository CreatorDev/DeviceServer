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
using CoAP;
using CoAP.Server.Resources;
using Imagination.LWM2M;
using Imagination.Model;

namespace Imagination.LWM2M.Resources
{
	internal class ServerResource : LWM2MResource
	{
		public int ShortServerID { get; set; }
		public int Lifetime { get; set; }
		public int? DefaultMinimumPeriod { get; set; }
		public int? DefaultMaximumPeriod { get; set; }
		//public uint Disable { get; set; }
		public int? DisableTimeout { get; set; }
		public bool NotificationStoringWhenOffline { get; set; }
		public TBindingMode Binding { get; set; }
		//public uint RegistrationUpdateTrigger { get; set; }

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

		public ServerResource(String name)
			: base(name, false)
		{ }

		public static ServerResource Deserialise(Request request)
		{
			ServerResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new ServerResource(name);
				using (TlvReader reader = new TlvReader(request.Payload))
				{
					result.Deserialise(reader);
				}
			}
			return result;
		}

		public override bool Deserialise(TlvReader reader)
		{
			bool result = false;
			while (reader.Read())
			{
                if (reader.TlvRecord.TypeIdentifier == TTlvTypeIdentifier.ObjectInstance)
                {
                    if (string.Compare(this.Name, reader.TlvRecord.Identifier.ToString()) != 0)
                    {
                        this.Name = reader.TlvRecord.Identifier.ToString();
                    }
                    reader = new TlvReader(reader.TlvRecord.Value);
                }
                else if ((reader.TlvRecord.TypeIdentifier != TTlvTypeIdentifier.ObjectInstance) && (reader.TlvRecord.TypeIdentifier != TTlvTypeIdentifier.NotSet))
				{
					switch ((ResourceID)reader.TlvRecord.Identifier)
					{
						case ResourceID.ShortServerID:
							this.ShortServerID = reader.TlvRecord.ValueAsInt32();
							result = true;
							break;
						case ResourceID.Lifetime:
							this.Lifetime = reader.TlvRecord.ValueAsInt32();
							result = true;
							break;
						case ResourceID.DefaultMinimumPeriod:
							this.DefaultMinimumPeriod = reader.TlvRecord.ValueAsInt32();
							result = true;
							break;
						case ResourceID.DefaultMaximumPeriod:
							this.DefaultMaximumPeriod = reader.TlvRecord.ValueAsInt32();
							result = true;
							break;
						case ResourceID.DisableTimeout:
							this.DisableTimeout = reader.TlvRecord.ValueAsInt32();
							result = true;
							break;
						case ResourceID.NotificationStoringWhenOffline:
							this.NotificationStoringWhenOffline = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.Binding:
							TBindingMode binding = TBindingMode.NotSet;
							string bindingText = reader.TlvRecord.ValueAsString();
							switch (bindingText)
							{
								case "U":
									binding = TBindingMode.UDP;
									break;
								case "UQ":
									binding = TBindingMode.QueuedUDP;
									break;
								case "S":
									binding = TBindingMode.SMS;
									break;
								case "SQ":
									binding = TBindingMode.QueuedSMS;
									break;
								case "US":
									binding = TBindingMode.UDPSMS;
									break;
								case "UQS":
									binding = TBindingMode.QueuedUDPSMS;
									break;
								default:
									break;
							}
							this.Binding = binding;
							result = true;
							break;
						default:
							break;
					}
				}
			}
			return result;
		}

		protected override void DoPut(CoapExchange exchange)
		{
			if (exchange.Request.ContentType == TlvConstant.CONTENT_TYPE_TLV)
			{
				using (TlvReader reader = new TlvReader(exchange.Request.Payload))
				{
					this.Deserialise(reader);
				}
				Response response = Response.CreateResponse(exchange.Request, StatusCode.Changed);
				exchange.Respond(response);
			}
			else
				base.DoPut(exchange);
		}

		public override void Serialise(TlvWriter writer)
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

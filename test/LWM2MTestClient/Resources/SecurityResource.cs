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

namespace Imagination.LWM2M.Resources
{
	internal class SecurityResource : LWM2MResource
	{
		public string ServerURI { get; set; }
		public bool BootstrapServer { get; set; }
		public TSecurityMode SecurityMode { get; set; }
		public byte[] ClientPublicKey { get; set; }
		public byte[] ServerPublicKey { get; set; }
		public byte[] SecretKey { get; set; }
		public TSMSSecurityMode SMSSecurityMode { get; set; }
		public byte[] SMSBindingKeyParameters { get; set; }
		public byte[] SMSBindingSecretKeys { get; set; }
		public long? ServerSMSNumber { get; set; }
		public int ShortServerID { get; set; }
		public int? ClientHoldOffTime { get; set; }


		private enum ResourceID
		{
			ServerURI = 0,
			BootstrapServer = 1,
			SecurityMode = 2,
			ClientPublicKey = 3,
			ServerPublicKey = 4,
			SecretKey = 5,
			SMSSecurityMode = 6,
			SMSBindingKeyParameters = 7,
			SMSBindingSecretKeys = 8,
			ServerSMSNumber = 9,
			ShortServerID = 10,
			ClientHoldOffTime = 11,
		}

		public SecurityResource(String name)
			: base(name, false)
		{ }

		public static SecurityResource Deserialise(Request request)
		{
			SecurityResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new SecurityResource(name);

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
                    if (string.Compare(this.Name , reader.TlvRecord.Identifier.ToString()) != 0 )
                    {
                        this.Name = reader.TlvRecord.Identifier.ToString();
                    }
                    reader = new TlvReader(reader.TlvRecord.Value);
                }
				else if ((reader.TlvRecord.TypeIdentifier != TTlvTypeIdentifier.ObjectInstance) && (reader.TlvRecord.TypeIdentifier != TTlvTypeIdentifier.NotSet))
				{
					switch ((ResourceID)reader.TlvRecord.Identifier)
					{
						case ResourceID.ServerURI:
							this.ServerURI = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.BootstrapServer:
							this.BootstrapServer = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.SecurityMode:
							this.SecurityMode = (TSecurityMode)reader.TlvRecord.ValueAsInt32();
							result = true;
							break;
						case ResourceID.ClientPublicKey:
							this.ClientPublicKey = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.ServerPublicKey:
							this.ServerPublicKey = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.SecretKey:
							this.SecretKey = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.SMSSecurityMode:
							this.SMSSecurityMode = (TSMSSecurityMode)reader.TlvRecord.ValueAsInt32();
							result = true;
							break;
						case ResourceID.SMSBindingKeyParameters:
							this.SMSBindingKeyParameters = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.SMSBindingSecretKeys:
							this.SMSBindingSecretKeys = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.ServerSMSNumber:
							this.ServerSMSNumber = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.ShortServerID:
							this.ShortServerID = reader.TlvRecord.ValueAsInt32();
							result = true;
							break;
						case ResourceID.ClientHoldOffTime:
							this.ClientHoldOffTime = reader.TlvRecord.ValueAsInt32();
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
			if (!string.IsNullOrEmpty(ServerURI))
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.ServerURI, ServerURI);
			}
			writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.BootstrapServer, BootstrapServer);
			writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.SecurityMode, (int)SecurityMode);
			if (ClientPublicKey != null)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.ClientPublicKey, ClientPublicKey);
			}
			if (ServerPublicKey != null)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.ServerPublicKey, ServerPublicKey);
			}
			if (SecretKey != null)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.SecretKey, SecretKey);
			}
			if (SMSSecurityMode != TSMSSecurityMode.NotSet)
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.SMSSecurityMode, (int)SMSSecurityMode);
			if (SMSBindingKeyParameters != null)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.SMSBindingKeyParameters, SMSBindingKeyParameters);
			}
			if (SMSBindingSecretKeys != null)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.SMSBindingSecretKeys, SMSBindingSecretKeys);
			}
			if (ServerSMSNumber.HasValue)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.ServerSMSNumber, ServerSMSNumber.Value);
			}
			if (ShortServerID > 0)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.ShortServerID, ShortServerID);
			}
			if (ClientHoldOffTime.HasValue)
			{
				writer.Write(TTlvTypeIdentifier.ResourceWithValue, (ushort)ResourceID.ClientHoldOffTime, ClientHoldOffTime.Value);
			}
		}

	}
}

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
	internal class Security : ITlvSerialisable
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
		public ulong? ServerSMSNumber { get; set; }
		public int ShortServerID { get; set; }
		public uint? ClientHoldOffTime { get; set; }

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


		public Security()
		{
			SMSSecurityMode = TSMSSecurityMode.NotSet;
		}

		public static Security Deserialise(XmlReader reader)
		{
			Security result = new Security();
			string value;
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.Name)
					{
						case "ServerURI":
							result.ServerURI = reader.ReadInnerXml();
							break;
						case "BootstrapServer":
							value = reader.ReadInnerXml();
							bool bootstrapServer;
							if (bool.TryParse(value, out bootstrapServer))
								result.BootstrapServer = bootstrapServer;
							break;
						case "SecurityMode":
							value = reader.ReadInnerXml();
							TSecurityMode securityMode;
							if (Enum.TryParse<TSecurityMode>(value, true, out securityMode))
								result.SecurityMode = securityMode;
							break;
						case "ClientPublicKey":
							value = reader.ReadInnerXml();						
							if (!string.IsNullOrEmpty(value))
								result.ClientPublicKey = Convert.FromBase64String(value);
							break;
						case "ServerPublicKey":
							value = reader.ReadInnerXml();						
							if (!string.IsNullOrEmpty(value))
								result.ServerPublicKey = Convert.FromBase64String(value);
							break;
						case "SecretKey":
							value = reader.ReadInnerXml();						
							if (!string.IsNullOrEmpty(value))
								result.SecretKey = Convert.FromBase64String(value);
							break;
						case "SMSSecurityMode":
							value = reader.ReadInnerXml();
							TSMSSecurityMode smsSecurityMode;
							if (Enum.TryParse<TSMSSecurityMode>(value, true, out smsSecurityMode))
								result.SMSSecurityMode = smsSecurityMode;
							break;
						case "SMSBindingKeyParameters":
							value = reader.ReadInnerXml();						
							if (!string.IsNullOrEmpty(value))
								result.SMSBindingKeyParameters = Convert.FromBase64String(value);
							break;
						case "SMSBindingSecretKeys":
							value = reader.ReadInnerXml();						
							if (!string.IsNullOrEmpty(value))
								result.SMSBindingSecretKeys = Convert.FromBase64String(value);
							break;
						case "ServerSMSNumber":
							value = reader.ReadInnerXml();
							ulong serverSMSNumber;
							if (ulong.TryParse(value, out serverSMSNumber))
								result.ServerSMSNumber = serverSMSNumber;
							break;
						case "ShortServerID":
							value = reader.ReadInnerXml();
							int shortServerID;
							if (int.TryParse(value, out shortServerID))
								result.ShortServerID = shortServerID;
							break;
						case "ClientHoldOffTime":
							value = reader.ReadInnerXml();
							uint clientHoldOffTime;
							if (uint.TryParse(value, out clientHoldOffTime))
								result.ClientHoldOffTime = clientHoldOffTime;
							break;				
						default:
							break;
					}
				}
			}
			return result;
		}


		public void Serialise(TlvWriter writer)
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


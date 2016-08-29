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
	internal class LWM2MSecurityResource : LWM2MResource
	{
		private enum ResourceID
		{
			LWM2MServerURI = 0,
			BootstrapServer = 1,
			SecurityMode = 2,
			PublicKeyorIdentity = 3,
			ServerPublicKeyorIdentity = 4,
			SecretKey = 5,
			SMSSecurityMode = 6,
			SMSBindingKeyParameters = 7,
			SMSBindingSecretKeys = 8,
			LWM2MServerSMSNumber = 9,
			ShortServerID = 10,
			ClientHoldOffTime = 11,
		}

		private StringResource _LWM2MServerURI;
		private BooleanResource _BootstrapServer;
		private IntegerResource _SecurityMode;
		private OpaqueResource _PublicKeyorIdentity;
		private OpaqueResource _ServerPublicKeyorIdentity;
		private OpaqueResource _SecretKey;
		private IntegerResource _SMSSecurityMode;
		private OpaqueResource _SMSBindingKeyParameters;
		private OpaqueResource _SMSBindingSecretKeys;
		private IntegerResource _LWM2MServerSMSNumber;
		private IntegerResource _ShortServerID;
		private IntegerResource _ClientHoldOffTime;

		public string LWM2MServerURI
		{
			get
			{
				string result = null;
				if (_LWM2MServerURI != null)
					result = _LWM2MServerURI.Value;
				return result;
			}
			set
			{
				if (value == null)
					_LWM2MServerURI = null;
				else
				{
					if (_LWM2MServerURI == null)
					{
						_LWM2MServerURI = new StringResource("0") { Description = "LWM2MServerURI", Value = value};
						Add(_LWM2MServerURI);
					}
					else
						_LWM2MServerURI.Value = value;
				}
			}
		}

		public bool? BootstrapServer
		{
			get
			{
				bool? result = null;
				if (_BootstrapServer != null)
					result = _BootstrapServer.Value;
				return result;
			}
			set
			{
				if (value == null)
					_BootstrapServer = null;
				else
				{
					if (_BootstrapServer == null)
					{
						_BootstrapServer = new BooleanResource("1") { Description = "BootstrapServer", Value = value.Value};
						Add(_BootstrapServer);
					}
					else
						_BootstrapServer.Value = value.Value;
				}
			}
		}

		public long? SecurityMode
		{
			get
			{
				long? result = null;
				if (_SecurityMode != null)
					result = _SecurityMode.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SecurityMode = null;
				else
				{
					if (_SecurityMode == null)
					{
						_SecurityMode = new IntegerResource("2") { Description = "SecurityMode", Value = value.Value};
						Add(_SecurityMode);
					}
					else
						_SecurityMode.Value = value.Value;
				}
			}
		}

		public byte[] PublicKeyorIdentity
		{
			get
			{
				byte[] result = null;
				if (_PublicKeyorIdentity != null)
					result = _PublicKeyorIdentity.Value;
				return result;
			}
			set
			{
				if (value == null)
					_PublicKeyorIdentity = null;
				else
				{
					if (_PublicKeyorIdentity == null)
					{
						_PublicKeyorIdentity = new OpaqueResource("3") { Description = "PublicKeyorIdentity", Value = value};
						Add(_PublicKeyorIdentity);
					}
					else
						_PublicKeyorIdentity.Value = value;
				}
			}
		}

		public byte[] ServerPublicKeyorIdentity
		{
			get
			{
				byte[] result = null;
				if (_ServerPublicKeyorIdentity != null)
					result = _ServerPublicKeyorIdentity.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ServerPublicKeyorIdentity = null;
				else
				{
					if (_ServerPublicKeyorIdentity == null)
					{
						_ServerPublicKeyorIdentity = new OpaqueResource("4") { Description = "ServerPublicKeyorIdentity", Value = value};
						Add(_ServerPublicKeyorIdentity);
					}
					else
						_ServerPublicKeyorIdentity.Value = value;
				}
			}
		}

		public byte[] SecretKey
		{
			get
			{
				byte[] result = null;
				if (_SecretKey != null)
					result = _SecretKey.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SecretKey = null;
				else
				{
					if (_SecretKey == null)
					{
						_SecretKey = new OpaqueResource("5") { Description = "SecretKey", Value = value};
						Add(_SecretKey);
					}
					else
						_SecretKey.Value = value;
				}
			}
		}

		public long? SMSSecurityMode
		{
			get
			{
				long? result = null;
				if (_SMSSecurityMode != null)
					result = _SMSSecurityMode.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SMSSecurityMode = null;
				else
				{
					if (_SMSSecurityMode == null)
					{
						_SMSSecurityMode = new IntegerResource("6") { Description = "SMSSecurityMode", Value = value.Value};
						Add(_SMSSecurityMode);
					}
					else
						_SMSSecurityMode.Value = value.Value;
				}
			}
		}

		public byte[] SMSBindingKeyParameters
		{
			get
			{
				byte[] result = null;
				if (_SMSBindingKeyParameters != null)
					result = _SMSBindingKeyParameters.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SMSBindingKeyParameters = null;
				else
				{
					if (_SMSBindingKeyParameters == null)
					{
						_SMSBindingKeyParameters = new OpaqueResource("7") { Description = "SMSBindingKeyParameters", Value = value};
						Add(_SMSBindingKeyParameters);
					}
					else
						_SMSBindingKeyParameters.Value = value;
				}
			}
		}

		public byte[] SMSBindingSecretKeys
		{
			get
			{
				byte[] result = null;
				if (_SMSBindingSecretKeys != null)
					result = _SMSBindingSecretKeys.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SMSBindingSecretKeys = null;
				else
				{
					if (_SMSBindingSecretKeys == null)
					{
						_SMSBindingSecretKeys = new OpaqueResource("8") { Description = "SMSBindingSecretKeys", Value = value};
						Add(_SMSBindingSecretKeys);
					}
					else
						_SMSBindingSecretKeys.Value = value;
				}
			}
		}

		public long? LWM2MServerSMSNumber
		{
			get
			{
				long? result = null;
				if (_LWM2MServerSMSNumber != null)
					result = _LWM2MServerSMSNumber.Value;
				return result;
			}
			set
			{
				if (value == null)
					_LWM2MServerSMSNumber = null;
				else
				{
					if (_LWM2MServerSMSNumber == null)
					{
						_LWM2MServerSMSNumber = new IntegerResource("9") { Description = "LWM2MServerSMSNumber", Value = value.Value};
						Add(_LWM2MServerSMSNumber);
					}
					else
						_LWM2MServerSMSNumber.Value = value.Value;
				}
			}
		}

		public long? ShortServerID
		{
			get
			{
				long? result = null;
				if (_ShortServerID != null)
					result = _ShortServerID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ShortServerID = null;
				else
				{
					if (_ShortServerID == null)
					{
						_ShortServerID = new IntegerResource("10") { Description = "ShortServerID", Value = value.Value};
						Add(_ShortServerID);
					}
					else
						_ShortServerID.Value = value.Value;
				}
			}
		}

		public long? ClientHoldOffTime
		{
			get
			{
				long? result = null;
				if (_ClientHoldOffTime != null)
					result = _ClientHoldOffTime.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ClientHoldOffTime = null;
				else
				{
					if (_ClientHoldOffTime == null)
					{
						_ClientHoldOffTime = new IntegerResource("11") { Description = "ClientHoldOffTime", Value = value.Value};
						Add(_ClientHoldOffTime);
					}
					else
						_ClientHoldOffTime.Value = value.Value;
				}
			}
		}

		public LWM2MSecurityResource(String name)
			: base(name, true)
		{ }

		public static LWM2MSecurityResource Deserialise(Request request)
		{
			LWM2MSecurityResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new LWM2MSecurityResource(name);
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
				if ((reader.TlvRecord.TypeIdentifier != TTlvTypeIdentifier.ObjectInstance) && (reader.TlvRecord.TypeIdentifier != TTlvTypeIdentifier.NotSet))
				{
					switch ((ResourceID)reader.TlvRecord.Identifier)
					{
						case ResourceID.LWM2MServerURI:
							this.LWM2MServerURI = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.BootstrapServer:
							this.BootstrapServer = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.SecurityMode:
							this.SecurityMode = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.PublicKeyorIdentity:
							this.PublicKeyorIdentity = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.ServerPublicKeyorIdentity:
							this.ServerPublicKeyorIdentity = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.SecretKey:
							this.SecretKey = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.SMSSecurityMode:
							this.SMSSecurityMode = reader.TlvRecord.ValueAsInt64();
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
						case ResourceID.LWM2MServerSMSNumber:
							this.LWM2MServerSMSNumber = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.ShortServerID:
							this.ShortServerID = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.ClientHoldOffTime:
							this.ClientHoldOffTime = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						default:
							break;
					}
				}
			}
			return result;
		}

		public override void Serialise(TlvWriter writer)
		{
			if (_LWM2MServerURI != null)
				_LWM2MServerURI.Serialise(writer);
			if (_BootstrapServer != null)
				_BootstrapServer.Serialise(writer);
			if (_SecurityMode != null)
				_SecurityMode.Serialise(writer);
			if (_PublicKeyorIdentity != null)
				_PublicKeyorIdentity.Serialise(writer);
			if (_ServerPublicKeyorIdentity != null)
				_ServerPublicKeyorIdentity.Serialise(writer);
			if (_SecretKey != null)
				_SecretKey.Serialise(writer);
			if (_SMSSecurityMode != null)
				_SMSSecurityMode.Serialise(writer);
			if (_SMSBindingKeyParameters != null)
				_SMSBindingKeyParameters.Serialise(writer);
			if (_SMSBindingSecretKeys != null)
				_SMSBindingSecretKeys.Serialise(writer);
			if (_LWM2MServerSMSNumber != null)
				_LWM2MServerSMSNumber.Serialise(writer);
			if (_ShortServerID != null)
				_ShortServerID.Serialise(writer);
			if (_ClientHoldOffTime != null)
				_ClientHoldOffTime.Serialise(writer);
		}

	}
}

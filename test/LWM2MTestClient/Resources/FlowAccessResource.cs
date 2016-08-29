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
	internal class FlowAccessResource : LWM2MResource
	{
		private enum ResourceID
		{
			URL = 0,
			CustomerKey = 1,
			CustomerSecret = 2,
			RememberMeToken = 3,
			RememberMeTokenExpiry = 4,
		}

		private StringResource _URL;
		private StringResource _CustomerKey;
		private StringResource _CustomerSecret;
		private StringResource _RememberMeToken;
		private DateTimeResource _RememberMeTokenExpiry;

		public string URL
		{
			get
			{
				string result = null;
				if (_URL != null)
					result = _URL.Value;
				return result;
			}
			set
			{
				if (value == null)
					_URL = null;
				else
				{
					if (_URL == null)
					{
						_URL = new StringResource("0") { Description = "URL", Value = value};
						Add(_URL);
					}
					else
						_URL.Value = value;
				}
			}
		}

		public string CustomerKey
		{
			get
			{
				string result = null;
				if (_CustomerKey != null)
					result = _CustomerKey.Value;
				return result;
			}
			set
			{
				if (value == null)
					_CustomerKey = null;
				else
				{
					if (_CustomerKey == null)
					{
						_CustomerKey = new StringResource("1") { Description = "CustomerKey", Value = value};
						Add(_CustomerKey);
					}
					else
						_CustomerKey.Value = value;
				}
			}
		}

		public string CustomerSecret
		{
			get
			{
				string result = null;
				if (_CustomerSecret != null)
					result = _CustomerSecret.Value;
				return result;
			}
			set
			{
				if (value == null)
					_CustomerSecret = null;
				else
				{
					if (_CustomerSecret == null)
					{
						_CustomerSecret = new StringResource("2") { Description = "CustomerSecret", Value = value};
						Add(_CustomerSecret);
					}
					else
						_CustomerSecret.Value = value;
				}
			}
		}

		public string RememberMeToken
		{
			get
			{
				string result = null;
				if (_RememberMeToken != null)
					result = _RememberMeToken.Value;
				return result;
			}
			set
			{
				if (value == null)
					_RememberMeToken = null;
				else
				{
					if (_RememberMeToken == null)
					{
						_RememberMeToken = new StringResource("3") { Description = "RememberMeToken", Value = value};
						Add(_RememberMeToken);
					}
					else
						_RememberMeToken.Value = value;
				}
			}
		}

		public DateTime? RememberMeTokenExpiry
		{
			get
			{
				DateTime? result = null;
				if (_RememberMeTokenExpiry != null)
					result = _RememberMeTokenExpiry.Value;
				return result;
			}
			set
			{
				if (value == null)
					_RememberMeTokenExpiry = null;
				else
				{
					if (_RememberMeTokenExpiry == null)
					{
						_RememberMeTokenExpiry = new DateTimeResource("4") { Description = "RememberMeTokenExpiry", Value = value.Value};
						Add(_RememberMeTokenExpiry);
					}
					else
						_RememberMeTokenExpiry.Value = value.Value;
				}
			}
		}

		public FlowAccessResource()
			: base("0", true)
		{
        }

		public static FlowAccessResource Deserialise(Request request)
		{
			FlowAccessResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new FlowAccessResource();
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
						case ResourceID.URL:
							this.URL = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.CustomerKey:
							this.CustomerKey = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.CustomerSecret:
							this.CustomerSecret = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.RememberMeToken:
							this.RememberMeToken = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.RememberMeTokenExpiry:
							this.RememberMeTokenExpiry = reader.TlvRecord.ValueAsDateTime();
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
			if (_URL != null)
				_URL.Serialise(writer);
			if (_CustomerKey != null)
				_CustomerKey.Serialise(writer);
			if (_CustomerSecret != null)
				_CustomerSecret.Serialise(writer);
			if (_RememberMeToken != null)
				_RememberMeToken.Serialise(writer);
			if (_RememberMeTokenExpiry != null)
				_RememberMeTokenExpiry.Serialise(writer);
		}

	}
}

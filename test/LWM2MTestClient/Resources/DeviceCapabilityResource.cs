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
	internal class DeviceCapabilityResource : LWM2MResource
	{
		private enum ResourceID
		{
			Property = 0,
			Group = 1,
			Description = 2,
			Attached = 3,
			Enabled = 4,
			opEnable = 5,
			opDisable = 6,
			NotifyEn = 7,
		}

		private StringResource _Property;
		private IntegerResource _Group;
		private StringResource _Description;
		private BooleanResource _Attached;
		private BooleanResource _Enabled;
		private BooleanResource _NotifyEn;

		public string Property
		{
			get
			{
				string result = null;
				if (_Property != null)
					result = _Property.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Property = null;
				else
				{
					if (_Property == null)
					{
						_Property = new StringResource("0") { Description = "Property", Value = value};
						Add(_Property);
					}
					else
						_Property.Value = value;
				}
			}
		}

		public long? Group
		{
			get
			{
				long? result = null;
				if (_Group != null)
					result = _Group.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Group = null;
				else
				{
					if (_Group == null)
					{
						_Group = new IntegerResource("1") { Description = "Group", Value = value.Value};
						Add(_Group);
					}
					else
						_Group.Value = value.Value;
				}
			}
		}

		public string Description
		{
			get
			{
				string result = null;
				if (_Description != null)
					result = _Description.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Description = null;
				else
				{
					if (_Description == null)
					{
						_Description = new StringResource("2") { Description = "Description", Value = value};
						Add(_Description);
					}
					else
						_Description.Value = value;
				}
			}
		}

		public bool? Attached
		{
			get
			{
				bool? result = null;
				if (_Attached != null)
					result = _Attached.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Attached = null;
				else
				{
					if (_Attached == null)
					{
						_Attached = new BooleanResource("3") { Description = "Attached", Value = value.Value};
						Add(_Attached);
					}
					else
						_Attached.Value = value.Value;
				}
			}
		}

		public bool? Enabled
		{
			get
			{
				bool? result = null;
				if (_Enabled != null)
					result = _Enabled.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Enabled = null;
				else
				{
					if (_Enabled == null)
					{
						_Enabled = new BooleanResource("4") { Description = "Enabled", Value = value.Value};
						Add(_Enabled);
					}
					else
						_Enabled.Value = value.Value;
				}
			}
		}

		public bool? NotifyEn
		{
			get
			{
				bool? result = null;
				if (_NotifyEn != null)
					result = _NotifyEn.Value;
				return result;
			}
			set
			{
				if (value == null)
					_NotifyEn = null;
				else
				{
					if (_NotifyEn == null)
					{
						_NotifyEn = new BooleanResource("7") { Description = "NotifyEn", Value = value.Value};
						Add(_NotifyEn);
					}
					else
						_NotifyEn.Value = value.Value;
				}
			}
		}

		public DeviceCapabilityResource(String name)
			: base(name, true)
		{ }

		public static DeviceCapabilityResource Deserialise(Request request)
		{
			DeviceCapabilityResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new DeviceCapabilityResource(name);
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
						case ResourceID.Property:
							this.Property = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Group:
							this.Group = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.Description:
							this.Description = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Attached:
							this.Attached = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.Enabled:
							this.Enabled = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.NotifyEn:
							this.NotifyEn = reader.TlvRecord.ValueAsBoolean();
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
			if (_Property != null)
				_Property.Serialise(writer);
			if (_Group != null)
				_Group.Serialise(writer);
			if (_Description != null)
				_Description.Serialise(writer);
			if (_Attached != null)
				_Attached.Serialise(writer);
			if (_Enabled != null)
				_Enabled.Serialise(writer);
			if (_NotifyEn != null)
				_NotifyEn.Serialise(writer);
		}

	}
}

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
	internal class LWM2MServerResource : LWM2MResource
	{
		private enum ResourceID
		{
			ShortServerID = 0,
			Lifetime = 1,
			DefaultMinimumPeriod = 2,
			DefaultMaximumPeriod = 3,
			Disable = 4,
			DisableTimeout = 5,
			NotificationStoringWhenDisabledorOffline = 6,
			Binding = 7,
			RegistrationUpdateTrigger = 8,
		}

		private IntegerResource _ShortServerID;
		private IntegerResource _Lifetime;
		private IntegerResource _DefaultMinimumPeriod;
		private IntegerResource _DefaultMaximumPeriod;
		private IntegerResource _DisableTimeout;
		private BooleanResource _NotificationStoringWhenDisabledorOffline;
		private StringResource _Binding;

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
						_ShortServerID = new IntegerResource("0") { Description = "ShortServerID", Value = value.Value};
						Add(_ShortServerID);
					}
					else
						_ShortServerID.Value = value.Value;
				}
			}
		}

		public long? Lifetime
		{
			get
			{
				long? result = null;
				if (_Lifetime != null)
					result = _Lifetime.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Lifetime = null;
				else
				{
					if (_Lifetime == null)
					{
						_Lifetime = new IntegerResource("1") { Description = "Lifetime", Value = value.Value};
						Add(_Lifetime);
					}
					else
						_Lifetime.Value = value.Value;
				}
			}
		}

		public long? DefaultMinimumPeriod
		{
			get
			{
				long? result = null;
				if (_DefaultMinimumPeriod != null)
					result = _DefaultMinimumPeriod.Value;
				return result;
			}
			set
			{
				if (value == null)
					_DefaultMinimumPeriod = null;
				else
				{
					if (_DefaultMinimumPeriod == null)
					{
						_DefaultMinimumPeriod = new IntegerResource("2") { Description = "DefaultMinimumPeriod", Value = value.Value};
						Add(_DefaultMinimumPeriod);
					}
					else
						_DefaultMinimumPeriod.Value = value.Value;
				}
			}
		}

		public long? DefaultMaximumPeriod
		{
			get
			{
				long? result = null;
				if (_DefaultMaximumPeriod != null)
					result = _DefaultMaximumPeriod.Value;
				return result;
			}
			set
			{
				if (value == null)
					_DefaultMaximumPeriod = null;
				else
				{
					if (_DefaultMaximumPeriod == null)
					{
						_DefaultMaximumPeriod = new IntegerResource("3") { Description = "DefaultMaximumPeriod", Value = value.Value};
						Add(_DefaultMaximumPeriod);
					}
					else
						_DefaultMaximumPeriod.Value = value.Value;
				}
			}
		}

		public long? DisableTimeout
		{
			get
			{
				long? result = null;
				if (_DisableTimeout != null)
					result = _DisableTimeout.Value;
				return result;
			}
			set
			{
				if (value == null)
					_DisableTimeout = null;
				else
				{
					if (_DisableTimeout == null)
					{
						_DisableTimeout = new IntegerResource("5") { Description = "DisableTimeout", Value = value.Value};
						Add(_DisableTimeout);
					}
					else
						_DisableTimeout.Value = value.Value;
				}
			}
		}

		public bool? NotificationStoringWhenDisabledorOffline
		{
			get
			{
				bool? result = null;
				if (_NotificationStoringWhenDisabledorOffline != null)
					result = _NotificationStoringWhenDisabledorOffline.Value;
				return result;
			}
			set
			{
				if (value == null)
					_NotificationStoringWhenDisabledorOffline = null;
				else
				{
					if (_NotificationStoringWhenDisabledorOffline == null)
					{
						_NotificationStoringWhenDisabledorOffline = new BooleanResource("6") { Description = "NotificationStoringWhenDisabledorOffline", Value = value.Value};
						Add(_NotificationStoringWhenDisabledorOffline);
					}
					else
						_NotificationStoringWhenDisabledorOffline.Value = value.Value;
				}
			}
		}

		public string Binding
		{
			get
			{
				string result = null;
				if (_Binding != null)
					result = _Binding.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Binding = null;
				else
				{
					if (_Binding == null)
					{
						_Binding = new StringResource("7") { Description = "Binding", Value = value};
						Add(_Binding);
					}
					else
						_Binding.Value = value;
				}
			}
		}

		public LWM2MServerResource(String name)
			: base(name, true)
		{ }

		public static LWM2MServerResource Deserialise(Request request)
		{
			LWM2MServerResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new LWM2MServerResource(name);
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
						case ResourceID.ShortServerID:
							this.ShortServerID = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.Lifetime:
							this.Lifetime = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.DefaultMinimumPeriod:
							this.DefaultMinimumPeriod = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.DefaultMaximumPeriod:
							this.DefaultMaximumPeriod = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.DisableTimeout:
							this.DisableTimeout = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.NotificationStoringWhenDisabledorOffline:
							this.NotificationStoringWhenDisabledorOffline = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.Binding:
							this.Binding = reader.TlvRecord.ValueAsString();
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
			if (_ShortServerID != null)
				_ShortServerID.Serialise(writer);
			if (_Lifetime != null)
				_Lifetime.Serialise(writer);
			if (_DefaultMinimumPeriod != null)
				_DefaultMinimumPeriod.Serialise(writer);
			if (_DefaultMaximumPeriod != null)
				_DefaultMaximumPeriod.Serialise(writer);
			if (_DisableTimeout != null)
				_DisableTimeout.Serialise(writer);
			if (_NotificationStoringWhenDisabledorOffline != null)
				_NotificationStoringWhenDisabledorOffline.Serialise(writer);
			if (_Binding != null)
				_Binding.Serialise(writer);
		}

	}
}

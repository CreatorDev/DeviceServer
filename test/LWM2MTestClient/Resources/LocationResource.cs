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
	internal class LocationResource : LWM2MResource
	{
		private enum ResourceID
		{
			Latitude = 0,
			Longitude = 1,
			Altitude = 2,
			Uncertainty = 3,
			Velocity = 4,
			Timestamp = 5,
		}

		private StringResource _Latitude;
		private StringResource _Longitude;
		private StringResource _Altitude;
		private StringResource _Uncertainty;
		private OpaqueResource _Velocity;
		private DateTimeResource _Timestamp;

		public string Latitude
		{
			get
			{
				string result = null;
				if (_Latitude != null)
					result = _Latitude.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Latitude = null;
				else
				{
					if (_Latitude == null)
					{
						_Latitude = new StringResource("0") { Description = "Latitude", Value = value};
						Add(_Latitude);
					}
					else
						_Latitude.Value = value;
				}
			}
		}

		public string Longitude
		{
			get
			{
				string result = null;
				if (_Longitude != null)
					result = _Longitude.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Longitude = null;
				else
				{
					if (_Longitude == null)
					{
						_Longitude = new StringResource("1") { Description = "Longitude", Value = value};
						Add(_Longitude);
					}
					else
						_Longitude.Value = value;
				}
			}
		}

		public string Altitude
		{
			get
			{
				string result = null;
				if (_Altitude != null)
					result = _Altitude.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Altitude = null;
				else
				{
					if (_Altitude == null)
					{
						_Altitude = new StringResource("2") { Description = "Altitude", Value = value};
						Add(_Altitude);
					}
					else
						_Altitude.Value = value;
				}
			}
		}

		public string Uncertainty
		{
			get
			{
				string result = null;
				if (_Uncertainty != null)
					result = _Uncertainty.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Uncertainty = null;
				else
				{
					if (_Uncertainty == null)
					{
						_Uncertainty = new StringResource("3") { Description = "Uncertainty", Value = value};
						Add(_Uncertainty);
					}
					else
						_Uncertainty.Value = value;
				}
			}
		}

		public byte[] Velocity
		{
			get
			{
				byte[] result = null;
				if (_Velocity != null)
					result = _Velocity.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Velocity = null;
				else
				{
					if (_Velocity == null)
					{
						_Velocity = new OpaqueResource("4") { Description = "Velocity", Value = value};
						Add(_Velocity);
					}
					else
						_Velocity.Value = value;
				}
			}
		}

		public DateTime? Timestamp
		{
			get
			{
				DateTime? result = null;
				if (_Timestamp != null)
					result = _Timestamp.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Timestamp = null;
				else
				{
					if (_Timestamp == null)
					{
						_Timestamp = new DateTimeResource("5") { Description = "Timestamp", Value = value.Value};
						Add(_Timestamp);
					}
					else
						_Timestamp.Value = value.Value;
				}
			}
		}

		public LocationResource()
			: base("0", true)
		{ }

		public static LocationResource Deserialise(Request request)
		{
			LocationResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new LocationResource();
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
						case ResourceID.Latitude:
							this.Latitude = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Longitude:
							this.Longitude = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Altitude:
							this.Altitude = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Uncertainty:
							this.Uncertainty = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Velocity:
							this.Velocity = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.Timestamp:
							this.Timestamp = reader.TlvRecord.ValueAsDateTime();
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
			if (_Latitude != null)
				_Latitude.Serialise(writer);
			if (_Longitude != null)
				_Longitude.Serialise(writer);
			if (_Altitude != null)
				_Altitude.Serialise(writer);
			if (_Uncertainty != null)
				_Uncertainty.Serialise(writer);
			if (_Velocity != null)
				_Velocity.Serialise(writer);
			if (_Timestamp != null)
				_Timestamp.Serialise(writer);
		}

	}
}

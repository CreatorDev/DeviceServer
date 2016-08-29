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
	internal class ConnectivityMonitoringResource : LWM2MResource
	{
		private enum ResourceID
		{
			NetworkBearer = 0,
			AvailableNetworkBearers = 1,
			RadioSignalStrength = 2,
			LinkQuality = 3,
			IPAddresses = 4,
			RouterIPAddresses = 5,
			LinkUtilization = 6,
			APNs = 7,
			CellID = 8,
			SMNC = 9,
			SMCC = 10,
		}

		private IntegerResource _NetworkBearer;
		private IntegerResources _AvailableNetworkBearers;
		private IntegerResource _RadioSignalStrength;
		private IntegerResource _LinkQuality;
		private StringResources _IPAddresses;
		private StringResources _RouterIPAddresses;
		private IntegerResource _LinkUtilization;
		private StringResources _APNs;
		private IntegerResource _CellID;
		private IntegerResource _SMNC;
		private IntegerResource _SMCC;

		public long? NetworkBearer
		{
			get
			{
				long? result = null;
				if (_NetworkBearer != null)
					result = _NetworkBearer.Value;
				return result;
			}
			set
			{
				if (value == null)
					_NetworkBearer = null;
				else
				{
					if (_NetworkBearer == null)
					{
						_NetworkBearer = new IntegerResource("0") { Description = "NetworkBearer", Value = value.Value};
						Add(_NetworkBearer);
					}
					else
						_NetworkBearer.Value = value.Value;
				}
			}
		}

		public IntegerResources AvailableNetworkBearers
		{
			get
			{
				return _AvailableNetworkBearers;
			}
			set
			{
				_AvailableNetworkBearers = value;
				if (_AvailableNetworkBearers != null)
				{
					Add(_AvailableNetworkBearers);
					_AvailableNetworkBearers.Name = "1";
					_AvailableNetworkBearers.Description = "AvailableNetworkBearers";
				}
			}
		}

		public long? RadioSignalStrength
		{
			get
			{
				long? result = null;
				if (_RadioSignalStrength != null)
					result = _RadioSignalStrength.Value;
				return result;
			}
			set
			{
				if (value == null)
					_RadioSignalStrength = null;
				else
				{
					if (_RadioSignalStrength == null)
					{
						_RadioSignalStrength = new IntegerResource("2") { Description = "RadioSignalStrength", Value = value.Value};
						Add(_RadioSignalStrength);
					}
					else
						_RadioSignalStrength.Value = value.Value;
				}
			}
		}

		public long? LinkQuality
		{
			get
			{
				long? result = null;
				if (_LinkQuality != null)
					result = _LinkQuality.Value;
				return result;
			}
			set
			{
				if (value == null)
					_LinkQuality = null;
				else
				{
					if (_LinkQuality == null)
					{
						_LinkQuality = new IntegerResource("3") { Description = "LinkQuality", Value = value.Value};
						Add(_LinkQuality);
					}
					else
						_LinkQuality.Value = value.Value;
				}
			}
		}

		public StringResources IPAddresses
		{
			get
			{
				return _IPAddresses;
			}
			set
			{
				_IPAddresses = value;
				if (_IPAddresses != null)
				{
					Add(_IPAddresses);
					_IPAddresses.Name = "4";
					_IPAddresses.Description = "IPAddresses";
				}
			}
		}

		public StringResources RouterIPAddresses
		{
			get
			{
				return _RouterIPAddresses;
			}
			set
			{
				_RouterIPAddresses = value;
				if (_RouterIPAddresses != null)
				{
					Add(_RouterIPAddresses);
					_RouterIPAddresses.Name = "5";
					_RouterIPAddresses.Description = "RouterIPAddresses";
				}
			}
		}

		public long? LinkUtilization
		{
			get
			{
				long? result = null;
				if (_LinkUtilization != null)
					result = _LinkUtilization.Value;
				return result;
			}
			set
			{
				if (value == null)
					_LinkUtilization = null;
				else
				{
					if (_LinkUtilization == null)
					{
						_LinkUtilization = new IntegerResource("6") { Description = "LinkUtilization", Value = value.Value};
						Add(_LinkUtilization);
					}
					else
						_LinkUtilization.Value = value.Value;
				}
			}
		}

		public StringResources APNs
		{
			get
			{
				return _APNs;
			}
			set
			{
				_APNs = value;
				if (_APNs != null)
				{
					Add(_APNs);
					_APNs.Name = "7";
					_APNs.Description = "APNs";
				}
			}
		}

		public long? CellID
		{
			get
			{
				long? result = null;
				if (_CellID != null)
					result = _CellID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_CellID = null;
				else
				{
					if (_CellID == null)
					{
						_CellID = new IntegerResource("8") { Description = "CellID", Value = value.Value};
						Add(_CellID);
					}
					else
						_CellID.Value = value.Value;
				}
			}
		}

		public long? SMNC
		{
			get
			{
				long? result = null;
				if (_SMNC != null)
					result = _SMNC.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SMNC = null;
				else
				{
					if (_SMNC == null)
					{
						_SMNC = new IntegerResource("9") { Description = "SMNC", Value = value.Value};
						Add(_SMNC);
					}
					else
						_SMNC.Value = value.Value;
				}
			}
		}

		public long? SMCC
		{
			get
			{
				long? result = null;
				if (_SMCC != null)
					result = _SMCC.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SMCC = null;
				else
				{
					if (_SMCC == null)
					{
						_SMCC = new IntegerResource("10") { Description = "SMCC", Value = value.Value};
						Add(_SMCC);
					}
					else
						_SMCC.Value = value.Value;
				}
			}
		}

		public ConnectivityMonitoringResource()
			: base("0", true)
		{ }

		public static ConnectivityMonitoringResource Deserialise(Request request)
		{
			ConnectivityMonitoringResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new ConnectivityMonitoringResource();
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
						case ResourceID.NetworkBearer:
							this.NetworkBearer = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.AvailableNetworkBearers:
							this.AvailableNetworkBearers = IntegerResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.RadioSignalStrength:
							this.RadioSignalStrength = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.LinkQuality:
							this.LinkQuality = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.IPAddresses:
							this.IPAddresses = StringResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.RouterIPAddresses:
							this.RouterIPAddresses = StringResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.LinkUtilization:
							this.LinkUtilization = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.APNs:
							this.APNs = StringResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.CellID:
							this.CellID = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.SMNC:
							this.SMNC = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.SMCC:
							this.SMCC = reader.TlvRecord.ValueAsInt64();
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
			if (_NetworkBearer != null)
				_NetworkBearer.Serialise(writer);
			if (_AvailableNetworkBearers != null)
				_AvailableNetworkBearers.Serialise(writer);
			if (_RadioSignalStrength != null)
				_RadioSignalStrength.Serialise(writer);
			if (_LinkQuality != null)
				_LinkQuality.Serialise(writer);
			if (_IPAddresses != null)
				_IPAddresses.Serialise(writer);
			if (_RouterIPAddresses != null)
				_RouterIPAddresses.Serialise(writer);
			if (_LinkUtilization != null)
				_LinkUtilization.Serialise(writer);
			if (_APNs != null)
				_APNs.Serialise(writer);
			if (_CellID != null)
				_CellID.Serialise(writer);
			if (_SMNC != null)
				_SMNC.Serialise(writer);
			if (_SMCC != null)
				_SMCC.Serialise(writer);
		}

	}
}

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
	internal class ConnectivityStatisticsResource : LWM2MResource
	{
		private enum ResourceID
		{
			SMSTxCounter = 0,
			SMSRxCounter = 1,
			TxData = 2,
			RxData = 3,
			MaxMessageSize = 4,
			AverageMessageSize = 5,
			StartOrReset = 6,
		}

		private IntegerResource _SMSTxCounter;
		private IntegerResource _SMSRxCounter;
		private IntegerResource _TxData;
		private IntegerResource _RxData;
		private IntegerResource _MaxMessageSize;
		private IntegerResource _AverageMessageSize;

		public long? SMSTxCounter
		{
			get
			{
				long? result = null;
				if (_SMSTxCounter != null)
					result = _SMSTxCounter.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SMSTxCounter = null;
				else
				{
					if (_SMSTxCounter == null)
					{
						_SMSTxCounter = new IntegerResource("0") { Description = "SMSTxCounter", Value = value.Value};
						Add(_SMSTxCounter);
					}
					else
						_SMSTxCounter.Value = value.Value;
				}
			}
		}

		public long? SMSRxCounter
		{
			get
			{
				long? result = null;
				if (_SMSRxCounter != null)
					result = _SMSRxCounter.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SMSRxCounter = null;
				else
				{
					if (_SMSRxCounter == null)
					{
						_SMSRxCounter = new IntegerResource("1") { Description = "SMSRxCounter", Value = value.Value};
						Add(_SMSRxCounter);
					}
					else
						_SMSRxCounter.Value = value.Value;
				}
			}
		}

		public long? TxData
		{
			get
			{
				long? result = null;
				if (_TxData != null)
					result = _TxData.Value;
				return result;
			}
			set
			{
				if (value == null)
					_TxData = null;
				else
				{
					if (_TxData == null)
					{
						_TxData = new IntegerResource("2") { Description = "TxData", Value = value.Value};
						Add(_TxData);
					}
					else
						_TxData.Value = value.Value;
				}
			}
		}

		public long? RxData
		{
			get
			{
				long? result = null;
				if (_RxData != null)
					result = _RxData.Value;
				return result;
			}
			set
			{
				if (value == null)
					_RxData = null;
				else
				{
					if (_RxData == null)
					{
						_RxData = new IntegerResource("3") { Description = "RxData", Value = value.Value};
						Add(_RxData);
					}
					else
						_RxData.Value = value.Value;
				}
			}
		}

		public long? MaxMessageSize
		{
			get
			{
				long? result = null;
				if (_MaxMessageSize != null)
					result = _MaxMessageSize.Value;
				return result;
			}
			set
			{
				if (value == null)
					_MaxMessageSize = null;
				else
				{
					if (_MaxMessageSize == null)
					{
						_MaxMessageSize = new IntegerResource("4") { Description = "MaxMessageSize", Value = value.Value};
						Add(_MaxMessageSize);
					}
					else
						_MaxMessageSize.Value = value.Value;
				}
			}
		}

		public long? AverageMessageSize
		{
			get
			{
				long? result = null;
				if (_AverageMessageSize != null)
					result = _AverageMessageSize.Value;
				return result;
			}
			set
			{
				if (value == null)
					_AverageMessageSize = null;
				else
				{
					if (_AverageMessageSize == null)
					{
						_AverageMessageSize = new IntegerResource("5") { Description = "AverageMessageSize", Value = value.Value};
						Add(_AverageMessageSize);
					}
					else
						_AverageMessageSize.Value = value.Value;
				}
			}
		}

		public ConnectivityStatisticsResource()
			: base("0", true)
		{ }

		public static ConnectivityStatisticsResource Deserialise(Request request)
		{
			ConnectivityStatisticsResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new ConnectivityStatisticsResource();
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
						case ResourceID.SMSTxCounter:
							this.SMSTxCounter = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.SMSRxCounter:
							this.SMSRxCounter = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.TxData:
							this.TxData = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.RxData:
							this.RxData = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.MaxMessageSize:
							this.MaxMessageSize = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.AverageMessageSize:
							this.AverageMessageSize = reader.TlvRecord.ValueAsInt64();
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
			if (_SMSTxCounter != null)
				_SMSTxCounter.Serialise(writer);
			if (_SMSRxCounter != null)
				_SMSRxCounter.Serialise(writer);
			if (_TxData != null)
				_TxData.Serialise(writer);
			if (_RxData != null)
				_RxData.Serialise(writer);
			if (_MaxMessageSize != null)
				_MaxMessageSize.Serialise(writer);
			if (_AverageMessageSize != null)
				_AverageMessageSize.Serialise(writer);
		}

	}
}

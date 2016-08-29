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
	internal class FlowCommandResource : LWM2MResource
	{
		private enum ResourceID
		{
			CommandID = 0,
			CommandTypeID = 1,
			Status = 2,
			StatusCode = 3,
			ParameterName = 4,
			ParameterValue = 5,
			ResultContentType = 6,
			ResultContent = 7,
			ErrorContentType = 8,
			ErrorContent = 9,
		}

		private OpaqueResource _CommandID;
		private StringResource _CommandTypeID;
		private IntegerResource _Status;
		private StringResource _StatusCode;
		private StringResources _ParameterName;
		private StringResources _ParameterValue;
		private StringResource _ResultContentType;
		private StringResource _ResultContent;
		private StringResource _ErrorContentType;
		private StringResource _ErrorContent;

		public byte[] CommandID
		{
			get
			{
				byte[] result = null;
				if (_CommandID != null)
					result = _CommandID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_CommandID = null;
				else
				{
					if (_CommandID == null)
					{
						_CommandID = new OpaqueResource("0") { Description = "CommandID", Value = value};
						Add(_CommandID);
					}
					else
						_CommandID.Value = value;
				}
			}
		}

		public string CommandTypeID
		{
			get
			{
				string result = null;
				if (_CommandTypeID != null)
					result = _CommandTypeID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_CommandTypeID = null;
				else
				{
					if (_CommandTypeID == null)
					{
						_CommandTypeID = new StringResource("1") { Description = "CommandTypeID", Value = value};
						Add(_CommandTypeID);
					}
					else
						_CommandTypeID.Value = value;
				}
			}
		}

		public long? Status
		{
			get
			{
				long? result = null;
				if (_Status != null)
					result = _Status.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Status = null;
				else
				{
					if (_Status == null)
					{
						_Status = new IntegerResource("2") { Description = "Status", Value = value.Value};
						Add(_Status);
					}
					else
						_Status.Value = value.Value;
				}
			}
		}

		public string StatusCode
		{
			get
			{
				string result = null;
				if (_StatusCode != null)
					result = _StatusCode.Value;
				return result;
			}
			set
			{
				if (value == null)
					_StatusCode = null;
				else
				{
					if (_StatusCode == null)
					{
						_StatusCode = new StringResource("3") { Description = "StatusCode", Value = value};
						Add(_StatusCode);
					}
					else
						_StatusCode.Value = value;
				}
			}
		}

		public StringResources ParameterName
		{
			get
			{
				return _ParameterName;
			}
			set
			{
				_ParameterName = value;
				if (_ParameterName != null)
				{
					Add(_ParameterName);
					_ParameterName.Name = "4";
					_ParameterName.Description = "ParameterName";
				}
			}
		}

		public StringResources ParameterValue
		{
			get
			{
				return _ParameterValue;
			}
			set
			{
				_ParameterValue = value;
				if (_ParameterValue != null)
				{
					Add(_ParameterValue);
					_ParameterValue.Name = "5";
					_ParameterValue.Description = "ParameterValue";
				}
			}
		}

		public string ResultContentType
		{
			get
			{
				string result = null;
				if (_ResultContentType != null)
					result = _ResultContentType.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ResultContentType = null;
				else
				{
					if (_ResultContentType == null)
					{
						_ResultContentType = new StringResource("6") { Description = "ResultContentType", Value = value};
						Add(_ResultContentType);
					}
					else
						_ResultContentType.Value = value;
				}
			}
		}

		public string ResultContent
		{
			get
			{
				string result = null;
				if (_ResultContent != null)
					result = _ResultContent.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ResultContent = null;
				else
				{
					if (_ResultContent == null)
					{
						_ResultContent = new StringResource("7") { Description = "ResultContent", Value = value};
						Add(_ResultContent);
					}
					else
						_ResultContent.Value = value;
				}
			}
		}

		public string ErrorContentType
		{
			get
			{
				string result = null;
				if (_ErrorContentType != null)
					result = _ErrorContentType.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ErrorContentType = null;
				else
				{
					if (_ErrorContentType == null)
					{
						_ErrorContentType = new StringResource("8") { Description = "ErrorContentType", Value = value};
						Add(_ErrorContentType);
					}
					else
						_ErrorContentType.Value = value;
				}
			}
		}

		public string ErrorContent
		{
			get
			{
				string result = null;
				if (_ErrorContent != null)
					result = _ErrorContent.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ErrorContent = null;
				else
				{
					if (_ErrorContent == null)
					{
						_ErrorContent = new StringResource("9") { Description = "ErrorContent", Value = value};
						Add(_ErrorContent);
					}
					else
						_ErrorContent.Value = value;
				}
			}
		}

		public FlowCommandResource(String name)
			: base(name, true)
		{ }

		public static FlowCommandResource Deserialise(Request request)
		{
			FlowCommandResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new FlowCommandResource(name);
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
						case ResourceID.CommandID:
							this.CommandID = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.CommandTypeID:
							this.CommandTypeID = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Status:
							this.Status = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.StatusCode:
							this.StatusCode = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.ParameterName:
							this.ParameterName = StringResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.ParameterValue:
							this.ParameterValue = StringResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.ResultContentType:
							this.ResultContentType = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.ResultContent:
							this.ResultContent = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.ErrorContentType:
							this.ErrorContentType = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.ErrorContent:
							this.ErrorContent = reader.TlvRecord.ValueAsString();
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
			if (_CommandID != null)
				_CommandID.Serialise(writer);
			if (_CommandTypeID != null)
				_CommandTypeID.Serialise(writer);
			if (_Status != null)
				_Status.Serialise(writer);
			if (_StatusCode != null)
				_StatusCode.Serialise(writer);
			if (_ParameterName != null)
				_ParameterName.Serialise(writer);
			if (_ParameterValue != null)
				_ParameterValue.Serialise(writer);
			if (_ResultContentType != null)
				_ResultContentType.Serialise(writer);
			if (_ResultContent != null)
				_ResultContent.Serialise(writer);
			if (_ErrorContentType != null)
				_ErrorContentType.Serialise(writer);
			if (_ErrorContent != null)
				_ErrorContent.Serialise(writer);
		}

	}
}

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
	internal class FlowObjectResource : LWM2MResource
	{
		private enum ResourceID
		{
			DeviceID = 0,
			ParentID = 1,
			DeviceType = 2,
			Name = 3,
			Description = 4,
			FCAP = 5,
			TenantID = 6,
			TenantChallenge = 7,
			HashIterations = 8,
			TenantHash = 9,
			Status = 10,
		}

		private OpaqueResource _DeviceID;
		private OpaqueResource _ParentID;
		private StringResource _DeviceType;
		private StringResource _Name;
		private StringResource _Description;
		private StringResource _FCAP;
		private IntegerResource _TenantID;
		private OpaqueResource _TenantChallenge;
		private IntegerResource _HashIterations;
		private OpaqueResource _TenantHash;
		private IntegerResource _Status;

		public byte[] DeviceID
		{
			get
			{
				byte[] result = null;
				if (_DeviceID != null)
					result = _DeviceID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_DeviceID = null;
				else
				{
					if (_DeviceID == null)
					{
						_DeviceID = new OpaqueResource("0") { Description = "DeviceID", Value = value};
						Add(_DeviceID);
					}
					else
						_DeviceID.Value = value;
				}
			}
		}

		public byte[] ParentID
		{
			get
			{
				byte[] result = null;
				if (_ParentID != null)
					result = _ParentID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ParentID = null;
				else
				{
					if (_ParentID == null)
					{
						_ParentID = new OpaqueResource("1") { Description = "ParentID", Value = value};
						Add(_ParentID);
					}
					else
						_ParentID.Value = value;
				}
			}
		}

		public string DeviceType
		{
			get
			{
				string result = null;
				if (_DeviceType != null)
					result = _DeviceType.Value;
				return result;
			}
			set
			{
				if (value == null)
					_DeviceType = null;
				else
				{
					if (_DeviceType == null)
					{
						_DeviceType = new StringResource("2") { Description = "DeviceType", Value = value};
						Add(_DeviceType);
					}
					else
						_DeviceType.Value = value;
				}
			}
		}

		public string Name
		{
			get
			{
				string result = null;
				if (_Name != null)
					result = _Name.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Name = null;
				else
				{
					if (_Name == null)
					{
						_Name = new StringResource("3") { Description = "Name", Value = value};
						Add(_Name);
					}
					else
						_Name.Value = value;
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
						_Description = new StringResource("4") { Description = "Description", Value = value};
						Add(_Description);
					}
					else
						_Description.Value = value;
				}
			}
		}

		public string FCAP
		{
			get
			{
				string result = null;
				if (_FCAP != null)
					result = _FCAP.Value;
				return result;
			}
			set
			{
				if (value == null)
					_FCAP = null;
				else
				{
					if (_FCAP == null)
					{
						_FCAP = new StringResource("5") { Description = "FCAP", Value = value};
						Add(_FCAP);
					}
					else
						_FCAP.Value = value;
				}
			}
		}

		public long? TenantID
		{
			get
			{
				long? result = null;
				if (_TenantID != null)
					result = _TenantID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_TenantID = null;
				else
				{
					if (_TenantID == null)
					{
						_TenantID = new IntegerResource("6") { Description = "TenantID", Value = value.Value};
						Add(_TenantID);
					}
					else
						_TenantID.Value = value.Value;
				}
			}
		}

		public byte[] TenantChallenge
		{
			get
			{
				byte[] result = null;
				if (_TenantChallenge != null)
					result = _TenantChallenge.Value;
				return result;
			}
			set
			{
				if (value == null)
					_TenantChallenge = null;
				else
				{
					if (_TenantChallenge == null)
					{
						_TenantChallenge = new OpaqueResource("7") { Description = "TenantChallenge", Value = value};
						Add(_TenantChallenge);
					}
					else
						_TenantChallenge.Value = value;
				}
			}
		}

		public long? HashIterations
		{
			get
			{
				long? result = null;
				if (_HashIterations != null)
					result = _HashIterations.Value;
				return result;
			}
			set
			{
				if (value == null)
					_HashIterations = null;
				else
				{
					if (_HashIterations == null)
					{
						_HashIterations = new IntegerResource("8") { Description = "HashIterations", Value = value.Value};
						Add(_HashIterations);
					}
					else
						_HashIterations.Value = value.Value;
				}
			}
		}

		public byte[] TenantHash
		{
			get
			{
				byte[] result = null;
				if (_TenantHash != null)
					result = _TenantHash.Value;
				return result;
			}
			set
			{
				if (value == null)
					_TenantHash = null;
				else
				{
					if (_TenantHash == null)
					{
						_TenantHash = new OpaqueResource("9") { Description = "TenantHash", Value = value};
						Add(_TenantHash);
					}
					else
						_TenantHash.Value = value;
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
						_Status = new IntegerResource("10") { Description = "Status", Value = value.Value};
						Add(_Status);
					}
					else
						_Status.Value = value.Value;
				}
			}
		}

		public FlowObjectResource()
			: base("0", true)
		{ }

		public static FlowObjectResource Deserialise(Request request)
		{
			FlowObjectResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new FlowObjectResource();
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
						case ResourceID.DeviceID:
							this.DeviceID = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.ParentID:
							this.ParentID = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.DeviceType:
							this.DeviceType = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Name:
							this.Name = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Description:
							this.Description = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.FCAP:
							this.FCAP = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.TenantID:
							this.TenantID = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.TenantChallenge:
							this.TenantChallenge = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.HashIterations:
							this.HashIterations = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.TenantHash:
							this.TenantHash = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.Status:
							this.Status = reader.TlvRecord.ValueAsInt64();
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
			if (_DeviceID != null)
				_DeviceID.Serialise(writer);
			if (_ParentID != null)
				_ParentID.Serialise(writer);
			if (_DeviceType != null)
				_DeviceType.Serialise(writer);
			if (_Name != null)
				_Name.Serialise(writer);
			if (_Description != null)
				_Description.Serialise(writer);
			if (_FCAP != null)
				_FCAP.Serialise(writer);
			if (_TenantID != null)
				_TenantID.Serialise(writer);
			if (_TenantChallenge != null)
				_TenantChallenge.Serialise(writer);
			if (_HashIterations != null)
				_HashIterations.Serialise(writer);
			if (_TenantHash != null)
				_TenantHash.Serialise(writer);
			if (_Status != null)
				_Status.Serialise(writer);
		}

	}
}

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
	internal class LWM2MAccessControlResource : LWM2MResource
	{
		private enum ResourceID
		{
			ObjectID = 0,
			ObjectInstanceID = 1,
			ACLs = 2,
			AccessControlOwner = 3,
		}

		private IntegerResource _ObjectID;
		private IntegerResource _ObjectInstanceID;
		private IntegerResources _ACLs;
		private IntegerResource _AccessControlOwner;

		public long? ObjectID
		{
			get
			{
				long? result = null;
				if (_ObjectID != null)
					result = _ObjectID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ObjectID = null;
				else
				{
					if (_ObjectID == null)
					{
						_ObjectID = new IntegerResource("0") { Description = "ObjectID", Value = value.Value};
						Add(_ObjectID);
					}
					else
						_ObjectID.Value = value.Value;
				}
			}
		}

		public long? ObjectInstanceID
		{
			get
			{
				long? result = null;
				if (_ObjectInstanceID != null)
					result = _ObjectInstanceID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ObjectInstanceID = null;
				else
				{
					if (_ObjectInstanceID == null)
					{
						_ObjectInstanceID = new IntegerResource("1") { Description = "ObjectInstanceID", Value = value.Value};
						Add(_ObjectInstanceID);
					}
					else
						_ObjectInstanceID.Value = value.Value;
				}
			}
		}

		public IntegerResources ACLs
		{
			get
			{
				return _ACLs;
			}
			set
			{
				_ACLs = value;
				if (_ACLs != null)
				{
					Add(_ACLs);
					_ACLs.Name = "2";
					_ACLs.Description = "ACLs";
				}
			}
		}

		public long? AccessControlOwner
		{
			get
			{
				long? result = null;
				if (_AccessControlOwner != null)
					result = _AccessControlOwner.Value;
				return result;
			}
			set
			{
				if (value == null)
					_AccessControlOwner = null;
				else
				{
					if (_AccessControlOwner == null)
					{
						_AccessControlOwner = new IntegerResource("3") { Description = "AccessControlOwner", Value = value.Value};
						Add(_AccessControlOwner);
					}
					else
						_AccessControlOwner.Value = value.Value;
				}
			}
		}

		public LWM2MAccessControlResource(String name)
			: base(name, true)
		{ }

		public static LWM2MAccessControlResource Deserialise(Request request)
		{
			LWM2MAccessControlResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new LWM2MAccessControlResource(name);
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
						case ResourceID.ObjectID:
							this.ObjectID = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.ObjectInstanceID:
							this.ObjectInstanceID = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.ACLs:
							this.ACLs = IntegerResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.AccessControlOwner:
							this.AccessControlOwner = reader.TlvRecord.ValueAsInt64();
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
			if (_ObjectID != null)
				_ObjectID.Serialise(writer);
			if (_ObjectInstanceID != null)
				_ObjectInstanceID.Serialise(writer);
			if (_ACLs != null)
				_ACLs.Serialise(writer);
			if (_AccessControlOwner != null)
				_AccessControlOwner.Serialise(writer);
		}

	}
}

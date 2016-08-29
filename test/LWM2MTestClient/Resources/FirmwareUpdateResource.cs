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
	internal class FirmwareUpdateResource : LWM2MResource
	{
		private enum ResourceID
		{
			Package = 0,
			PackageURI = 1,
			Update = 2,
			State = 3,
			UpdateSupportedObjects = 4,
			UpdateResult = 5,
			PackageName = 6,
			PackageVersion = 7,
		}

		private OpaqueResource _Package;
		private StringResource _PackageURI;
		private IntegerResource _State;
		private BooleanResource _UpdateSupportedObjects;
		private IntegerResource _UpdateResult;
		private StringResource _PackageName;
		private StringResource _PackageVersion;

		public byte[] Package
		{
			get
			{
				byte[] result = null;
				if (_Package != null)
					result = _Package.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Package = null;
				else
				{
					if (_Package == null)
					{
						_Package = new OpaqueResource("0") { Description = "Package", Value = value};
						Add(_Package);
					}
					else
						_Package.Value = value;
				}
			}
		}

		public string PackageURI
		{
			get
			{
				string result = null;
				if (_PackageURI != null)
					result = _PackageURI.Value;
				return result;
			}
			set
			{
				if (value == null)
					_PackageURI = null;
				else
				{
					if (_PackageURI == null)
					{
						_PackageURI = new StringResource("1") { Description = "PackageURI", Value = value};
						Add(_PackageURI);
					}
					else
						_PackageURI.Value = value;
				}
			}
		}

		public long? State
		{
			get
			{
				long? result = null;
				if (_State != null)
					result = _State.Value;
				return result;
			}
			set
			{
				if (value == null)
					_State = null;
				else
				{
					if (_State == null)
					{
						_State = new IntegerResource("3") { Description = "State", Value = value.Value};
						Add(_State);
					}
					else
						_State.Value = value.Value;
				}
			}
		}

		public bool? UpdateSupportedObjects
		{
			get
			{
				bool? result = null;
				if (_UpdateSupportedObjects != null)
					result = _UpdateSupportedObjects.Value;
				return result;
			}
			set
			{
				if (value == null)
					_UpdateSupportedObjects = null;
				else
				{
					if (_UpdateSupportedObjects == null)
					{
						_UpdateSupportedObjects = new BooleanResource("4") { Description = "UpdateSupportedObjects", Value = value.Value};
						Add(_UpdateSupportedObjects);
					}
					else
						_UpdateSupportedObjects.Value = value.Value;
				}
			}
		}

		public long? UpdateResult
		{
			get
			{
				long? result = null;
				if (_UpdateResult != null)
					result = _UpdateResult.Value;
				return result;
			}
			set
			{
				if (value == null)
					_UpdateResult = null;
				else
				{
					if (_UpdateResult == null)
					{
						_UpdateResult = new IntegerResource("5") { Description = "UpdateResult", Value = value.Value};
						Add(_UpdateResult);
					}
					else
						_UpdateResult.Value = value.Value;
				}
			}
		}

		public string PackageName
		{
			get
			{
				string result = null;
				if (_PackageName != null)
					result = _PackageName.Value;
				return result;
			}
			set
			{
				if (value == null)
					_PackageName = null;
				else
				{
					if (_PackageName == null)
					{
						_PackageName = new StringResource("6") { Description = "PackageName", Value = value};
						Add(_PackageName);
					}
					else
						_PackageName.Value = value;
				}
			}
		}

		public string PackageVersion
		{
			get
			{
				string result = null;
				if (_PackageVersion != null)
					result = _PackageVersion.Value;
				return result;
			}
			set
			{
				if (value == null)
					_PackageVersion = null;
				else
				{
					if (_PackageVersion == null)
					{
						_PackageVersion = new StringResource("7") { Description = "PackageVersion", Value = value};
						Add(_PackageVersion);
					}
					else
						_PackageVersion.Value = value;
				}
			}
		}

		public FirmwareUpdateResource()
			: base("0", true)
		{ }

		public static FirmwareUpdateResource Deserialise(Request request)
		{
			FirmwareUpdateResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new FirmwareUpdateResource();
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
						case ResourceID.Package:
							this.Package = reader.TlvRecord.Value;
							result = true;
							break;
						case ResourceID.PackageURI:
							this.PackageURI = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.State:
							this.State = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.UpdateSupportedObjects:
							this.UpdateSupportedObjects = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.UpdateResult:
							this.UpdateResult = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.PackageName:
							this.PackageName = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.PackageVersion:
							this.PackageVersion = reader.TlvRecord.ValueAsString();
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
			if (_Package != null)
				_Package.Serialise(writer);
			if (_PackageURI != null)
				_PackageURI.Serialise(writer);
			if (_State != null)
				_State.Serialise(writer);
			if (_UpdateSupportedObjects != null)
				_UpdateSupportedObjects.Serialise(writer);
			if (_UpdateResult != null)
				_UpdateResult.Serialise(writer);
			if (_PackageName != null)
				_PackageName.Serialise(writer);
			if (_PackageVersion != null)
				_PackageVersion.Serialise(writer);
		}

	}
}

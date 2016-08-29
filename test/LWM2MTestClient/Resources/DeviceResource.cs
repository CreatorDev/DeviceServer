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
	internal class DeviceResource : LWM2MResource
	{
		private enum ResourceID
		{
			Manufacturer = 0,
			ModelNumber = 1,
			SerialNumber = 2,
			FirmwareVersion = 3,
			Reboot = 4,
			FactoryReset = 5,
			AvailablePowerSources = 6,
			PowerSourceVoltages = 7,
			PowerSourceCurrents = 8,
			BatteryLevel = 9,
			MemoryFree = 10,
			ErrorCodes = 11,
			ResetErrorCode = 12,
			CurrentTime = 13,
			UTCOffset = 14,
			Timezone = 15,
			SupportedBindingandModes = 16,
			DeviceType = 17,
			HardwareVersion = 18,
			SoftwareVersion = 19,
			BatteryStatus = 20,
			MemoryTotal = 21,
		}

		private StringResource _Manufacturer;
		private StringResource _ModelNumber;
		private StringResource _SerialNumber;
		private StringResource _FirmwareVersion;
        private ExecuteResource _Reboot;
        private ExecuteResource _FactoryReset;
        private IntegerResources _AvailablePowerSources;
		private IntegerResources _PowerSourceVoltages;
		private IntegerResources _PowerSourceCurrents;
		private IntegerResource _BatteryLevel;
		private IntegerResource _MemoryFree;
		private IntegerResources _ErrorCodes;
		private DateTimeResource _CurrentTime;
		private StringResource _UTCOffset;
		private StringResource _Timezone;
		private StringResource _SupportedBindingandModes;
		private StringResource _DeviceType;
		private StringResource _HardwareVersion;
		private StringResource _SoftwareVersion;
		private IntegerResource _BatteryStatus;
		private IntegerResource _MemoryTotal;

		public string Manufacturer
		{
			get
			{
				string result = null;
				if (_Manufacturer != null)
					result = _Manufacturer.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Manufacturer = null;
				else
				{
					if (_Manufacturer == null)
					{
						_Manufacturer = new StringResource("0") { Description = "Manufacturer", Value = value};
						Add(_Manufacturer);
					}
					else
						_Manufacturer.Value = value;
				}
			}
		}

		public string ModelNumber
		{
			get
			{
				string result = null;
				if (_ModelNumber != null)
					result = _ModelNumber.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ModelNumber = null;
				else
				{
					if (_ModelNumber == null)
					{
						_ModelNumber = new StringResource("1") { Description = "ModelNumber", Value = value};
						Add(_ModelNumber);
					}
					else
						_ModelNumber.Value = value;
				}
			}
		}

		public string SerialNumber
		{
			get
			{
				string result = null;
				if (_SerialNumber != null)
					result = _SerialNumber.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SerialNumber = null;
				else
				{
					if (_SerialNumber == null)
					{
						_SerialNumber = new StringResource("2") { Description = "SerialNumber", Value = value};
						Add(_SerialNumber);
					}
					else
						_SerialNumber.Value = value;
				}
			}
		}

		public string FirmwareVersion
		{
			get
			{
				string result = null;
				if (_FirmwareVersion != null)
					result = _FirmwareVersion.Value;
				return result;
			}
			set
			{
				if (value == null)
					_FirmwareVersion = null;
				else
				{
					if (_FirmwareVersion == null)
					{
						_FirmwareVersion = new StringResource("3") { Description = "FirmwareVersion", Value = value};
						Add(_FirmwareVersion);
					}
					else
						_FirmwareVersion.Value = value;
				}
			}
		}

        public ExecuteResource Reboot
        {
            get
            {
                return _Reboot;
            }
        }

        public ExecuteResource FactoryReset
        {
            get
            {
                return _FactoryReset;
            }
        }

        public IntegerResources AvailablePowerSources
		{
			get
			{
				return _AvailablePowerSources;
			}
			set
			{
				_AvailablePowerSources = value;
				if (_AvailablePowerSources != null)
				{
					Add(_AvailablePowerSources);
					_AvailablePowerSources.Name = "6";
					_AvailablePowerSources.Description = "AvailablePowerSources";
				}
			}
		}

		public IntegerResources PowerSourceVoltages
		{
			get
			{
				return _PowerSourceVoltages;
			}
			set
			{
				_PowerSourceVoltages = value;
				if (_PowerSourceVoltages != null)
				{
					Add(_PowerSourceVoltages);
					_PowerSourceVoltages.Name = "7";
					_PowerSourceVoltages.Description = "PowerSourceVoltages";
				}
			}
		}

		public IntegerResources PowerSourceCurrents
		{
			get
			{
				return _PowerSourceCurrents;
			}
			set
			{
				_PowerSourceCurrents = value;
				if (_PowerSourceCurrents != null)
				{
					Add(_PowerSourceCurrents);
					_PowerSourceCurrents.Name = "8";
					_PowerSourceCurrents.Description = "PowerSourceCurrents";
				}
			}
		}

		public long? BatteryLevel
		{
			get
			{
				long? result = null;
				if (_BatteryLevel != null)
					result = _BatteryLevel.Value;
				return result;
			}
			set
			{
				if (value == null)
					_BatteryLevel = null;
				else
				{
					if (_BatteryLevel == null)
					{
						_BatteryLevel = new IntegerResource("9") { Description = "BatteryLevel", Value = value.Value};
						Add(_BatteryLevel);
					}
					else
						_BatteryLevel.Value = value.Value;
				}
			}
		}

		public long? MemoryFree
		{
			get
			{
				long? result = null;
				if (_MemoryFree != null)
					result = _MemoryFree.Value;
				return result;
			}
			set
			{
				if (value == null)
					_MemoryFree = null;
				else
				{
					if (_MemoryFree == null)
					{
						_MemoryFree = new IntegerResource("10") { Description = "MemoryFree", Value = value.Value};
						Add(_MemoryFree);
					}
					else
						_MemoryFree.Value = value.Value;
				}
			}
		}

		public IntegerResources ErrorCodes
		{
			get
			{
				return _ErrorCodes;
			}
			set
			{
				_ErrorCodes = value;
				if (_ErrorCodes != null)
				{
					Add(_ErrorCodes);
					_ErrorCodes.Name = "11";
					_ErrorCodes.Description = "ErrorCodes";
				}
			}
		}

		public DateTime? CurrentTime
		{
			get
			{
				DateTime? result = null;
				if (_CurrentTime != null)
					result = _CurrentTime.Value;
				return result;
			}
			set
			{
				if (value == null)
					_CurrentTime = null;
				else
				{
					if (_CurrentTime == null)
					{
						_CurrentTime = new DateTimeResource("13") { Description = "CurrentTime", Value = value.Value};
						Add(_CurrentTime);
					}
					else
						_CurrentTime.Value = value.Value;
				}
			}
		}

		public string UTCOffset
		{
			get
			{
				string result = null;
				if (_UTCOffset != null)
					result = _UTCOffset.Value;
				return result;
			}
			set
			{
				if (value == null)
					_UTCOffset = null;
				else
				{
					if (_UTCOffset == null)
					{
						_UTCOffset = new StringResource("14") { Description = "UTCOffset", Value = value};
						Add(_UTCOffset);
					}
					else
						_UTCOffset.Value = value;
				}
			}
		}

		public string Timezone
		{
			get
			{
				string result = null;
				if (_Timezone != null)
					result = _Timezone.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Timezone = null;
				else
				{
					if (_Timezone == null)
					{
						_Timezone = new StringResource("15") { Description = "Timezone", Value = value};
						Add(_Timezone);
					}
					else
						_Timezone.Value = value;
				}
			}
		}

		public string SupportedBindingandModes
		{
			get
			{
				string result = null;
				if (_SupportedBindingandModes != null)
					result = _SupportedBindingandModes.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SupportedBindingandModes = null;
				else
				{
					if (_SupportedBindingandModes == null)
					{
						_SupportedBindingandModes = new StringResource("16") { Description = "SupportedBindingandModes", Value = value};
						Add(_SupportedBindingandModes);
					}
					else
						_SupportedBindingandModes.Value = value;
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
						_DeviceType = new StringResource("17") { Description = "DeviceType", Value = value};
						Add(_DeviceType);
					}
					else
						_DeviceType.Value = value;
				}
			}
		}

		public string HardwareVersion
		{
			get
			{
				string result = null;
				if (_HardwareVersion != null)
					result = _HardwareVersion.Value;
				return result;
			}
			set
			{
				if (value == null)
					_HardwareVersion = null;
				else
				{
					if (_HardwareVersion == null)
					{
						_HardwareVersion = new StringResource("18") { Description = "HardwareVersion", Value = value};
						Add(_HardwareVersion);
					}
					else
						_HardwareVersion.Value = value;
				}
			}
		}

		public string SoftwareVersion
		{
			get
			{
				string result = null;
				if (_SoftwareVersion != null)
					result = _SoftwareVersion.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SoftwareVersion = null;
				else
				{
					if (_SoftwareVersion == null)
					{
						_SoftwareVersion = new StringResource("19") { Description = "SoftwareVersion", Value = value};
						Add(_SoftwareVersion);
					}
					else
						_SoftwareVersion.Value = value;
				}
			}
		}

		public long? BatteryStatus
		{
			get
			{
				long? result = null;
				if (_BatteryStatus != null)
					result = _BatteryStatus.Value;
				return result;
			}
			set
			{
				if (value == null)
					_BatteryStatus = null;
				else
				{
					if (_BatteryStatus == null)
					{
						_BatteryStatus = new IntegerResource("20") { Description = "BatteryStatus", Value = value.Value};
						Add(_BatteryStatus);
					}
					else
						_BatteryStatus.Value = value.Value;
				}
			}
		}

		public long? MemoryTotal
		{
			get
			{
				long? result = null;
				if (_MemoryTotal != null)
					result = _MemoryTotal.Value;
				return result;
			}
			set
			{
				if (value == null)
					_MemoryTotal = null;
				else
				{
					if (_MemoryTotal == null)
					{
						_MemoryTotal = new IntegerResource("21") { Description = "MemoryTotal", Value = value.Value};
						Add(_MemoryTotal);
					}
					else
						_MemoryTotal.Value = value.Value;
				}
			}
		}

        public DeviceResource()
            : base("0", true)
        {
            _Reboot = new ExecuteResource("4");
            Add(_Reboot);
            _FactoryReset = new ExecuteResource("5");
            Add(_FactoryReset);
        }

        public static DeviceResource Deserialise(Request request)
		{
			DeviceResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new DeviceResource();
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
						case ResourceID.Manufacturer:
							this.Manufacturer = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.ModelNumber:
							this.ModelNumber = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.SerialNumber:
							this.SerialNumber = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.FirmwareVersion:
							this.FirmwareVersion = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.AvailablePowerSources:
							this.AvailablePowerSources = IntegerResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.PowerSourceVoltages:
							this.PowerSourceVoltages = IntegerResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.PowerSourceCurrents:
							this.PowerSourceCurrents = IntegerResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.BatteryLevel:
							this.BatteryLevel = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.MemoryFree:
							this.MemoryFree = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.ErrorCodes:
							this.ErrorCodes = IntegerResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.CurrentTime:
							this.CurrentTime = reader.TlvRecord.ValueAsDateTime();
							result = true;
							break;
						case ResourceID.UTCOffset:
							this.UTCOffset = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Timezone:
							this.Timezone = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.SupportedBindingandModes:
							this.SupportedBindingandModes = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.DeviceType:
							this.DeviceType = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.HardwareVersion:
							this.HardwareVersion = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.SoftwareVersion:
							this.SoftwareVersion = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.BatteryStatus:
							this.BatteryStatus = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.MemoryTotal:
							this.MemoryTotal = reader.TlvRecord.ValueAsInt64();
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
			if (_Manufacturer != null)
				_Manufacturer.Serialise(writer);
			if (_ModelNumber != null)
				_ModelNumber.Serialise(writer);
			if (_SerialNumber != null)
				_SerialNumber.Serialise(writer);
			if (_FirmwareVersion != null)
				_FirmwareVersion.Serialise(writer);
			if (_AvailablePowerSources != null)
				_AvailablePowerSources.Serialise(writer);
			if (_PowerSourceVoltages != null)
				_PowerSourceVoltages.Serialise(writer);
			if (_PowerSourceCurrents != null)
				_PowerSourceCurrents.Serialise(writer);
			if (_BatteryLevel != null)
				_BatteryLevel.Serialise(writer);
			if (_MemoryFree != null)
				_MemoryFree.Serialise(writer);
			if (_ErrorCodes != null)
				_ErrorCodes.Serialise(writer);
			if (_CurrentTime != null)
				_CurrentTime.Serialise(writer);
			if (_UTCOffset != null)
				_UTCOffset.Serialise(writer);
			if (_Timezone != null)
				_Timezone.Serialise(writer);
			if (_SupportedBindingandModes != null)
				_SupportedBindingandModes.Serialise(writer);
			if (_DeviceType != null)
				_DeviceType.Serialise(writer);
			if (_HardwareVersion != null)
				_HardwareVersion.Serialise(writer);
			if (_SoftwareVersion != null)
				_SoftwareVersion.Serialise(writer);
			if (_BatteryStatus != null)
				_BatteryStatus.Serialise(writer);
			if (_MemoryTotal != null)
				_MemoryTotal.Serialise(writer);
		}

	}
}

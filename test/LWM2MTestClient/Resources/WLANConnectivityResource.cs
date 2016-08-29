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
	internal class WLANConnectivityResource : LWM2MResource
	{
		private enum ResourceID
		{
			InterfaceName = 0,
			Enable = 1,
			RadioEnabled = 2,
			Status = 3,
			BSSID = 4,
			SSID = 5,
			BroadcastSSID = 6,
			BeaconEnabled = 7,
			Mode = 8,
			Channel = 9,
			SupportedChannels = 11,
			ChannelsInUse = 12,
			RegulatoryDomain = 13,
			Standard = 14,
			AuthenticationMode = 15,
			EncryptionMode = 16,
			WPAPreSharedKey = 17,
			WPAKeyPhrase = 18,
			WEPEncryptionType = 19,
			WEPKeyIndex = 20,
			WEPKeyPhrase = 21,
			WEPKey1 = 22,
			WEPKey2 = 23,
			WEPKey3 = 24,
			WEPKey4 = 25,
			RADIUSServer = 26,
			RADIUSServerPort = 27,
			RADIUSSecret = 28,
			WMMSupported = 29,
			WMMEnabled = 30,
			MACControlEnabled = 31,
			MACAddressList = 32,
			TotalBytesSent = 33,
			TotalBytesReceived = 34,
			TotalPacketsSent = 35,
			TotalPacketsReceived = 36,
			TransmitErrors = 37,
			ReceiveErrors = 38,
			UnicastPacketsSent = 39,
			UnicastPacketsReceived = 40,
			MulticastPacketsSent = 41,
			MulticastPacketsReceived = 42,
			BroadcastPacketsSent = 43,
			BroadcastPacketsReceived = 44,
			DiscardPacketsSent = 45,
			DiscardPacketsReceived = 46,
			UnknownPacketsReceived = 47,
			VendorSpecificExtensions = 48,
		}

		private StringResource _InterfaceName;
		private BooleanResource _Enable;
		private IntegerResource _RadioEnabled;
		private IntegerResource _Status;
		private StringResource _BSSID;
		private StringResource _SSID;
		private BooleanResource _BroadcastSSID;
		private BooleanResource _BeaconEnabled;
		private IntegerResource _Mode;
		private IntegerResource _Channel;
		private IntegerResources _SupportedChannels;
		private IntegerResources _ChannelsInUse;
		private StringResource _RegulatoryDomain;
		private IntegerResource _Standard;
		private IntegerResource _AuthenticationMode;
		private IntegerResource _EncryptionMode;
		private StringResource _WPAPreSharedKey;
		private StringResource _WPAKeyPhrase;
		private IntegerResource _WEPEncryptionType;
		private IntegerResource _WEPKeyIndex;
		private StringResource _WEPKeyPhrase;
		private StringResource _WEPKey1;
		private StringResource _WEPKey2;
		private StringResource _WEPKey3;
		private StringResource _WEPKey4;
		private StringResource _RADIUSServer;
		private IntegerResource _RADIUSServerPort;
		private StringResource _RADIUSSecret;
		private BooleanResource _WMMSupported;
		private BooleanResource _WMMEnabled;
		private BooleanResource _MACControlEnabled;
		private StringResources _MACAddressList;
		private IntegerResource _TotalBytesSent;
		private IntegerResource _TotalBytesReceived;
		private IntegerResource _TotalPacketsSent;
		private IntegerResource _TotalPacketsReceived;
		private IntegerResource _TransmitErrors;
		private IntegerResource _ReceiveErrors;
		private IntegerResource _UnicastPacketsSent;
		private IntegerResource _UnicastPacketsReceived;
		private IntegerResource _MulticastPacketsSent;
		private IntegerResource _MulticastPacketsReceived;
		private IntegerResource _BroadcastPacketsSent;
		private IntegerResource _BroadcastPacketsReceived;
		private IntegerResource _DiscardPacketsSent;
		private IntegerResource _DiscardPacketsReceived;
		private IntegerResource _UnknownPacketsReceived;
		private StringResource _VendorSpecificExtensions;

		public string InterfaceName
		{
			get
			{
				string result = null;
				if (_InterfaceName != null)
					result = _InterfaceName.Value;
				return result;
			}
			set
			{
				if (value == null)
					_InterfaceName = null;
				else
				{
					if (_InterfaceName == null)
					{
						_InterfaceName = new StringResource("0") { Description = "InterfaceName", Value = value};
						Add(_InterfaceName);
					}
					else
						_InterfaceName.Value = value;
				}
			}
		}

		public bool? Enable
		{
			get
			{
				bool? result = null;
				if (_Enable != null)
					result = _Enable.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Enable = null;
				else
				{
					if (_Enable == null)
					{
						_Enable = new BooleanResource("1") { Description = "Enable", Value = value.Value};
						Add(_Enable);
					}
					else
						_Enable.Value = value.Value;
				}
			}
		}

		public long? RadioEnabled
		{
			get
			{
				long? result = null;
				if (_RadioEnabled != null)
					result = _RadioEnabled.Value;
				return result;
			}
			set
			{
				if (value == null)
					_RadioEnabled = null;
				else
				{
					if (_RadioEnabled == null)
					{
						_RadioEnabled = new IntegerResource("2") { Description = "RadioEnabled", Value = value.Value};
						Add(_RadioEnabled);
					}
					else
						_RadioEnabled.Value = value.Value;
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
						_Status = new IntegerResource("3") { Description = "Status", Value = value.Value};
						Add(_Status);
					}
					else
						_Status.Value = value.Value;
				}
			}
		}

		public string BSSID
		{
			get
			{
				string result = null;
				if (_BSSID != null)
					result = _BSSID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_BSSID = null;
				else
				{
					if (_BSSID == null)
					{
						_BSSID = new StringResource("4") { Description = "BSSID", Value = value};
						Add(_BSSID);
					}
					else
						_BSSID.Value = value;
				}
			}
		}

		public string SSID
		{
			get
			{
				string result = null;
				if (_SSID != null)
					result = _SSID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_SSID = null;
				else
				{
					if (_SSID == null)
					{
						_SSID = new StringResource("5") { Description = "SSID", Value = value};
						Add(_SSID);
					}
					else
						_SSID.Value = value;
				}
			}
		}

		public bool? BroadcastSSID
		{
			get
			{
				bool? result = null;
				if (_BroadcastSSID != null)
					result = _BroadcastSSID.Value;
				return result;
			}
			set
			{
				if (value == null)
					_BroadcastSSID = null;
				else
				{
					if (_BroadcastSSID == null)
					{
						_BroadcastSSID = new BooleanResource("6") { Description = "BroadcastSSID", Value = value.Value};
						Add(_BroadcastSSID);
					}
					else
						_BroadcastSSID.Value = value.Value;
				}
			}
		}

		public bool? BeaconEnabled
		{
			get
			{
				bool? result = null;
				if (_BeaconEnabled != null)
					result = _BeaconEnabled.Value;
				return result;
			}
			set
			{
				if (value == null)
					_BeaconEnabled = null;
				else
				{
					if (_BeaconEnabled == null)
					{
						_BeaconEnabled = new BooleanResource("7") { Description = "BeaconEnabled", Value = value.Value};
						Add(_BeaconEnabled);
					}
					else
						_BeaconEnabled.Value = value.Value;
				}
			}
		}

		public long? Mode
		{
			get
			{
				long? result = null;
				if (_Mode != null)
					result = _Mode.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Mode = null;
				else
				{
					if (_Mode == null)
					{
						_Mode = new IntegerResource("8") { Description = "Mode", Value = value.Value};
						Add(_Mode);
					}
					else
						_Mode.Value = value.Value;
				}
			}
		}

		public long? Channel
		{
			get
			{
				long? result = null;
				if (_Channel != null)
					result = _Channel.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Channel = null;
				else
				{
					if (_Channel == null)
					{
						_Channel = new IntegerResource("9") { Description = "Channel", Value = value.Value};
						Add(_Channel);
					}
					else
						_Channel.Value = value.Value;
				}
			}
		}

		public IntegerResources SupportedChannels
		{
			get
			{
				return _SupportedChannels;
			}
			set
			{
				_SupportedChannels = value;
				if (_SupportedChannels != null)
				{
					Add(_SupportedChannels);
					_SupportedChannels.Name = "11";
					_SupportedChannels.Description = "SupportedChannels";
				}
			}
		}

		public IntegerResources ChannelsInUse
		{
			get
			{
				return _ChannelsInUse;
			}
			set
			{
				_ChannelsInUse = value;
				if (_ChannelsInUse != null)
				{
					Add(_ChannelsInUse);
					_ChannelsInUse.Name = "12";
					_ChannelsInUse.Description = "ChannelsInUse";
				}
			}
		}

		public string RegulatoryDomain
		{
			get
			{
				string result = null;
				if (_RegulatoryDomain != null)
					result = _RegulatoryDomain.Value;
				return result;
			}
			set
			{
				if (value == null)
					_RegulatoryDomain = null;
				else
				{
					if (_RegulatoryDomain == null)
					{
						_RegulatoryDomain = new StringResource("13") { Description = "RegulatoryDomain", Value = value};
						Add(_RegulatoryDomain);
					}
					else
						_RegulatoryDomain.Value = value;
				}
			}
		}

		public long? Standard
		{
			get
			{
				long? result = null;
				if (_Standard != null)
					result = _Standard.Value;
				return result;
			}
			set
			{
				if (value == null)
					_Standard = null;
				else
				{
					if (_Standard == null)
					{
						_Standard = new IntegerResource("14") { Description = "Standard", Value = value.Value};
						Add(_Standard);
					}
					else
						_Standard.Value = value.Value;
				}
			}
		}

		public long? AuthenticationMode
		{
			get
			{
				long? result = null;
				if (_AuthenticationMode != null)
					result = _AuthenticationMode.Value;
				return result;
			}
			set
			{
				if (value == null)
					_AuthenticationMode = null;
				else
				{
					if (_AuthenticationMode == null)
					{
						_AuthenticationMode = new IntegerResource("15") { Description = "AuthenticationMode", Value = value.Value};
						Add(_AuthenticationMode);
					}
					else
						_AuthenticationMode.Value = value.Value;
				}
			}
		}

		public long? EncryptionMode
		{
			get
			{
				long? result = null;
				if (_EncryptionMode != null)
					result = _EncryptionMode.Value;
				return result;
			}
			set
			{
				if (value == null)
					_EncryptionMode = null;
				else
				{
					if (_EncryptionMode == null)
					{
						_EncryptionMode = new IntegerResource("16") { Description = "EncryptionMode", Value = value.Value};
						Add(_EncryptionMode);
					}
					else
						_EncryptionMode.Value = value.Value;
				}
			}
		}

		public string WPAPreSharedKey
		{
			get
			{
				string result = null;
				if (_WPAPreSharedKey != null)
					result = _WPAPreSharedKey.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WPAPreSharedKey = null;
				else
				{
					if (_WPAPreSharedKey == null)
					{
						_WPAPreSharedKey = new StringResource("17") { Description = "WPAPreSharedKey", Value = value};
						Add(_WPAPreSharedKey);
					}
					else
						_WPAPreSharedKey.Value = value;
				}
			}
		}

		public string WPAKeyPhrase
		{
			get
			{
				string result = null;
				if (_WPAKeyPhrase != null)
					result = _WPAKeyPhrase.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WPAKeyPhrase = null;
				else
				{
					if (_WPAKeyPhrase == null)
					{
						_WPAKeyPhrase = new StringResource("18") { Description = "WPAKeyPhrase", Value = value};
						Add(_WPAKeyPhrase);
					}
					else
						_WPAKeyPhrase.Value = value;
				}
			}
		}

		public long? WEPEncryptionType
		{
			get
			{
				long? result = null;
				if (_WEPEncryptionType != null)
					result = _WEPEncryptionType.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WEPEncryptionType = null;
				else
				{
					if (_WEPEncryptionType == null)
					{
						_WEPEncryptionType = new IntegerResource("19") { Description = "WEPEncryptionType", Value = value.Value};
						Add(_WEPEncryptionType);
					}
					else
						_WEPEncryptionType.Value = value.Value;
				}
			}
		}

		public long? WEPKeyIndex
		{
			get
			{
				long? result = null;
				if (_WEPKeyIndex != null)
					result = _WEPKeyIndex.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WEPKeyIndex = null;
				else
				{
					if (_WEPKeyIndex == null)
					{
						_WEPKeyIndex = new IntegerResource("20") { Description = "WEPKeyIndex", Value = value.Value};
						Add(_WEPKeyIndex);
					}
					else
						_WEPKeyIndex.Value = value.Value;
				}
			}
		}

		public string WEPKeyPhrase
		{
			get
			{
				string result = null;
				if (_WEPKeyPhrase != null)
					result = _WEPKeyPhrase.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WEPKeyPhrase = null;
				else
				{
					if (_WEPKeyPhrase == null)
					{
						_WEPKeyPhrase = new StringResource("21") { Description = "WEPKeyPhrase", Value = value};
						Add(_WEPKeyPhrase);
					}
					else
						_WEPKeyPhrase.Value = value;
				}
			}
		}

		public string WEPKey1
		{
			get
			{
				string result = null;
				if (_WEPKey1 != null)
					result = _WEPKey1.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WEPKey1 = null;
				else
				{
					if (_WEPKey1 == null)
					{
						_WEPKey1 = new StringResource("22") { Description = "WEPKey1", Value = value};
						Add(_WEPKey1);
					}
					else
						_WEPKey1.Value = value;
				}
			}
		}

		public string WEPKey2
		{
			get
			{
				string result = null;
				if (_WEPKey2 != null)
					result = _WEPKey2.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WEPKey2 = null;
				else
				{
					if (_WEPKey2 == null)
					{
						_WEPKey2 = new StringResource("23") { Description = "WEPKey2", Value = value};
						Add(_WEPKey2);
					}
					else
						_WEPKey2.Value = value;
				}
			}
		}

		public string WEPKey3
		{
			get
			{
				string result = null;
				if (_WEPKey3 != null)
					result = _WEPKey3.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WEPKey3 = null;
				else
				{
					if (_WEPKey3 == null)
					{
						_WEPKey3 = new StringResource("24") { Description = "WEPKey3", Value = value};
						Add(_WEPKey3);
					}
					else
						_WEPKey3.Value = value;
				}
			}
		}

		public string WEPKey4
		{
			get
			{
				string result = null;
				if (_WEPKey4 != null)
					result = _WEPKey4.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WEPKey4 = null;
				else
				{
					if (_WEPKey4 == null)
					{
						_WEPKey4 = new StringResource("25") { Description = "WEPKey4", Value = value};
						Add(_WEPKey4);
					}
					else
						_WEPKey4.Value = value;
				}
			}
		}

		public string RADIUSServer
		{
			get
			{
				string result = null;
				if (_RADIUSServer != null)
					result = _RADIUSServer.Value;
				return result;
			}
			set
			{
				if (value == null)
					_RADIUSServer = null;
				else
				{
					if (_RADIUSServer == null)
					{
						_RADIUSServer = new StringResource("26") { Description = "RADIUSServer", Value = value};
						Add(_RADIUSServer);
					}
					else
						_RADIUSServer.Value = value;
				}
			}
		}

		public long? RADIUSServerPort
		{
			get
			{
				long? result = null;
				if (_RADIUSServerPort != null)
					result = _RADIUSServerPort.Value;
				return result;
			}
			set
			{
				if (value == null)
					_RADIUSServerPort = null;
				else
				{
					if (_RADIUSServerPort == null)
					{
						_RADIUSServerPort = new IntegerResource("27") { Description = "RADIUSServerPort", Value = value.Value};
						Add(_RADIUSServerPort);
					}
					else
						_RADIUSServerPort.Value = value.Value;
				}
			}
		}

		public string RADIUSSecret
		{
			get
			{
				string result = null;
				if (_RADIUSSecret != null)
					result = _RADIUSSecret.Value;
				return result;
			}
			set
			{
				if (value == null)
					_RADIUSSecret = null;
				else
				{
					if (_RADIUSSecret == null)
					{
						_RADIUSSecret = new StringResource("28") { Description = "RADIUSSecret", Value = value};
						Add(_RADIUSSecret);
					}
					else
						_RADIUSSecret.Value = value;
				}
			}
		}

		public bool? WMMSupported
		{
			get
			{
				bool? result = null;
				if (_WMMSupported != null)
					result = _WMMSupported.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WMMSupported = null;
				else
				{
					if (_WMMSupported == null)
					{
						_WMMSupported = new BooleanResource("29") { Description = "WMMSupported", Value = value.Value};
						Add(_WMMSupported);
					}
					else
						_WMMSupported.Value = value.Value;
				}
			}
		}

		public bool? WMMEnabled
		{
			get
			{
				bool? result = null;
				if (_WMMEnabled != null)
					result = _WMMEnabled.Value;
				return result;
			}
			set
			{
				if (value == null)
					_WMMEnabled = null;
				else
				{
					if (_WMMEnabled == null)
					{
						_WMMEnabled = new BooleanResource("30") { Description = "WMMEnabled", Value = value.Value};
						Add(_WMMEnabled);
					}
					else
						_WMMEnabled.Value = value.Value;
				}
			}
		}

		public bool? MACControlEnabled
		{
			get
			{
				bool? result = null;
				if (_MACControlEnabled != null)
					result = _MACControlEnabled.Value;
				return result;
			}
			set
			{
				if (value == null)
					_MACControlEnabled = null;
				else
				{
					if (_MACControlEnabled == null)
					{
						_MACControlEnabled = new BooleanResource("31") { Description = "MACControlEnabled", Value = value.Value};
						Add(_MACControlEnabled);
					}
					else
						_MACControlEnabled.Value = value.Value;
				}
			}
		}

		public StringResources MACAddressList
		{
			get
			{
				return _MACAddressList;
			}
			set
			{
				_MACAddressList = value;
				if (_MACAddressList != null)
				{
					Add(_MACAddressList);
					_MACAddressList.Name = "32";
					_MACAddressList.Description = "MACAddressList";
				}
			}
		}

		public long? TotalBytesSent
		{
			get
			{
				long? result = null;
				if (_TotalBytesSent != null)
					result = _TotalBytesSent.Value;
				return result;
			}
			set
			{
				if (value == null)
					_TotalBytesSent = null;
				else
				{
					if (_TotalBytesSent == null)
					{
						_TotalBytesSent = new IntegerResource("33") { Description = "TotalBytesSent", Value = value.Value};
						Add(_TotalBytesSent);
					}
					else
						_TotalBytesSent.Value = value.Value;
				}
			}
		}

		public long? TotalBytesReceived
		{
			get
			{
				long? result = null;
				if (_TotalBytesReceived != null)
					result = _TotalBytesReceived.Value;
				return result;
			}
			set
			{
				if (value == null)
					_TotalBytesReceived = null;
				else
				{
					if (_TotalBytesReceived == null)
					{
						_TotalBytesReceived = new IntegerResource("34") { Description = "TotalBytesReceived", Value = value.Value};
						Add(_TotalBytesReceived);
					}
					else
						_TotalBytesReceived.Value = value.Value;
				}
			}
		}

		public long? TotalPacketsSent
		{
			get
			{
				long? result = null;
				if (_TotalPacketsSent != null)
					result = _TotalPacketsSent.Value;
				return result;
			}
			set
			{
				if (value == null)
					_TotalPacketsSent = null;
				else
				{
					if (_TotalPacketsSent == null)
					{
						_TotalPacketsSent = new IntegerResource("35") { Description = "TotalPacketsSent", Value = value.Value};
						Add(_TotalPacketsSent);
					}
					else
						_TotalPacketsSent.Value = value.Value;
				}
			}
		}

		public long? TotalPacketsReceived
		{
			get
			{
				long? result = null;
				if (_TotalPacketsReceived != null)
					result = _TotalPacketsReceived.Value;
				return result;
			}
			set
			{
				if (value == null)
					_TotalPacketsReceived = null;
				else
				{
					if (_TotalPacketsReceived == null)
					{
						_TotalPacketsReceived = new IntegerResource("36") { Description = "TotalPacketsReceived", Value = value.Value};
						Add(_TotalPacketsReceived);
					}
					else
						_TotalPacketsReceived.Value = value.Value;
				}
			}
		}

		public long? TransmitErrors
		{
			get
			{
				long? result = null;
				if (_TransmitErrors != null)
					result = _TransmitErrors.Value;
				return result;
			}
			set
			{
				if (value == null)
					_TransmitErrors = null;
				else
				{
					if (_TransmitErrors == null)
					{
						_TransmitErrors = new IntegerResource("37") { Description = "TransmitErrors", Value = value.Value};
						Add(_TransmitErrors);
					}
					else
						_TransmitErrors.Value = value.Value;
				}
			}
		}

		public long? ReceiveErrors
		{
			get
			{
				long? result = null;
				if (_ReceiveErrors != null)
					result = _ReceiveErrors.Value;
				return result;
			}
			set
			{
				if (value == null)
					_ReceiveErrors = null;
				else
				{
					if (_ReceiveErrors == null)
					{
						_ReceiveErrors = new IntegerResource("38") { Description = "ReceiveErrors", Value = value.Value};
						Add(_ReceiveErrors);
					}
					else
						_ReceiveErrors.Value = value.Value;
				}
			}
		}

		public long? UnicastPacketsSent
		{
			get
			{
				long? result = null;
				if (_UnicastPacketsSent != null)
					result = _UnicastPacketsSent.Value;
				return result;
			}
			set
			{
				if (value == null)
					_UnicastPacketsSent = null;
				else
				{
					if (_UnicastPacketsSent == null)
					{
						_UnicastPacketsSent = new IntegerResource("39") { Description = "UnicastPacketsSent", Value = value.Value};
						Add(_UnicastPacketsSent);
					}
					else
						_UnicastPacketsSent.Value = value.Value;
				}
			}
		}

		public long? UnicastPacketsReceived
		{
			get
			{
				long? result = null;
				if (_UnicastPacketsReceived != null)
					result = _UnicastPacketsReceived.Value;
				return result;
			}
			set
			{
				if (value == null)
					_UnicastPacketsReceived = null;
				else
				{
					if (_UnicastPacketsReceived == null)
					{
						_UnicastPacketsReceived = new IntegerResource("40") { Description = "UnicastPacketsReceived", Value = value.Value};
						Add(_UnicastPacketsReceived);
					}
					else
						_UnicastPacketsReceived.Value = value.Value;
				}
			}
		}

		public long? MulticastPacketsSent
		{
			get
			{
				long? result = null;
				if (_MulticastPacketsSent != null)
					result = _MulticastPacketsSent.Value;
				return result;
			}
			set
			{
				if (value == null)
					_MulticastPacketsSent = null;
				else
				{
					if (_MulticastPacketsSent == null)
					{
						_MulticastPacketsSent = new IntegerResource("41") { Description = "MulticastPacketsSent", Value = value.Value};
						Add(_MulticastPacketsSent);
					}
					else
						_MulticastPacketsSent.Value = value.Value;
				}
			}
		}

		public long? MulticastPacketsReceived
		{
			get
			{
				long? result = null;
				if (_MulticastPacketsReceived != null)
					result = _MulticastPacketsReceived.Value;
				return result;
			}
			set
			{
				if (value == null)
					_MulticastPacketsReceived = null;
				else
				{
					if (_MulticastPacketsReceived == null)
					{
						_MulticastPacketsReceived = new IntegerResource("42") { Description = "MulticastPacketsReceived", Value = value.Value};
						Add(_MulticastPacketsReceived);
					}
					else
						_MulticastPacketsReceived.Value = value.Value;
				}
			}
		}

		public long? BroadcastPacketsSent
		{
			get
			{
				long? result = null;
				if (_BroadcastPacketsSent != null)
					result = _BroadcastPacketsSent.Value;
				return result;
			}
			set
			{
				if (value == null)
					_BroadcastPacketsSent = null;
				else
				{
					if (_BroadcastPacketsSent == null)
					{
						_BroadcastPacketsSent = new IntegerResource("43") { Description = "BroadcastPacketsSent", Value = value.Value};
						Add(_BroadcastPacketsSent);
					}
					else
						_BroadcastPacketsSent.Value = value.Value;
				}
			}
		}

		public long? BroadcastPacketsReceived
		{
			get
			{
				long? result = null;
				if (_BroadcastPacketsReceived != null)
					result = _BroadcastPacketsReceived.Value;
				return result;
			}
			set
			{
				if (value == null)
					_BroadcastPacketsReceived = null;
				else
				{
					if (_BroadcastPacketsReceived == null)
					{
						_BroadcastPacketsReceived = new IntegerResource("44") { Description = "BroadcastPacketsReceived", Value = value.Value};
						Add(_BroadcastPacketsReceived);
					}
					else
						_BroadcastPacketsReceived.Value = value.Value;
				}
			}
		}

		public long? DiscardPacketsSent
		{
			get
			{
				long? result = null;
				if (_DiscardPacketsSent != null)
					result = _DiscardPacketsSent.Value;
				return result;
			}
			set
			{
				if (value == null)
					_DiscardPacketsSent = null;
				else
				{
					if (_DiscardPacketsSent == null)
					{
						_DiscardPacketsSent = new IntegerResource("45") { Description = "DiscardPacketsSent", Value = value.Value};
						Add(_DiscardPacketsSent);
					}
					else
						_DiscardPacketsSent.Value = value.Value;
				}
			}
		}

		public long? DiscardPacketsReceived
		{
			get
			{
				long? result = null;
				if (_DiscardPacketsReceived != null)
					result = _DiscardPacketsReceived.Value;
				return result;
			}
			set
			{
				if (value == null)
					_DiscardPacketsReceived = null;
				else
				{
					if (_DiscardPacketsReceived == null)
					{
						_DiscardPacketsReceived = new IntegerResource("46") { Description = "DiscardPacketsReceived", Value = value.Value};
						Add(_DiscardPacketsReceived);
					}
					else
						_DiscardPacketsReceived.Value = value.Value;
				}
			}
		}

		public long? UnknownPacketsReceived
		{
			get
			{
				long? result = null;
				if (_UnknownPacketsReceived != null)
					result = _UnknownPacketsReceived.Value;
				return result;
			}
			set
			{
				if (value == null)
					_UnknownPacketsReceived = null;
				else
				{
					if (_UnknownPacketsReceived == null)
					{
						_UnknownPacketsReceived = new IntegerResource("47") { Description = "UnknownPacketsReceived", Value = value.Value};
						Add(_UnknownPacketsReceived);
					}
					else
						_UnknownPacketsReceived.Value = value.Value;
				}
			}
		}

		public string VendorSpecificExtensions
		{
			get
			{
				string result = null;
				if (_VendorSpecificExtensions != null)
					result = _VendorSpecificExtensions.Value;
				return result;
			}
			set
			{
				if (value == null)
					_VendorSpecificExtensions = null;
				else
				{
					if (_VendorSpecificExtensions == null)
					{
						_VendorSpecificExtensions = new StringResource("48") { Description = "VendorSpecificExtensions", Value = value};
						Add(_VendorSpecificExtensions);
					}
					else
						_VendorSpecificExtensions.Value = value;
				}
			}
		}

		public WLANConnectivityResource(String name)
			: base(name, true)
		{ }

		public static WLANConnectivityResource Deserialise(Request request)
		{
			WLANConnectivityResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				result = new WLANConnectivityResource(name);
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
						case ResourceID.InterfaceName:
							this.InterfaceName = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Enable:
							this.Enable = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.RadioEnabled:
							this.RadioEnabled = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.Status:
							this.Status = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.BSSID:
							this.BSSID = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.SSID:
							this.SSID = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.BroadcastSSID:
							this.BroadcastSSID = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.BeaconEnabled:
							this.BeaconEnabled = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.Mode:
							this.Mode = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.Channel:
							this.Channel = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.SupportedChannels:
							this.SupportedChannels = IntegerResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.ChannelsInUse:
							this.ChannelsInUse = IntegerResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.RegulatoryDomain:
							this.RegulatoryDomain = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.Standard:
							this.Standard = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.AuthenticationMode:
							this.AuthenticationMode = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.EncryptionMode:
							this.EncryptionMode = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.WPAPreSharedKey:
							this.WPAPreSharedKey = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.WPAKeyPhrase:
							this.WPAKeyPhrase = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.WEPEncryptionType:
							this.WEPEncryptionType = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.WEPKeyIndex:
							this.WEPKeyIndex = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.WEPKeyPhrase:
							this.WEPKeyPhrase = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.WEPKey1:
							this.WEPKey1 = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.WEPKey2:
							this.WEPKey2 = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.WEPKey3:
							this.WEPKey3 = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.WEPKey4:
							this.WEPKey4 = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.RADIUSServer:
							this.RADIUSServer = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.RADIUSServerPort:
							this.RADIUSServerPort = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.RADIUSSecret:
							this.RADIUSSecret = reader.TlvRecord.ValueAsString();
							result = true;
							break;
						case ResourceID.WMMSupported:
							this.WMMSupported = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.WMMEnabled:
							this.WMMEnabled = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.MACControlEnabled:
							this.MACControlEnabled = reader.TlvRecord.ValueAsBoolean();
							result = true;
							break;
						case ResourceID.MACAddressList:
							this.MACAddressList = StringResources.Deserialise(reader);
							result = true;
							break;
						case ResourceID.TotalBytesSent:
							this.TotalBytesSent = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.TotalBytesReceived:
							this.TotalBytesReceived = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.TotalPacketsSent:
							this.TotalPacketsSent = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.TotalPacketsReceived:
							this.TotalPacketsReceived = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.TransmitErrors:
							this.TransmitErrors = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.ReceiveErrors:
							this.ReceiveErrors = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.UnicastPacketsSent:
							this.UnicastPacketsSent = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.UnicastPacketsReceived:
							this.UnicastPacketsReceived = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.MulticastPacketsSent:
							this.MulticastPacketsSent = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.MulticastPacketsReceived:
							this.MulticastPacketsReceived = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.BroadcastPacketsSent:
							this.BroadcastPacketsSent = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.BroadcastPacketsReceived:
							this.BroadcastPacketsReceived = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.DiscardPacketsSent:
							this.DiscardPacketsSent = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.DiscardPacketsReceived:
							this.DiscardPacketsReceived = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.UnknownPacketsReceived:
							this.UnknownPacketsReceived = reader.TlvRecord.ValueAsInt64();
							result = true;
							break;
						case ResourceID.VendorSpecificExtensions:
							//this.VendorSpecificExtensions = reader.TlvRecord.Value;
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
			if (_InterfaceName != null)
				_InterfaceName.Serialise(writer);
			if (_Enable != null)
				_Enable.Serialise(writer);
			if (_RadioEnabled != null)
				_RadioEnabled.Serialise(writer);
			if (_Status != null)
				_Status.Serialise(writer);
			if (_BSSID != null)
				_BSSID.Serialise(writer);
			if (_SSID != null)
				_SSID.Serialise(writer);
			if (_BroadcastSSID != null)
				_BroadcastSSID.Serialise(writer);
			if (_BeaconEnabled != null)
				_BeaconEnabled.Serialise(writer);
			if (_Mode != null)
				_Mode.Serialise(writer);
			if (_Channel != null)
				_Channel.Serialise(writer);
			if (_SupportedChannels != null)
				_SupportedChannels.Serialise(writer);
			if (_ChannelsInUse != null)
				_ChannelsInUse.Serialise(writer);
			if (_RegulatoryDomain != null)
				_RegulatoryDomain.Serialise(writer);
			if (_Standard != null)
				_Standard.Serialise(writer);
			if (_AuthenticationMode != null)
				_AuthenticationMode.Serialise(writer);
			if (_EncryptionMode != null)
				_EncryptionMode.Serialise(writer);
			if (_WPAPreSharedKey != null)
				_WPAPreSharedKey.Serialise(writer);
			if (_WPAKeyPhrase != null)
				_WPAKeyPhrase.Serialise(writer);
			if (_WEPEncryptionType != null)
				_WEPEncryptionType.Serialise(writer);
			if (_WEPKeyIndex != null)
				_WEPKeyIndex.Serialise(writer);
			if (_WEPKeyPhrase != null)
				_WEPKeyPhrase.Serialise(writer);
			if (_WEPKey1 != null)
				_WEPKey1.Serialise(writer);
			if (_WEPKey2 != null)
				_WEPKey2.Serialise(writer);
			if (_WEPKey3 != null)
				_WEPKey3.Serialise(writer);
			if (_WEPKey4 != null)
				_WEPKey4.Serialise(writer);
			if (_RADIUSServer != null)
				_RADIUSServer.Serialise(writer);
			if (_RADIUSServerPort != null)
				_RADIUSServerPort.Serialise(writer);
			if (_RADIUSSecret != null)
				_RADIUSSecret.Serialise(writer);
			if (_WMMSupported != null)
				_WMMSupported.Serialise(writer);
			if (_WMMEnabled != null)
				_WMMEnabled.Serialise(writer);
			if (_MACControlEnabled != null)
				_MACControlEnabled.Serialise(writer);
			if (_MACAddressList != null)
				_MACAddressList.Serialise(writer);
			if (_TotalBytesSent != null)
				_TotalBytesSent.Serialise(writer);
			if (_TotalBytesReceived != null)
				_TotalBytesReceived.Serialise(writer);
			if (_TotalPacketsSent != null)
				_TotalPacketsSent.Serialise(writer);
			if (_TotalPacketsReceived != null)
				_TotalPacketsReceived.Serialise(writer);
			if (_TransmitErrors != null)
				_TransmitErrors.Serialise(writer);
			if (_ReceiveErrors != null)
				_ReceiveErrors.Serialise(writer);
			if (_UnicastPacketsSent != null)
				_UnicastPacketsSent.Serialise(writer);
			if (_UnicastPacketsReceived != null)
				_UnicastPacketsReceived.Serialise(writer);
			if (_MulticastPacketsSent != null)
				_MulticastPacketsSent.Serialise(writer);
			if (_MulticastPacketsReceived != null)
				_MulticastPacketsReceived.Serialise(writer);
			if (_BroadcastPacketsSent != null)
				_BroadcastPacketsSent.Serialise(writer);
			if (_BroadcastPacketsReceived != null)
				_BroadcastPacketsReceived.Serialise(writer);
			if (_DiscardPacketsSent != null)
				_DiscardPacketsSent.Serialise(writer);
			if (_DiscardPacketsReceived != null)
				_DiscardPacketsReceived.Serialise(writer);
			if (_UnknownPacketsReceived != null)
				_UnknownPacketsReceived.Serialise(writer);
			if (_VendorSpecificExtensions != null)
				_VendorSpecificExtensions.Serialise(writer);
		}

	}
}

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
using System.IO;

namespace Imagination.LWM2M
{
	public class TlvWriter
	{
		private Stream _Stream;
		private static DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public TlvWriter(Stream stream)
		{
			_Stream = stream;
		}

		public void WriteType(TTlvTypeIdentifier typeIdentifier, ushort identifier, int length)
		{
			int type = 0;
			byte[] tlvHeader = null;
			int headerLength = 2;
			switch (typeIdentifier)
			{
				case TTlvTypeIdentifier.NotSet:
					break;
				case TTlvTypeIdentifier.ObjectInstance:
					break;
				case TTlvTypeIdentifier.ResourceWithValue:
					type = type | TlvConstant.RESOURCE_WITH_VALUE;
					break;
				case TTlvTypeIdentifier.MultipleResources:
					type = type | TlvConstant.MULTIPLE_RESOURCES;
					break;
				case TTlvTypeIdentifier.ResourceInstance:
					type = type | TlvConstant.RESOURCE_INSTANCE;
					break;
				default:
					break;
			}
			if (identifier > byte.MaxValue)
			{
				type = type | TlvConstant.IDENTIFIER_16BITS;
				headerLength += 1;
			}
			if (length > ushort.MaxValue)
			{
				type = type | TlvConstant.LENGTH_24BIT;
				headerLength += 3;
			}
			else if (length > byte.MaxValue)
			{
				type = type | TlvConstant.LENGTH_16BIT;
				headerLength += 2;
			}
			else if (length > 7)
			{
				type = type | TlvConstant.LENGTH_8BIT;
				headerLength += 1;
			}
			else
			{
				type = type | (int)length;
			}

			tlvHeader = new byte[headerLength];
			tlvHeader[0] = (byte)type;
			int lengthOffset = 2;
			if (identifier > byte.MaxValue)
			{
				NetworkByteOrderConverter.WriteUInt16(tlvHeader, 1, identifier);
				lengthOffset = 3;
			}
			else
			{
				tlvHeader[1] = (byte)identifier;
			}
			if (length > ushort.MaxValue) //24Bit length
			{
				NetworkByteOrderConverter.WriteUInt24(tlvHeader,lengthOffset, (uint)length);
			}
			else if (length > byte.MaxValue) //16Bit length
			{
				NetworkByteOrderConverter.WriteUInt16(tlvHeader, lengthOffset, (ushort)length);
			}
			else if (length > 7) //8Bit length
			{
				tlvHeader[lengthOffset] = (byte)length;
			}
			_Stream.Write(tlvHeader, 0, headerLength);
		}

		public void Write(TTlvTypeIdentifier typeIdentifier, ushort identifier, bool value)
		{
			WriteType(typeIdentifier, identifier, 1);
			if (value)
				_Stream.WriteByte((byte)1);
			else
				_Stream.WriteByte((byte)0);
		}

		public void Write(TTlvTypeIdentifier typeIdentifier, ushort identifier, short value)
		{
			if ((value > 127) || (value < -128)) //16Bit length
			{
				WriteType(typeIdentifier, identifier, 2);
				NetworkByteOrderConverter.WriteInt16(_Stream, (short)value);
			}
			else
			{
				WriteType(typeIdentifier, identifier, 1);
				_Stream.WriteByte((byte)value);
			}
		}

		public void Write(TTlvTypeIdentifier typeIdentifier, ushort identifier, int value)
		{
			if ((value > short.MaxValue) || (value < short.MinValue)) //32Bit length
			{
				WriteType(typeIdentifier, identifier, 4);
				NetworkByteOrderConverter.WriteInt32(_Stream, (int)value);
			}
			else if ((value > 127) || (value < -128)) //16Bit length
			{
				WriteType(typeIdentifier, identifier, 2);
				NetworkByteOrderConverter.WriteInt16(_Stream, (short)value);
			}
			else
			{
				WriteType(typeIdentifier, identifier, 1);
				_Stream.WriteByte((byte)value);
			}
		}

		public void Write(TTlvTypeIdentifier typeIdentifier, ushort identifier, long value)
		{
			if ((value > int.MaxValue) || (value < int.MinValue))
			{
				WriteType(typeIdentifier, identifier, 8);
				NetworkByteOrderConverter.WriteInt64(_Stream, value);
			}
			else if ((value > short.MaxValue) || (value < short.MinValue)) //32Bit length
			{
				WriteType(typeIdentifier, identifier, 4);
				NetworkByteOrderConverter.WriteInt32(_Stream, (int)value);
			}
			else if ((value > 127) || (value < -128)) //16Bit length
			{
				WriteType(typeIdentifier, identifier, 2);
				NetworkByteOrderConverter.WriteInt16(_Stream, (short)value);
			}
			else
			{
				WriteType(typeIdentifier, identifier, 1);
				_Stream.WriteByte((byte)value);
			}
		}

		public void Write(TTlvTypeIdentifier typeIdentifier, ushort identifier, DateTime value)
		{
			TimeSpan diff = value.Subtract(_Epoch);
			long seconds = (long)diff.TotalSeconds;
			Write(typeIdentifier, identifier, seconds);
		}

		public void Write(TTlvTypeIdentifier typeIdentifier, ushort identifier, float value)
		{
			//need IEEE-754 format which C# uses
			byte[] buffer = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                buffer.Reverse();
            Write(typeIdentifier, identifier, buffer);
		}

		public void Write(TTlvTypeIdentifier typeIdentifier, ushort identifier, double value)
		{
			//need IEEE-754 format which C# uses
			byte[] buffer = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                buffer.Reverse();
            Write(typeIdentifier, identifier, buffer);
		}
		
		public void Write(TTlvTypeIdentifier typeIdentifier, ushort identifier, byte[] value)
		{
			if (value == null)
				WriteType(typeIdentifier, identifier, 0);
			else
			{
				WriteType(typeIdentifier, identifier, value.Length);
				_Stream.Write(value, 0, value.Length);
			}
		}

		public void Write(TTlvTypeIdentifier typeIdentifier, ushort identifier, string value)
		{
			byte[] buffer = null;
			if (!string.IsNullOrEmpty(value))
			{
				buffer = Encoding.UTF8.GetBytes(value);
			}
			Write(typeIdentifier, identifier, buffer);
		}

	}
}

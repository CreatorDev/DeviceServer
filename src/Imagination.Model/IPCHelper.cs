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

namespace Imagination
{
	public class IPCHelper
	{

		public static bool ReadBoolean(Stream stream)
		{
			byte value = ReadByte(stream);
			return value != 0x00;
		}		

		public static byte ReadByte(Stream stream)
		{
			int read = stream.ReadByte();
			if (read == -1)
				throw new EndOfStreamException();
			return (byte)read;
		}		

		public static Guid ReadGuid(Stream stream)
		{
			byte[] buffer = new byte[16];
			stream.Read(buffer, 0, buffer.Length);
			return new Guid(buffer);
		}
        
		public static DateTime? ReadDateTime(Stream stream)
		{
			DateTime? result = null;
			byte kind = ReadByte(stream);
			if (kind != 0xFF)
			{
				long ticks = ReadInt64(stream);
				result = new DateTime(ticks, (DateTimeKind)kind);
			}
			return result;
		}

        public static double ReadDouble(Stream stream)
        {
            double result;
            byte[] buffer = new byte[8];
			int read = stream.Read(buffer, 0, 8);
			if (read == 8)
				result = BitConverter.ToDouble(buffer, 0);
			else
               throw new EndOfStreamException();
            return result;
        }

        public static double? ReadDoubleNullable(Stream stream)
        {
            double? result = null;
            byte haveVale = ReadByte(stream);
            if (haveVale == 0xFF)
            {
                result = ReadDouble(stream);
            }
            return result;
        }

		public static int ReadInt32(Stream stream)
		{
			return NetworkByteOrderConverter.ToInt32(stream);
		}

        public static int? ReadInt32Nullable(Stream stream)
        {
            int? result = null;
            byte haveVale = ReadByte(stream);
            if (haveVale == 0xFF)
            {
                result = ReadInt32(stream);
            }
            return result;
        }

		public static long ReadInt64(Stream stream)
		{
			return NetworkByteOrderConverter.ToInt64(stream);
		}

		public static string ReadString(Stream stream)
		{
			string result;
			int length = NetworkByteOrderConverter.ToInt32(stream);
			if (length < 0)
			{
				result = null;
			}
			else
			{
				byte[] buffer = new byte[length];
				stream.Read(buffer, 0, length);
				result = Encoding.UTF8.GetString(buffer);
			}
			return result;
		}

		public static void Write(Stream stream, bool value)
		{
			if (value)
				stream.WriteByte(0xFF);
			else
				stream.WriteByte(0x00);
		}

		public static void Write(Stream stream, byte value)
		{
			stream.WriteByte(value);
		}

		public static void Write(Stream stream, DateTime? value)
		{
			if (value.HasValue)
			{
				Write(stream, (byte)value.Value.Kind);
				Write(stream, value.Value.Ticks);
			}
			else
			{
				Write(stream, (byte)0xFF);
			}
		}

        public static void Write(Stream stream, double? value)
        {
            if (value.HasValue)
            {
                Write(stream, (byte)0xFF);
                byte[] data =  BitConverter.GetBytes(value.Value);
                stream.Write(data, 0, data.Length);
            }
            else
            {
                Write(stream, (byte)0x00);
            }
        }

        public static void Write(Stream stream, int? value)
        {
            if (value.HasValue)
            {
                Write(stream, (byte)0xFF);
                Write(stream, value.Value);
            }
            else
            {
                Write(stream, (byte)0x00);
            }
        }

		public static void Write(Stream stream, int value)
		{
			NetworkByteOrderConverter.WriteInt32(stream, value);
		}

		public static void Write(Stream stream, long value)
		{
			NetworkByteOrderConverter.WriteInt64(stream, value);
		}

		public static void Write(Stream stream, Guid value)
		{
			byte[] buffer = value.ToByteArray();
			stream.Write(buffer, 0, buffer.Length);
		}

		public static void Write(Stream stream, string value)
		{
			if (value == null)
			{
				NetworkByteOrderConverter.WriteInt32(stream, -1);
			}
			else
			{
				byte[] buffer = Encoding.UTF8.GetBytes(value);
				NetworkByteOrderConverter.WriteInt32(stream, buffer.Length);
				stream.Write(buffer, 0, buffer.Length);
			}
		}

	}
}

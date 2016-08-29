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

namespace Imagination.LWM2M
{
	public class TlvRecord
	{
		private TTlvTypeIdentifier _TypeIdentifier;
		private ushort _Identifier;
		private uint _Length;
		private byte[] _Value;
		private static DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public TTlvTypeIdentifier TypeIdentifier 
		{
			get { return _TypeIdentifier; }
			set { _TypeIdentifier = value; }
		}

		public ushort Identifier
		{
			get { return _Identifier; }
			set { _Identifier = value; }
		}

		public uint Length
		{
			get { return _Length; }
			set { _Length = value; }
		}

		public byte[] Value
		{
			get { return _Value; }
			set { _Value = value; }
		}

		public bool ValueAsBoolean()
		{
			bool  result;
			if (_Length > 0)
				result = _Value[0] == 1;
			else
				throw new InvalidCastException();
			return result;
		}

		public DateTime ValueAsDateTime()
		{
			long seconds = 0;
			if (_Length == 1)
				seconds = _Value[0];
			else if (_Length == 2)
				seconds = NetworkByteOrderConverter.ToInt16(_Value, 0);
			else if (_Length == 3)
			{
				byte[] buffer = new byte[4];
				Buffer.BlockCopy(_Value, 0, buffer, 1, 3);
				seconds = NetworkByteOrderConverter.ToInt32(buffer, 0);
			}
			else if (_Length == 4)
				seconds = NetworkByteOrderConverter.ToInt32(_Value, 0);
			else if (_Length == 8)
				seconds = NetworkByteOrderConverter.ToInt64(_Value, 0);
			else
				throw new InvalidCastException();
			return _Epoch.AddSeconds(seconds);
		}
		 
		public short ValueAsInt16()
		{
			short result;
			if (_Length == 1)
				result = _Value[0];
			else if (_Length == 2)
				result = NetworkByteOrderConverter.ToInt16(_Value, 0);
			else
				throw new InvalidCastException();
			return result;
		}

		public int ValueAsInt32()
		{
			int result;
			if (_Length == 1)
				result = _Value[0];
			else if (_Length == 2)
				result = NetworkByteOrderConverter.ToInt16(_Value, 0);
			else if (_Length == 4)
				result = NetworkByteOrderConverter.ToInt32(_Value, 0);
			else
				throw new InvalidCastException();
			return result;
		}

		public long ValueAsInt64()
		{
			long result;
			if (_Length == 1)
				result = _Value[0];
			else if (_Length == 2)
				result = NetworkByteOrderConverter.ToInt16(_Value, 0);
			else if (_Length == 4)
				result = NetworkByteOrderConverter.ToInt32(_Value, 0);
			else if (_Length == 8)
				result = NetworkByteOrderConverter.ToInt64(_Value, 0);
			else
				throw new InvalidCastException();
			return result;
		}

        public float ValueAsSingle()
        {
            byte[] buffer;
            if (BitConverter.IsLittleEndian)
            {
                buffer = new byte[4];
                buffer[0] = _Value[3];
                buffer[1] = _Value[2];
                buffer[2] = _Value[1];
                buffer[3] = _Value[0];
            }
            else
                buffer = _Value;
            return BitConverter.ToSingle(buffer, 0);
		}

		public double ValueAsDouble()
		{
            double result = double.NaN;
            if (_Length == 4)
                result = ValueAsSingle();
            else if (_Length == 8)
            {
                byte[] buffer;
                if (BitConverter.IsLittleEndian)
                {
                    buffer = new byte[8];
                    buffer[0] = _Value[7];
                    buffer[1] = _Value[6];
                    buffer[2] = _Value[5];
                    buffer[3] = _Value[4];
                    buffer[4] = _Value[3];
                    buffer[5] = _Value[2];
                    buffer[6] = _Value[1];
                    buffer[7] = _Value[0];
                }
                else
                    buffer = _Value;
                result = BitConverter.ToDouble(buffer, 0);
            }
            return result;
		}

		public string ValueAsString()
		{
            string result = System.Text.Encoding.UTF8.GetString(_Value);
            int index = result.IndexOf('\0');
            if (index >= 0)
            {
                result = result.Substring(0, index);
            }
            return result;
		}

	}
}

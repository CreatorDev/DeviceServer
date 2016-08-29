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

namespace Imagination
{
	public class Base32Encoder
	{
		private const string DEFAULT_ENCODING_TABLE = "abcdefghijklmnopqrstuvwxyz234567";
		private const char DEFAULT_PADDING_CHARACTER = '=';

		private readonly string _EncodeTable;
		private readonly char _PaddingCharacter;
		private readonly byte[] _DecodeTable;

		public Base32Encoder() : this(DEFAULT_ENCODING_TABLE, DEFAULT_PADDING_CHARACTER) { }
		public Base32Encoder(char padding) : this(DEFAULT_ENCODING_TABLE, padding) { }
		public Base32Encoder(string encodingTable) : this(encodingTable, DEFAULT_PADDING_CHARACTER) { }

		public Base32Encoder(string encodingTable, char padding)
		{
			this._EncodeTable = encodingTable;
			this._PaddingCharacter = padding;
			_DecodeTable = new byte[0x80];
			InitialiseDecodingTable();
		}

		public virtual string Encode(byte[] input)
		{
			var output = new StringBuilder();
			int specialLength = input.Length % 5;
			int normalLength = input.Length - specialLength;
			for (int i = 0; i < normalLength; i += 5)
			{
				int b1 = input[i] & 0xff;
				int b2 = input[i + 1] & 0xff;
				int b3 = input[i + 2] & 0xff;
				int b4 = input[i + 3] & 0xff;
				int b5 = input[i + 4] & 0xff;

				output.Append(_EncodeTable[(b1 >> 3) & 0x1f]);
				output.Append(_EncodeTable[((b1 << 2) | (b2 >> 6)) & 0x1f]);
				output.Append(_EncodeTable[(b2 >> 1) & 0x1f]);
				output.Append(_EncodeTable[((b2 << 4) | (b3 >> 4)) & 0x1f]);
				output.Append(_EncodeTable[((b3 << 1) | (b4 >> 7)) & 0x1f]);
				output.Append(_EncodeTable[(b4 >> 2) & 0x1f]);
				output.Append(_EncodeTable[((b4 << 3) | (b5 >> 5)) & 0x1f]);
				output.Append(_EncodeTable[b5 & 0x1f]);
			}

			switch (specialLength)
			{
				case 1:
					{
						int b1 = input[normalLength] & 0xff;
						output.Append(_EncodeTable[(b1 >> 3) & 0x1f]);
						output.Append(_EncodeTable[(b1 << 2) & 0x1f]);
						output.Append(_PaddingCharacter).Append(_PaddingCharacter).Append(_PaddingCharacter).Append(_PaddingCharacter).Append(_PaddingCharacter).Append(_PaddingCharacter);
						break;
					}

				case 2:
					{
						int b1 = input[normalLength] & 0xff;
						int b2 = input[normalLength + 1] & 0xff;
						output.Append(_EncodeTable[(b1 >> 3) & 0x1f]);
						output.Append(_EncodeTable[((b1 << 2) | (b2 >> 6)) & 0x1f]);
						output.Append(_EncodeTable[(b2 >> 1) & 0x1f]);
						output.Append(_EncodeTable[(b2 << 4) & 0x1f]);
						output.Append(_PaddingCharacter).Append(_PaddingCharacter).Append(_PaddingCharacter).Append(_PaddingCharacter);
						break;
					}
				case 3:
					{
						int b1 = input[normalLength] & 0xff;
						int b2 = input[normalLength + 1] & 0xff;
						int b3 = input[normalLength + 2] & 0xff;
						output.Append(_EncodeTable[(b1 >> 3) & 0x1f]);
						output.Append(_EncodeTable[((b1 << 2) | (b2 >> 6)) & 0x1f]);
						output.Append(_EncodeTable[(b2 >> 1) & 0x1f]);
						output.Append(_EncodeTable[((b2 << 4) | (b3 >> 4)) & 0x1f]);
						output.Append(_EncodeTable[(b3 << 1) & 0x1f]);
						output.Append(_PaddingCharacter).Append(_PaddingCharacter).Append(_PaddingCharacter);
						break;
					}
				case 4:
					{
						int b1 = input[normalLength] & 0xff;
						int b2 = input[normalLength + 1] & 0xff;
						int b3 = input[normalLength + 2] & 0xff;
						int b4 = input[normalLength + 3] & 0xff;
						output.Append(_EncodeTable[(b1 >> 3) & 0x1f]);
						output.Append(_EncodeTable[((b1 << 2) | (b2 >> 6)) & 0x1f]);
						output.Append(_EncodeTable[(b2 >> 1) & 0x1f]);
						output.Append(_EncodeTable[((b2 << 4) | (b3 >> 4)) & 0x1f]);
						output.Append(_EncodeTable[((b3 << 1) | (b4 >> 7)) & 0x1f]);
						output.Append(_EncodeTable[(b4 >> 2) & 0x1f]);
						output.Append(_EncodeTable[(b4 << 3) & 0x1f]);
						output.Append(_PaddingCharacter);
						break;
					}
			}

			return output.ToString();
		}

		virtual public byte[] Decode(string data)
		{
			var outStream = new List<Byte>();

			int length = data.Length;
			while (length > 0)
			{
				if (!this.Ignore(data[length - 1])) break;
				length--;
			}

			int i = 0;
			int finish = length - 8;
			for (i = this.NextI(data, i, finish); i < finish; i = this.NextI(data, i, finish))
			{
				byte b1 = _DecodeTable[data[i++]];
				i = this.NextI(data, i, finish);
				byte b2 = _DecodeTable[data[i++]];
				i = this.NextI(data, i, finish);
				byte b3 = _DecodeTable[data[i++]];
				i = this.NextI(data, i, finish);
				byte b4 = _DecodeTable[data[i++]];
				i = this.NextI(data, i, finish);
				byte b5 = _DecodeTable[data[i++]];
				i = this.NextI(data, i, finish);
				byte b6 = _DecodeTable[data[i++]];
				i = this.NextI(data, i, finish);
				byte b7 = _DecodeTable[data[i++]];
				i = this.NextI(data, i, finish);
				byte b8 = _DecodeTable[data[i++]];

				outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
				outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
				outStream.Add((byte)((b4 << 4) | (b5 >> 1)));
				outStream.Add((byte)((b5 << 7) | (b6 << 2) | (b7 >> 3)));
				outStream.Add((byte)((b7 << 5) | b8));
			}
			this.DecodeLastBlock(outStream,
				data[length - 8], data[length - 7], data[length - 6], data[length - 5],
				data[length - 4], data[length - 3], data[length - 2], data[length - 1]);

			return outStream.ToArray();
		}

		virtual protected int DecodeLastBlock(ICollection<byte> outStream, char c1, char c2, char c3, char c4, char c5, char c6, char c7, char c8)
		{
			if (c3 == _PaddingCharacter)
			{
				byte b1 = _DecodeTable[c1];
				byte b2 = _DecodeTable[c2];
				outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
				return 1;
			}

			if (c5 == _PaddingCharacter)
			{
				byte b1 = _DecodeTable[c1];
				byte b2 = _DecodeTable[c2];
				byte b3 = _DecodeTable[c3];
				byte b4 = _DecodeTable[c4];
				outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
				outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
				return 2;
			}

			if (c6 == _PaddingCharacter)
			{
				byte b1 = _DecodeTable[c1];
				byte b2 = _DecodeTable[c2];
				byte b3 = _DecodeTable[c3];
				byte b4 = _DecodeTable[c4];
				byte b5 = _DecodeTable[c5];

				outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
				outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
				outStream.Add((byte)((b4 << 4) | (b5 >> 1)));
				return 3;
			}

			if (c8 == _PaddingCharacter)
			{
				byte b1 = _DecodeTable[c1];
				byte b2 = _DecodeTable[c2];
				byte b3 = _DecodeTable[c3];
				byte b4 = _DecodeTable[c4];
				byte b5 = _DecodeTable[c5];
				byte b6 = _DecodeTable[c6];
				byte b7 = _DecodeTable[c7];

				outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
				outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
				outStream.Add((byte)((b4 << 4) | (b5 >> 1)));
				outStream.Add((byte)((b5 << 7) | (b6 << 2) | (b7 >> 3)));
				return 4;
			}

			else
			{
				byte b1 = _DecodeTable[c1];
				byte b2 = _DecodeTable[c2];
				byte b3 = _DecodeTable[c3];
				byte b4 = _DecodeTable[c4];
				byte b5 = _DecodeTable[c5];
				byte b6 = _DecodeTable[c6];
				byte b7 = _DecodeTable[c7];
				byte b8 = _DecodeTable[c8];
				outStream.Add((byte)((b1 << 3) | (b2 >> 2)));
				outStream.Add((byte)((b2 << 6) | (b3 << 1) | (b4 >> 4)));
				outStream.Add((byte)((b4 << 4) | (b5 >> 1)));
				outStream.Add((byte)((b5 << 7) | (b6 << 2) | (b7 >> 3)));
				outStream.Add((byte)((b7 << 5) | b8));
				return 5;
			}
		}

		protected int NextI(string data, int i, int finish)
		{
			while ((i < finish) && this.Ignore(data[i])) i++;

			return i;
		}

		protected bool Ignore(char c)
		{
			return (c == '\n') || (c == '\r') || (c == '\t') || (c == ' ') || (c == '-');
		}

		protected void InitialiseDecodingTable()
		{
			for (int index = 0; index < _EncodeTable.Length; index++)
			{
				_DecodeTable[_EncodeTable[index]] = (byte)index;
			}
		}
	}
}

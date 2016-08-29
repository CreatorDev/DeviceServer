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
	public class TlvReader : IDisposable
	{
		private Stream _Stream;
		private TlvRecord _TlvRecord;

		public TlvRecord TlvRecord
		{
			get { return _TlvRecord; }
		}

		public TlvReader(byte[] stream)
		{
			_Stream = new MemoryStream(stream);
		}

		public TlvReader(Stream stream)
		{
			_Stream = stream;
		}

		public void Dispose()
		{
			//_Stream.Dispose();
		}


		public bool Read()
		{
			_TlvRecord = null;
			try
			{
				int type = _Stream.ReadByte();
				if (type == -1)
					throw new EndOfStreamException();

				TTlvTypeIdentifier typeIdentifier;
				ushort identifier;
				uint length;
				byte[] value = null;

				if ((type & TlvConstant.RESOURCE_WITH_VALUE) == TlvConstant.RESOURCE_WITH_VALUE)
					typeIdentifier = TTlvTypeIdentifier.ResourceWithValue;
				else if ((type & TlvConstant.MULTIPLE_RESOURCES) == TlvConstant.MULTIPLE_RESOURCES)
					typeIdentifier = TTlvTypeIdentifier.MultipleResources;
				else if ((type & TlvConstant.RESOURCE_INSTANCE) == TlvConstant.RESOURCE_INSTANCE)
					typeIdentifier = TTlvTypeIdentifier.ResourceInstance;
				else
					typeIdentifier = TTlvTypeIdentifier.ObjectInstance;

				bool identifier16Bits = ((type & TlvConstant.IDENTIFIER_16BITS) == TlvConstant.IDENTIFIER_16BITS);
				if (identifier16Bits)
				{
					identifier = NetworkByteOrderConverter.ToUInt16(_Stream);
				}
				else
				{
					int readByte = _Stream.ReadByte();
					if (readByte == -1)
						throw new EndOfStreamException();
					identifier = (ushort)readByte;
				}

				if ((type & TlvConstant.LENGTH_24BIT) == TlvConstant.LENGTH_24BIT)
				{
					length = NetworkByteOrderConverter.ToUInt24(_Stream); ;
				}
				else if ((type & TlvConstant.LENGTH_16BIT) == TlvConstant.LENGTH_16BIT)
				{
					length = NetworkByteOrderConverter.ToUInt16(_Stream);
				}
				else if ((type & TlvConstant.LENGTH_8BIT) == TlvConstant.LENGTH_8BIT)
				{
					int readByte = _Stream.ReadByte();
					if (readByte == -1)
						throw new EndOfStreamException();
					length = (uint)readByte;
				}
				else //3Bit length
				{
					length = (uint)(type & 0x7);
				}

                value = new byte[length];
                int read = _Stream.Read(value, 0, (int)length);
                if (read != (int)length)
                    throw new EndOfStreamException();
				_TlvRecord = new TlvRecord() { TypeIdentifier = typeIdentifier, Identifier = identifier, Length = length, Value = value };

			}
			catch (EndOfStreamException)
			{

			}
			return _TlvRecord != null;
		}



	}
}




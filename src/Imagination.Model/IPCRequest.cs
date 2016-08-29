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

namespace Imagination.Model
{
	public class IPCRequest
	{
		private MemoryStream _Payload = new MemoryStream(4096);

		public string Method { get; set; }

		public int RequestNumber { get; set; }
		
		public MemoryStream Payload { get {return _Payload;} }

		private static int _LastRequestNumber = 0;

		public IPCRequest()
		{
			RequestNumber = System.Threading.Interlocked.Increment(ref _LastRequestNumber);
			if (RequestNumber == (int.MaxValue - 100))
			{
				_LastRequestNumber = 0;
			}
		}

        public void AddToPayload(bool value)
        {
            IPCHelper.Write(_Payload, value);
        }

        
        public void AddToPayload(Guid value)
		{
			IPCHelper.Write(_Payload, value);
		}

		public void AddToPayload(int value)
		{
			IPCHelper.Write(_Payload, value);
		}

		public void AddToPayload(string value)
		{
			IPCHelper.Write(_Payload, value);
		}

		public byte[] Serialise()
		{
			MemoryStream result = new MemoryStream(4096);
			byte[] buffer = Encoding.UTF8.GetBytes(Method);
			NetworkByteOrderConverter.WriteInt32(result, (int)(buffer.Length + 8 + _Payload.Length));
			NetworkByteOrderConverter.WriteInt32(result, buffer.Length);
			result.Write(buffer, 0, buffer.Length);
			NetworkByteOrderConverter.WriteInt32(result, RequestNumber);
			_Payload.Position = 0;
			_Payload.CopyTo(result);
			return result.ToArray();
		}

		public static IPCRequest Deserialise(Stream stream)
		{
			IPCRequest result = new IPCRequest();
			int length = NetworkByteOrderConverter.ToInt32(stream);
			byte[] buffer = new byte[length];
			stream.Read(buffer, 0, length);
			result.Method = Encoding.UTF8.GetString(buffer);
			result.RequestNumber = NetworkByteOrderConverter.ToInt32(stream);
			stream.CopyTo(result._Payload);
			result._Payload.Position = 0;
			return result;
		}

        public bool ReadBoolean()
        {
            return IPCHelper.ReadBoolean(_Payload);
        }

		public Guid ReadGuid()
		{
			return IPCHelper.ReadGuid(_Payload);
		}

		public string ReadString()
		{
			return IPCHelper.ReadString(_Payload);
		}

		public int ReadInt32()
		{
			return IPCHelper.ReadInt32(_Payload);
		}
	}
}

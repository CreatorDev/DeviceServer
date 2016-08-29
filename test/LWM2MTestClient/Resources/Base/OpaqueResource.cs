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
	internal class OpaqueResource : LWM2MResource
	{
		public byte[] Value { get; set; }

		public OpaqueResource(String name)
			: base(name, true)
		{ }


		public static OpaqueResource Deserialise(Request request)
		{
			OpaqueResource result = null;
			string name = request.UriPaths.Last();
			if (!string.IsNullOrEmpty(name) && (request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				OpaqueResource resource = new OpaqueResource(name);
				using (TlvReader reader = new TlvReader(request.Payload))
				{
					if (Deserialise(reader, resource))
						result = resource;
				}
			}
			return result;
		}

		public static bool Deserialise(TlvReader reader, OpaqueResource item)
		{
			bool result = false; 
			if (reader.TlvRecord == null)
				reader.Read();
			if (reader.TlvRecord.TypeIdentifier == TTlvTypeIdentifier.ResourceWithValue)
			{
				item.Value = reader.TlvRecord.Value;
				result = true;
			}
			return result;
		}
		
		protected override void DoPost(CoapExchange exchange)
		{
			UpdateResource(exchange);
		}


		protected override void DoPut(CoapExchange exchange)
		{
			UpdateResource(exchange);
		}

		public override void Serialise(TlvWriter writer)
		{
			Serialise(writer, false);
		}

		public void Serialise(TlvWriter writer, bool resourceInstance)
		{
			ushort identifier;
			if (ushort.TryParse(Name, out identifier))
			{
				TTlvTypeIdentifier typeIdentifier = TTlvTypeIdentifier.ResourceWithValue;
				if (resourceInstance)
					typeIdentifier = TTlvTypeIdentifier.ResourceInstance;
				writer.Write(typeIdentifier, identifier, Value);
			}
		}

		public override void SetValue(string value)
		{

		}

		private void UpdateResource(CoapExchange exchange)
		{
			OpaqueResource opaqueResource = OpaqueResource.Deserialise(exchange.Request);
			if (opaqueResource == null)
			{
				Response response = Response.CreateResponse(exchange.Request, StatusCode.BadRequest);
				exchange.Respond(response);
			}
			else
			{
				Value = opaqueResource.Value;
				Response response = Response.CreateResponse(exchange.Request, StatusCode.Changed);
				exchange.Respond(response);
			}
		}

		public override string ToString()
		{
			StringBuilder result = new StringBuilder();
			if (Value != null)
			{
				result.Append("0x");
				foreach (byte item in Value)
				{
					byte b;
					b = ((byte)(item >> 4));
					result.Append((char)(b > 9 ? b + 0x37 : b + 0x30));
					b = ((byte)(item & 0xF));
					result.Append((char)(b > 9 ? b + 0x37 : b + 0x30));
				}
			}
			return result.ToString();

		}
	}
}

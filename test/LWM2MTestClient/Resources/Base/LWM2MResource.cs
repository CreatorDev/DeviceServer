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
using CoAP;
using CoAP.Server.Resources;
using Imagination.LWM2M;

namespace Imagination.LWM2M.Resources
{
	public class LWM2MResource : Resource
	{
		public event EventHandler Updated;

		public string Description { get; set; }

		public LWM2MResource(String name, Boolean visible)
			: base(name, visible)
		{
			if (visible)
			{
				Observable = true;
			}
		}

		public virtual bool Deserialise(TlvReader reader)
		{
			return true;
		}

		protected override void DoGet(CoapExchange exchange)
		{
			Response response; 
			if (exchange.Request.Observe.HasValue && exchange.Request.Observe.Value == 0)
			{
				response = Response.CreateResponse(exchange.Request, StatusCode.Content);
				response.MaxAge = 86400;
			}
			else
				response = Response.CreateResponse(exchange.Request, StatusCode.Content);
			using (MemoryStream steam = new MemoryStream())
			{
				TlvWriter writer = new TlvWriter(steam);
				this.Serialise(writer);
				response.Payload = steam.ToArray();
			}
			response.ContentType = TlvConstant.CONTENT_TYPE_TLV;
			exchange.Respond(response);
		}


		protected override void DoPost(CoapExchange exchange)
		{
			Request request = exchange.Request;
			bool processed = false;
			if ((request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				if (request.Payload != null)
				{
					using (TlvReader reader = new TlvReader(request.Payload))
					{
						this.Deserialise(reader);
						processed = true;
					}
				}
			}
			if (processed)
			{
				Response response = Response.CreateResponse(exchange.Request, StatusCode.Changed);
				exchange.Respond(response);
				if (Updated != null)
					Updated(this, null);
			}
			else
			{
				Response response = Response.CreateResponse(exchange.Request, StatusCode.BadRequest);
				exchange.Respond(response);
			}
		}

		protected override void DoPut(CoapExchange exchange)
		{
			Request request = exchange.Request;
			bool processed = false;
			if ((request.ContentType == TlvConstant.CONTENT_TYPE_TLV))
			{
				using (TlvReader reader = new TlvReader(request.Payload))
				{
					this.Deserialise(reader);
					processed = true;
				}
			}
			if (processed)
			{
				Response response = Response.CreateResponse(exchange.Request, StatusCode.Changed);
				exchange.Respond(response);
				if (Updated != null)
					Updated(this, null);
			}
			else
			{
				Response response = Response.CreateResponse(exchange.Request, StatusCode.BadRequest);
				exchange.Respond(response);
			}
		}

		public virtual void Serialise(TlvWriter writer)
		{

		}

		public virtual void SetValue(string value)
		{

		}

	}
}

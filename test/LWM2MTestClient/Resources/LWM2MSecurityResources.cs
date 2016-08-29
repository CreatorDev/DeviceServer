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

namespace Imagination.LWM2M.Resources
{
	internal class LWM2MSecurityResources : LWM2MResources
	{
		public LWM2MSecurityResources()
			: base("0", true)
        { }

	
		protected override void DoPost(CoapExchange exchange)
		{
			LWM2MSecurityResource lWM2MSecurityResource = LWM2MSecurityResource.Deserialise(exchange.Request);
			if (lWM2MSecurityResource == null)
			{
				Response response = Response.CreateResponse(exchange.Request, StatusCode.BadRequest);
				exchange.Respond(response);
			}
			else
			{
				this.Add(lWM2MSecurityResource);
				Response response = Response.CreateResponse(exchange.Request, StatusCode.Changed);
				exchange.Respond(response);
			}
		}

		protected override void DoPut(CoapExchange exchange)
		{
			LWM2MSecurityResource lWM2MSecurityResource = LWM2MSecurityResource.Deserialise(exchange.Request);
			if (lWM2MSecurityResource == null)
			{
				Response response = Response.CreateResponse(exchange.Request, StatusCode.BadRequest);
				exchange.Respond(response);
			}
			else
			{
				this.Add(lWM2MSecurityResource);
				Response response = Response.CreateResponse(exchange.Request, StatusCode.Changed);
				exchange.Respond(response);
			}
		}
	}
}

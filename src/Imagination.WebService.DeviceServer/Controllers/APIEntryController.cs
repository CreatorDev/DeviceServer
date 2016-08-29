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

using Microsoft.AspNetCore.Mvc;
using Imagination.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Imagination.Documentation;
using System.Net;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/", DisplayName = "API Entrypoint", Summary = "The entry point to the creator Device Management Server REST API.")]
    [AllowAnonymous]
    [Route("/")]
    public class APIEntryController : ControllerBase
    {
        [MethodDocumentation(
            Summary = "Retrieve a list of endpoints. You must have a valid session token to see all endpoints.",
            ResponseTypes = new[] { typeof(APIEntryPoint) },
            StatusCodes = new[] { HttpStatusCode.OK },
            AllowMultipleSecuritySchemes = true
        )]
        [HttpGet]
        public IActionResult GetEntryPoint()
        {
            IActionResult result;
            APIEntryPoint response = new APIEntryPoint();
            string rootUrl = Request.GetRootUrl();
            response.AddLink<OAuthToken>(Request, "authenticate", string.Concat(rootUrl, "/oauth/token"));
            if (User.Identity is OrganisationIdentity)
            {
                response.AddLink<AccessKeys>(Request, "accesskeys", string.Concat(rootUrl, "/accesskeys"));
                response.AddLink<Configuration>(Request, "configuration", string.Concat(rootUrl, "/configuration"));
                response.AddLink<Clients>(Request, "clients", string.Concat(rootUrl, "/clients"));
                response.AddLink<Identities>(Request, "identities", string.Concat(rootUrl, "/identities"));
                response.AddLink<ObjectDefinitions>(Request, "objectdefinitions", string.Concat(rootUrl, "/objecttypes/definitions"));
                response.AddLink<Subscriptions>(Request, "subscriptions", string.Concat(rootUrl, "/subscriptions"));
                response.AddLink<Metrics>(Request, "metrics", string.Concat(rootUrl, "/metrics"));
            }
            response.AddLink<Versions>(Request, "versions", string.Concat(rootUrl, "/versions"));
            result = Request.GetObjectResult(response);
            return result;
        }
    }
}

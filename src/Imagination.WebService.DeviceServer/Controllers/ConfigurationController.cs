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

using Imagination.BusinessLogic;
using Imagination.Documentation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/configuration", DisplayName = "Configuration", Summary = "Retrieve server configuration for the current organisation.")]
    [RouteDocumentation(Route = "/configuration/bootstrap", DisplayName = "Bootstrap Configuration", Summary = "Retrieve bootstrap server connection information that a client can use to connect to.")]
    [Authorize()]
    [Route("/configuration")]
    public class ConfigurationController: ControllerBase
    {
        [MethodDocumentation(
            Summary = "Retrieve links to configurable endpoints.",
            ResponseTypes = new[] { typeof(ServiceModels.Configuration) },
            StatusCodes = new[] { HttpStatusCode.OK, }
        )]
        [HttpGet]
        public IActionResult GetConfiguration()
        {
            IActionResult result;
            ServiceModels.Configuration response = new ServiceModels.Configuration();
            string rootUrl = Request.GetRootUrl();
            response.AddLink<ServiceModels.Bootstrap>(Request,"bootstrap", string.Concat(rootUrl, "/configuration/bootstrap"));
            result = Request.GetObjectResult(response);
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve bootstrap configuration for a client to use to connect to the Device Server.",
            ResponseTypes = new[] { typeof(ServiceModels.Bootstrap) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }
        )]
        [HttpGet("bootstrap")]
        public IActionResult GetBootstrapConfiguration()
        {
            IActionResult result;
            Model.BootstrapServer bootstrapServer = BusinessLogicFactory.Configuration.GetBootstrapServer(User.GetOrganisationID());
            if (bootstrapServer == null)
                result = new NotFoundResult();
            else
            {
                ServiceModels.Bootstrap response = new ServiceModels.Bootstrap(bootstrapServer);
                result = Request.GetObjectResult(response);
            }
            return result;
        }

    }
}

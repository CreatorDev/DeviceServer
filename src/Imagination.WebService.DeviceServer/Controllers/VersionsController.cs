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

using Imagination.Common;
using Imagination.Documentation;
using Imagination.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/versions", DisplayName = "Versions", Summary = "Retrieve versions for each component used by the Device Server.")]
    [AllowAnonymous]
    [Route("/versions")]
    public class VersionsController : Controller
    {
        [MethodDocumentation(
            Summary = "Retrieve a list of Device Server component versions.",
            ResponseTypes = new[] { typeof(ServiceModels.Versions) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpGet]
        public IActionResult GetVersions()
        {
            IActionResult result;
            Versions response = new Versions();

            response.BuildNumber = VersionsHelper.GetCurrentAssemblyVersions(true)?.Item2; // Use full version with pre-release suffix

#if DEBUG
            foreach (Tuple<string, string> nameVersionPair in VersionsHelper.GetAssemblyVersions(null, true))
#else
            foreach (Tuple<string, string> nameVersionPair in VersionsHelper.GetAssemblyVersions())
#endif
            {
                VersionComponent component = new VersionComponent();
                component.Name = nameVersionPair.Item1;
                component.Version = nameVersionPair.Item2;
             
                if (response.Components == null)
                {
                    response.Components = new List<VersionComponent>();
                }
                response.Components.Add(component);
            }
            result = Request.GetObjectResult(response);
            return result;
        }
    }
}

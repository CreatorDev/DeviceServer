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
using Imagination.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/identities", DisplayName = "Identities", Summary = "Retrieve PSK and certificate identities clients can use to securely connect to the Device Server.")]
    [RouteDocumentation(Route = "/identities/psk", DisplayName = "PSK Identities", Summary = "Manage a list of Pre-Shared Key identities.")]
    [RouteDocumentation(Route = "/identities/psk/{identity}", DisplayName = "PSK Identity", Summary = "Manage an individual Pre-Shared Key Identity.")]
    [RouteDocumentation(Route = "/identities/certificates", DisplayName = "Certificates", Summary = "Manage a list of certificates.")]
    [NamedParameterDocumentation("identity", "PSK Identity", TNamedParameterType.String, "Pre-Shared Key identity.")]
    [Authorize()]
    [Route("/identities")]
    public class IdentitiesController: ControllerBase
    {
        [MethodDocumentation(
            Summary = "Retrieve links to different identity types.",
            ResponseTypes = new[] { typeof(ServiceModels.Identities) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpGet]
        public IActionResult GetIdentities()
        {
            IActionResult result = null;
            string rootUrl = Request.GetRootUrl();
            ServiceModels.Identities response = new ServiceModels.Identities();
            response.AddLink<PSKIdentities>(Request,"psk", string.Concat(rootUrl, "/identities/psk"));
            response.AddLink<Certificates>(Request, "certificate", string.Concat(rootUrl, "/identities/certificates"));
            result = Request.GetObjectResult(response);
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve a list of PSK identities.",
            ResponseTypes = new[] { typeof(ServiceModels.PSKIdentities) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpGet("psk")]
        public IActionResult GetPSKIdentities()
        {
            IActionResult result = null;
            string rootUrl = Request.GetRootUrl();
            PSKIdentities response = new PSKIdentities();
            response.AddLink("add", string.Concat(rootUrl, "/identities/psk"), null);

            int organisationID = User.GetOrganisationID();
            List<Model.PSKIdentity> pskIdentities = BusinessLogicFactory.Identities.GetPSKIdentities(organisationID);
            response.PageInfo = Request.GetPageInfo(pskIdentities.Count);
            int endIndex = response.PageInfo.StartIndex + response.PageInfo.ItemsCount;
            for (int index = response.PageInfo.StartIndex; index < endIndex; index++)
            {
                ServiceModels.PSKIdentity pskIdentity = new ServiceModels.PSKIdentity(pskIdentities[index]);
                pskIdentity.AddSelfLink(string.Concat(rootUrl, "/identities/psk/", pskIdentity.Identity), false, true);
                response.Add(pskIdentity);
            }
            result = Request.GetObjectResult(response);

            return result;
        }

        [MethodDocumentation(
            Summary = "Create a new PSK identity.",
            ResponseTypes = new[] { typeof(ServiceModels.PSKIdentity) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpPost("psk")]
        public IActionResult AddPSKIdentity()
        {
            IActionResult result = null;
            string rootUrl = Request.GetRootUrl();


            Model.PSKIdentity item = new Model.PSKIdentity();
            item.OrganisationID = User.GetOrganisationID();

            BusinessLogicFactory.Identities.SavePSKIdentity(item, Model.TObjectState.Add);

            ServiceModels.PSKIdentity response = new ServiceModels.PSKIdentity(item);
            response.Secret = item.Secret;
            response.AddSelfLink(string.Concat(rootUrl, "/identities/psk/", response.Identity), false, true);

            result = Request.GetObjectResult(response, System.Net.HttpStatusCode.Created);
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve a single PSK identity.",
            ResponseTypes = new[] { typeof(ServiceModels.PSKIdentity) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }
        )]
        [HttpGet("psk/{identity}")]
        public IActionResult GetPSKIdentity(string identity)
        {
            IActionResult result;
            Model.PSKIdentity pskIdentity = BusinessLogicFactory.Identities.GetPSKIdentity(identity);
            if (pskIdentity == null)
                result = new NotFoundResult();
            else
            {
                ServiceModels.PSKIdentity response = new ServiceModels.PSKIdentity(pskIdentity);
                string rootUrl = Request.GetRootUrl();
                response.AddSelfLink(string.Concat(rootUrl, "/identities/psk/", response.Identity), false, true);
                result = Request.GetObjectResult(response);
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Delete a PSK identity.",
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound }
        )]
        [HttpDelete("psk/{identity}")]
        public IActionResult RemovePSKIdentity(string identity)
        {
            IActionResult result;
            Model.PSKIdentity pskIdentity = BusinessLogicFactory.Identities.GetPSKIdentity(identity);
            if (pskIdentity == null)
                result = new NotFoundResult();
            else
            {
                BusinessLogicFactory.Identities.SavePSKIdentity(pskIdentity, Model.TObjectState.Delete);
                result = new NoContentResult();
            }
            return result;
        }

        /*[HttpPut("psk/{identity}")]
        public IActionResult UpdatePSKIdentity(string identity, [FromBody] ServiceModels.PSKIdentity pskIdentity)
        {
            IActionResult result;
            Model.PSKIdentity existingPSKIdentity = BusinessLogicFactory.Identities.GetPSKIdentity(identity);
            if (existingPSKIdentity == null)
                result = new NotFoundResult();
            else
            {
                existingPSKIdentity.Identity = pskIdentity.Identity;
                existingPSKIdentity.Secret = pskIdentity.Secret;
                BusinessLogicFactory.Identities.SavePSKIdentity(existingPSKIdentity, Model.TObjectState.Update);
                result = new NoContentResult();
            }
            return result;
        }*/

        [MethodDocumentation(
            Summary = "Retrieve a list of certificates.",
            ResponseTypes = new[] { typeof(ServiceModels.Certificates) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpGet("certificates")]
        public IActionResult GetCertificates()
        {
            IActionResult result = null;
            string rootUrl = Request.GetRootUrl();
            Certificates response = new Certificates();
            response.AddLink("add", string.Concat(rootUrl, "/identities/certificates"), null);
            result = Request.GetObjectResult(response);
            return result;
        }

        [MethodDocumentation(
            Summary = "Create a new certificate.",
            ResponseTypes = new[] { typeof(ServiceModels.Certificate) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpPost("certificates")]
        public IActionResult AddCertificate()
        {
            IActionResult result = null;
            string rootUrl = Request.GetRootUrl();
            Certificate response = new Certificate();
            response.CertificateFormat = "PEM";
            response.RawCertificate = BusinessLogic.BusinessLogicFactory.Identities.CreateCertificate(User.GetOrganisationID());
            result = Request.GetObjectResult(response, System.Net.HttpStatusCode.Created);
            return result;
        }
    }
}

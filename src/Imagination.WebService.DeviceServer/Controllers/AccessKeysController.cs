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
using System.Collections.Generic;
using System.Net;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/accesskeys", DisplayName = "Access Keys", Summary = "Retrieve a list of keys that give users of an organisation access to the Device Server.")]
    [RouteDocumentation(Route = "/accesskeys/{key}", DisplayName = "Access Key", Summary = "Manage an individual access key.")]
    [NamedParameterDocumentation("key", "Key", TNamedParameterType.String, "A unique key used to access an organisation.")]
    [Authorize()]
    [Route("/accesskeys")]
    public class AccessKeysController: ControllerBase
    {
        [MethodDocumentation(
            Summary = "Retrieve the list of access keys for the current organisation.",
            ResponseTypes = new[] { typeof(ServiceModels.AccessKeys) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpGet()]
        public IActionResult GetAccessKeys()
        {
            IActionResult result;
            ServiceModels.AccessKeys response = new ServiceModels.AccessKeys();
            string rootUrl = Request.GetRootUrl();
            response.AddLink("add", string.Concat(rootUrl, "/accesskeys"), null);
            int organisationID = User.GetOrganisationID();
            List<Model.AccessKey> accessKeys = BusinessLogicFactory.AccessKeys.GetAccessKeys(organisationID);
            response.PageInfo = Request.GetPageInfo(accessKeys.Count);
            int endIndex = response.PageInfo.StartIndex + response.PageInfo.ItemsCount;
            for (int index = response.PageInfo.StartIndex; index < endIndex; index++)
            {
                ServiceModels.AccessKey accessKey = new ServiceModels.AccessKey(accessKeys[index]);
                accessKey.AddSelfLink(string.Concat(rootUrl, "/accesskeys/", accessKey.Key), true, true);
                response.Add(accessKey);
            }
            result = Request.GetObjectResult(response);
            return result;
        }

        [MethodDocumentation(
            Summary = "Add a new access key for the current organisation.",
            RequestTypes = new[] { typeof(ServiceModels.AccessKey) },
            ResponseTypes = new[] { typeof(ServiceModels.AccessKey) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest }
        )]
        [HttpPost()]
        public IActionResult AddAccessKey([FromBody] ServiceModels.AccessKey accessKey)
        {
            IActionResult result;
            if (accessKey == null)
                result = new BadRequestResult();
            else
            {
                Model.AccessKey item = accessKey.ToModel();
                item.OrganisationID = User.GetOrganisationID();
                BusinessLogicFactory.AccessKeys.SaveAccessKey(item, Model.TObjectState.Add);
                ServiceModels.AccessKey response = new ServiceModels.AccessKey(item);
                response.Secret = item.Secret;
                string rootUrl = Request.GetRootUrl();
                response.AddSelfLink(string.Concat(rootUrl, "/accesskeys/", response.Key), true, true);
                result = Request.GetObjectResult(response, HttpStatusCode.Created);
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve the contents of an access key.",
            ResponseTypes = new[] { typeof(ServiceModels.AccessKey) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }
        )]
        [HttpGet("{key}")]
        public IActionResult GetAccessKey(string key)
        {
            IActionResult result;
            Model.AccessKey accessKey = BusinessLogicFactory.AccessKeys.GetAccessKey(key);
            if (accessKey == null)
                result = new NotFoundResult();
            else
            {
                ServiceModels.AccessKey response = new ServiceModels.AccessKey(accessKey);
                string rootUrl = Request.GetRootUrl();
                response.AddSelfLink(string.Concat(rootUrl, "/accesskeys/", response.Key), true, true);
                result = Request.GetObjectResult(response);
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Delete an access key.",
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound }
        )]
        [HttpDelete("{key}")]
        public IActionResult RemoveAccessKey(string key)
        {
            IActionResult result;
            Model.AccessKey accessKey = BusinessLogicFactory.AccessKeys.GetAccessKey(key);
            if (accessKey == null)
                result = new NotFoundResult();
            else
            {
                BusinessLogicFactory.AccessKeys.SaveAccessKey(accessKey, Model.TObjectState.Delete);
                result = new NoContentResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Update an access key.",
            RequestTypes = new[] { typeof(ServiceModels.AccessKey) },
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound }
        )]
        [HttpPut("{key}")]
        public IActionResult UpdateAccessKey(string key, [FromBody] ServiceModels.AccessKey accessKey)
        {
            IActionResult result;
            Model.AccessKey existingAccessKey = BusinessLogicFactory.AccessKeys.GetAccessKey(key);
            if (existingAccessKey == null)
                result = new NotFoundResult();
            else
            {
                existingAccessKey.Name = accessKey.Name;
                BusinessLogicFactory.AccessKeys.SaveAccessKey(existingAccessKey, Model.TObjectState.Update);
                result = new NoContentResult();
            }
            return result;
        }

    }
}

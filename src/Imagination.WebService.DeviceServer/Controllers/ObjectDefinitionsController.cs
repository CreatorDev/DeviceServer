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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/objecttypes/definitions", DisplayName = "Object Definitions", Summary = "Manage a list of defined object types for the current organisation.")]
    [RouteDocumentation(Route = "/objecttypes/definitions/{id}", DisplayName = "Object Definition", Summary = "Manage an individual object definition.")]
    [NamedParameterDocumentation("id", "Object Definition ID", TNamedParameterType.String, "An object definition's unique ID.")]
    [Authorize()]
    [Route("/objecttypes/definitions")]
    public class ObjectDefinitionsController : ControllerBase
    {
        [MethodDocumentation(
            Summary = "Retrieve a list of object definitions for the current organisation.",
            ResponseTypes = new[] { typeof(ServiceModels.ObjectDefinitions) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpGet()]
        public IActionResult GetObjectDefinitions()
        {
            IActionResult result;
            ServiceModels.ObjectDefinitions response = new ServiceModels.ObjectDefinitions();
            string rootUrl = Request.GetRootUrl();
            response.AddLink("add", string.Concat(rootUrl, "/objecttypes/definitions"), null);
            int organisationID = User.GetOrganisationID();
            List<Model.ObjectDefinition> objectDefinitions = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinitions(organisationID);
            response.PageInfo = Request.GetPageInfo(objectDefinitions.Count);
            int endIndex = response.PageInfo.StartIndex + response.PageInfo.ItemsCount;
            for (int index = response.PageInfo.StartIndex; index < endIndex; index++)
            {
                ServiceModels.ObjectDefinition objectDefinition = new ServiceModels.ObjectDefinition(objectDefinitions[index]);
                if (organisationID == 0)
                    objectDefinition.AddSelfLink(string.Concat(rootUrl, "/objecttypes/definitions/", objectDefinition.ObjectDefinitionID), true, true);
                else
                    objectDefinition.AddSelfLink(string.Concat(rootUrl, "/objecttypes/definitions/", objectDefinition.ObjectDefinitionID), objectDefinitions[index].OrganisationID.HasValue, objectDefinitions[index].OrganisationID.HasValue);
                response.Add(objectDefinition);
            }
            result = Request.GetObjectResult(response);
            return result;
        }

        [MethodDocumentation(
            Summary = "Add a new object definition or collection of new object definitions.",
            RequestTypes = new[] { typeof(ServiceModels.ObjectDefinitions), typeof(ServiceModels.ObjectDefinition) },
            ResponseTypes = new[] { typeof(ServiceModels.ResourcesCreated), typeof(ServiceModels.ResourceCreated) },
            StatusCodes = new[] { HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Conflict }
        )]
        [HttpPost]
        public IActionResult AddObjectDefinitions()
        {
            IActionResult result;

            try
            {
                if (Request.ContentType.StartsWith(typeof(ServiceModels.ObjectDefinitions).GetContentType()))
                {
                    ServiceModels.ObjectDefinitions objectDefinitions = Request.Deserialise<ServiceModels.ObjectDefinitions>();
                    result = AddObjectDefinitions(objectDefinitions);
                }
                else
                {
                    ObjectDefinition objectDefinition = Request.Deserialise<ObjectDefinition>();
                    result = AddObjectDefinition(objectDefinition);
                }
            }
            catch (BadRequestException)
            {
                result = new BadRequestResult();
            }

            return result;
        }

        private IActionResult AddObjectDefinitions(ServiceModels.ObjectDefinitions objectDefinitions)
        {
            IActionResult result;
            if (objectDefinitions == null || objectDefinitions.Items == null)
                result = new BadRequestResult();
            else
            {
                List<Model.ObjectDefinition> items = new List<Model.ObjectDefinition>();
                foreach (ServiceModels.ObjectDefinition item in objectDefinitions.Items)
                {
                    Model.ObjectDefinition objectDefinition = item.ToModel();
                    objectDefinition.OrganisationID = User.GetOrganisationID();
                    items.Add(objectDefinition);
                }
                try
                {
                    BusinessLogicFactory.ObjectDefinitions.SaveObjectDefinitions(items, Model.TObjectState.Add);
                    string rootUrl = Request.GetRootUrl();
                    ResourcesCreated response = new ResourcesCreated();
                    foreach (Model.ObjectDefinition item in items)
                    {
                        ResourceCreated resourceCreated = new ResourceCreated();
                        resourceCreated.ID = StringUtils.GuidEncode(item.ObjectDefinitionID);
                        resourceCreated.AddSelfLink(string.Concat(rootUrl, "/objecttypes/definitions/", resourceCreated.ID), false, false);
                        response.Add(resourceCreated);
                    }
                    result = Request.GetObjectResult(response, System.Net.HttpStatusCode.Created);
                }
                catch (ConflictException)
                {
                    result = new StatusCodeResult((int)HttpStatusCode.Conflict);
                }
            }
            return result;
        }

        private IActionResult AddObjectDefinition(ServiceModels.ObjectDefinition objectDefinition)
        {
            IActionResult result;
            if (objectDefinition == null)
                result = new BadRequestResult();
            else
            {
                ResourceCreated response = new ResourceCreated();
                string rootUrl = Request.GetRootUrl();
                Model.ObjectDefinition item = objectDefinition.ToModel();
                item.OrganisationID = User.GetOrganisationID();
                try
                {
                    BusinessLogicFactory.ObjectDefinitions.SaveObjectDefinition(item, Model.TObjectState.Add);
                    response.ID = StringUtils.GuidEncode(item.ObjectDefinitionID);
                    response.AddSelfLink(string.Concat(rootUrl, "/objecttypes/definitions/", response.ID), false, false);
                    result = Request.GetObjectResult(response, System.Net.HttpStatusCode.Created);
                }
                catch (ConflictException)
                {
                    result = new StatusCodeResult((int)HttpStatusCode.Conflict);
                }
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve an individual object definition.",
            ResponseTypes = new[] { typeof(ServiceModels.ObjectDefinition) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest }
        )]
        [HttpGet("{id}")]
        public IActionResult GetObjectDefinition(string id)
        {
            IActionResult result;
            Guid objectDefinitionID;
            if (StringUtils.GuidTryDecode(id, out objectDefinitionID))
            {
                int organisationID = User.GetOrganisationID();
                Model.ObjectDefinition objectDefinition = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(organisationID, objectDefinitionID);
                if (objectDefinition == null)
                    result = new NotFoundResult();
                else
                {
                    ServiceModels.ObjectDefinition response = new ServiceModels.ObjectDefinition(objectDefinition);
                    string rootUrl = Request.GetRootUrl();                     
                    if (organisationID == 0)
                        response.AddSelfLink(string.Concat(rootUrl, "/objecttypes/definitions/", response.ObjectDefinitionID), true, true);
                    else
                        response.AddSelfLink(string.Concat(rootUrl, "/objecttypes/definitions/", response.ObjectDefinitionID), objectDefinition.OrganisationID.HasValue, objectDefinition.OrganisationID.HasValue);
                    result = Request.GetObjectResult(response);
                }
            }
            else
                result = new BadRequestResult();
            return result;
        }

        [MethodDocumentation(
            Summary = "Delete an object definition.",
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest }
        )]
        [HttpDelete("{id}")]
        public IActionResult RemoveObjectDefinition(string id)
        {
            IActionResult result;
            Guid objectDefinitionID;
            if (StringUtils.GuidTryDecode(id, out objectDefinitionID))
            {
                int organisationID = User.GetOrganisationID();
                Model.ObjectDefinition objectDefinition = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(organisationID, objectDefinitionID);
                if (objectDefinition == null)
                    result = new NotFoundResult();
                else
                {
                    if (!objectDefinition.OrganisationID.HasValue && (organisationID != 0))
                        result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
                    else
                    {
                        BusinessLogicFactory.ObjectDefinitions.SaveObjectDefinition(objectDefinition, Model.TObjectState.Delete);
                        result = new NoContentResult();
                    }
                }
            }
            else
                result = new BadRequestResult();
            return result;
        }

        [MethodDocumentation(
            Summary = "Update an object definition.",
            RequestTypes = new[] { typeof(ServiceModels.ObjectDefinition) },
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest }
        )]
        [HttpPut("{id}")]
        public IActionResult UpdateObjectDefinition(string id, [FromBody] ServiceModels.ObjectDefinition objectDefinition)
        {
            IActionResult result;
            Guid objectDefinitionID;
            if (StringUtils.GuidTryDecode(id, out objectDefinitionID))
            {
                int organisationID = User.GetOrganisationID();
                Model.ObjectDefinition existingObjectDefinition = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(organisationID, objectDefinitionID);
                if (existingObjectDefinition == null)
                    result = new NotFoundResult();
                else
                {
                    if (!existingObjectDefinition.OrganisationID.HasValue && (organisationID != 0))
                        result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
                    else
                    {
                        Model.ObjectDefinition updatedObjectDefinition = objectDefinition.ToModel();
                        updatedObjectDefinition.OrganisationID = User.GetOrganisationID();
                        BusinessLogicFactory.ObjectDefinitions.SaveObjectDefinition(updatedObjectDefinition, Model.TObjectState.Update);
                        result = new NoContentResult();
                    }
                }
            }
            else
                result = new BadRequestResult();
            return result;
        }

    }
}

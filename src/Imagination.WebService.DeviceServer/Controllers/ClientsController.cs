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
using Microsoft.AspNetCore.Mvc;
using Imagination.ServiceModels;
using Imagination.BusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Imagination.Documentation;
using System.Net;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/clients", DisplayName = "Clients", Summary = "Retrieve a list of connected devices within the current organisation.")]
    [RouteDocumentation(Route = "/clients/{clientID}", DisplayName = "Client", Summary = "Manage an individual connected device.")]
    [RouteDocumentation(Route = "/clients/{clientID}/objecttypes", DisplayName = "Client Object Types", Summary = "Access an individual connected device's object types.")]
    [RouteDocumentation(Route = "/clients/{clientID}/objecttypes/{definitionID}", DisplayName = "Client Object Type", Summary = "Manage a single object type for an individual connected device.")]
    [RouteDocumentation(Route = "/clients/{clientID}/objecttypes/{definitionID}/instances", DisplayName = "Object Instances", Summary = "Manage the object instances of an object type for an individual connected device.")]
    [RouteDocumentation(Route = "/clients/{clientID}/objecttypes/{definitionID}/instances/{instanceID}", DisplayName = "Client Object Type", Summary = "Manage a single object instance for an individual connected device.")]
    [NamedParameterDocumentation("clientID", "Client ID", TNamedParameterType.String, "A client's unique ID.")]
    [NamedParameterDocumentation("definitionID", "Definition ID", TNamedParameterType.String, "An object definition's unique ID.")]
    [NamedParameterDocumentation("instanceID", "Instance ID", TNamedParameterType.String, "An object instance's unique ID.")]
    [Authorize()]
    [Route("/clients")]
    public class ClientsController : ControllerBase
    {
        [MethodDocumentation(
            Summary = "Retrieve the list of connected clients within the current organisation.",
            ResponseTypes = new[] { typeof(ServiceModels.Clients) },
            StatusCodes = new[] { HttpStatusCode.OK,  }
        )]
        [HttpGet()]
        public IActionResult GetClients()
        {
            IActionResult result;
            ServiceModels.Clients response = new ServiceModels.Clients();
            string rootUrl = Request.GetRootUrl();
            List<Model.Client> clients = BusinessLogicFactory.Clients.GetConnectedClients(User.GetOrganisationID());
            response.PageInfo = Request.GetPageInfo(clients.Count);
            int endIndex = response.PageInfo.StartIndex + response.PageInfo.ItemsCount;
            for (int index = response.PageInfo.StartIndex; index < endIndex; index++)
            {
                ServiceModels.Client client = new ServiceModels.Client(clients[index]);
                string clientUrl = string.Concat(Request.GetRootUrl(), "/clients/", StringUtils.GuidEncode(clients[index].ClientID));
                client.AddSelfLink(clientUrl, false, true);
                client.AddLink<ObjectTypes>(Request, "objecttypes", string.Concat(clientUrl, "/objecttypes"));
                client.AddLink<ServiceModels.Subscriptions>(Request, "subscriptions", string.Concat(clientUrl, "/subscriptions"));
                client.AddLink<ServiceModels.Metrics>(Request, "metrics", string.Concat(clientUrl, "/metrics"));
                response.Add(client);
            }
            result = Request.GetObjectResult(response);
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve an individual connected client.",
            ResponseTypes = new[] { typeof(ServiceModels.Client) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }
        )]
        [HttpGet("/clients/{clientID}")]
        public IActionResult GetClient(string clientID)
        {
            IActionResult result;
            Guid clientIDGuid;
            if (StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                if (client != null)
                {
                    ServiceModels.Client response = new ServiceModels.Client(client);
                    string clientUrl = string.Concat(Request.GetRootUrl(), "/clients/", StringUtils.GuidEncode(client.ClientID));
                    response.AddSelfLink(clientUrl, false, true);
                    response.AddLink<ObjectTypes>(Request, "objecttypes", string.Concat(clientUrl, "/objecttypes"));
                    response.AddLink<ServiceModels.Subscriptions>(Request, "subscriptions", string.Concat(clientUrl, "/subscriptions"));
                    response.AddLink<ServiceModels.Metrics>(Request, "metrics", string.Concat(clientUrl, "/metrics"));
                    result = Request.GetObjectResult(response);
                }
                else
                {
                    result = new NotFoundResult();
                }
            }
            else
            {
                result = new NotFoundResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Remove and blacklist a client by its ID.",
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound }
        )]
        [HttpDelete("/clients/{clientID}")]
        public IActionResult RemoveClient(string clientID)
        {
            IActionResult result;
            Guid clientIDGuid;
            if (StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                if (client != null)
                {
                    BusinessLogicFactory.Clients.DeleteClient(client);
                    result = new NoContentResult();
                }
                else
                {
                    result = new NotFoundResult();
                }
            }
            else
            {
                result = new NotFoundResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve the available object types of a connected client.",
            ResponseTypes = new[] { typeof(ServiceModels.Client) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }
        )]
        [HttpGet("/clients/{clientID}/objecttypes")]
        public IActionResult GetObjectTypes(string clientID)
        {
            IActionResult result;
            Guid clientIDGuid;
            if (StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                if (client != null)
                {
                    ServiceModels.ObjectTypes response = new ServiceModels.ObjectTypes();
                    string rootUrl = Request.GetRootUrl();

                    if (client.SupportedTypes != null)
                    {
                        Model.ObjectDefinitionLookups definitions = BusinessLogicFactory.ObjectDefinitions.GetLookups();

                        response.PageInfo = Request.GetPageInfo(client.SupportedTypes.Count);
                        int endIndex = response.PageInfo.StartIndex + response.PageInfo.ItemsCount;
                        for (int index = response.PageInfo.StartIndex; index < endIndex; index++)
                        {
                            ServiceModels.ObjectType objectType = new ServiceModels.ObjectType(client.SupportedTypes[index]);
                            if (definitions != null)
                            {
                                Model.ObjectDefinition definition = definitions.GetObjectDefinition(User.GetOrganisationID(), objectType.ObjectTypeID);
                                if (definition != null)
                                {
                                    objectType.AddSelfLink(string.Concat(rootUrl, "/clients/", clientID, "/objecttypes/", StringUtils.GuidEncode(definition.ObjectDefinitionID)), false, false);
                                    objectType.AddLink<ObjectDefinition>(Request, "definition", string.Concat(rootUrl, "/objecttypes/definitions/", StringUtils.GuidEncode(definition.ObjectDefinitionID)));
                                    objectType.AddLink("instances", string.Concat(rootUrl, "/clients/", clientID, "/objecttypes/", StringUtils.GuidEncode(definition.ObjectDefinitionID), "/instances"), Request.GetContentType(definition.MIMEType));
                                }
                            }
                            response.Add(objectType);
                        }
                    }
                    result = Request.GetObjectResult(response);
                }
                else
                {
                    result = new NotFoundResult();
                }
            }
            else
            {
                result = new BadRequestResult();
            }
           
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve an object definition.",
            ResponseTypes = new[] { typeof(ServiceModels.ObjectType) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }
        )]
        [HttpGet("/clients/{clientID}/objecttypes/{definitionID}")]
        public IActionResult GetObjectType(string clientID, string definitionID)
        {
            IActionResult result = new NotFoundResult();
            Guid clientIDGuid;
            Guid definitionIDGuid;
            if (StringUtils.GuidTryDecode(clientID, out clientIDGuid) && StringUtils.GuidTryDecode(definitionID, out definitionIDGuid))
            {
                Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                if ((client != null) && (client.SupportedTypes != null))
                {
                    Model.ObjectDefinition definition = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(User.GetOrganisationID(), definitionIDGuid);
                    if (definition != null)
                    {
                        Model.ObjectType objectType = client.SupportedTypes.GetObjectType(int.Parse(definition.ObjectID));
                        if (objectType != null)
                        {
                            ServiceModels.ObjectType response = new ServiceModels.ObjectType(objectType);
                            string rootUrl = Request.GetRootUrl();
                            response.AddSelfLink(string.Concat(rootUrl, "/clients/", clientID, "/objecttypes/", definitionID), false, false);
                            response.AddLink<ObjectDefinition>(Request, "definition", string.Concat(rootUrl, "/objecttypes/definitions/", definitionID));
                            response.AddLink("instances", string.Concat(rootUrl, "/clients/", clientID, "/objecttypes/", definitionID, "/instances"), Request.GetContentType(definition.MIMEType));
                            result = Request.GetObjectResult(response);
                        }
                    }
                }
            }
            else
            {
                result = new BadRequestResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Create an object instance.",
            RequestTypes = new[] { typeof(ServiceModels.ObjectInstance) },
            ResponseTypes = new[] { typeof(ServiceModels.ResourceCreated), },
            StatusCodes = new[] { HttpStatusCode.Created, HttpStatusCode.BadRequest }
        )]
        [HttpPost("/clients/{clientID}/objecttypes/{definitionID}/instances")]
        public IActionResult AddObjectInstance(string clientID, string definitionID)
        {
            IActionResult result;

            Guid definitionIDGuid, clientIDGuid;
            if (StringUtils.GuidTryDecode(definitionID, out definitionIDGuid) && StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                int organisationID = User.GetOrganisationID();
                Model.ObjectDefinition definition = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(organisationID, definitionIDGuid);
                if (definition != null)
                {
                    // TODO: add error handling around deserialisation.
                    // TODO: could lwm2mObject.instanceID be an optional parameter, allowing a web client to specify?
                    Model.Object lwm2mObject = new ServiceModels.ObjectInstance(definition, Request).Resource;
                    BusinessLogicFactory.Clients.SaveObject(client, lwm2mObject, Model.TObjectState.Add);

                    ServiceModels.ResourceCreated response = new ServiceModels.ResourceCreated();
                    response.ID = lwm2mObject.InstanceID;
                    string rootUrl = Request.GetRootUrl();
                    response.AddSelfLink(string.Concat(rootUrl, "/clients/", clientID, "/objecttypes/", definitionID, "/instances/", response.ID), true, true);
                    result = Request.GetObjectResult(response, System.Net.HttpStatusCode.Created);
                }
                else
                {
                    result = new BadRequestResult();
                }
            }
            else
            {
                result = new BadRequestResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve the instances of an object.",
            ResponseTypes = new[] { typeof(ServiceModels.ObjectInstances) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }
        )]
        [HttpGet("/clients/{clientID}/objecttypes/{definitionID}/instances")]
        public IActionResult GetObjectInstances(string clientID, string definitionID)
        {
            IActionResult result;

            Guid definitionIDGuid, clientIDGuid;
            if (StringUtils.GuidTryDecode(definitionID, out definitionIDGuid) && StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                int organisationID = User.GetOrganisationID();
                Model.ObjectDefinition definition = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(organisationID, definitionIDGuid);
                if (definition != null)
                {
                    Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                    if (client != null)
                    {
                        List<Model.Object> instances = BusinessLogicFactory.Clients.GetObjects(client, definition.ObjectDefinitionID);
                        if (instances != null)
                        {
                            ObjectInstances response = new ObjectInstances(definition);
                            string rootUrl = Request.GetRootUrl();
                            string instancesUrl = string.Concat(rootUrl, "/clients/", clientID, "/objecttypes/", StringUtils.GuidEncode(definition.ObjectDefinitionID), "/instances");

                            response.AddLink("add", instancesUrl, "");

                            response.PageInfo = Request.GetPageInfo(instances.Count);
                            int endIndex = response.PageInfo.StartIndex + response.PageInfo.ItemsCount;
                            for (int index = response.PageInfo.StartIndex; index < endIndex; index++)
                            {
                                ObjectInstance instance = new ObjectInstance(definition, instances[index]);
                                string instanceUrl = string.Concat(instancesUrl, "/", instances[index].InstanceID);
                                AddObjectInstanceLinks(Request, definition, instance, instanceUrl);                                
                                response.Add(instance);
                            }
                            result = response.GetAction();
                        }
                        else
                        {
                            result = new NotFoundResult();
                        }
                    }
                    else
                    {
                        result = new NotFoundResult();
                    }
                }
                else
                {
                    result = new NotFoundResult();
                }
            }
            else
            {
                result = new BadRequestResult();
            }
            return result;
        }

        private void AddObjectInstanceLinks(Microsoft.AspNetCore.Http.HttpRequest request, Model.ObjectDefinition objectDefinition, ObjectInstance instance, string instanceUrl)
        {
            instance.AddSelfLink(instanceUrl, true, true);
            instance.AddLink<ServiceModels.Subscriptions>(request, "subscriptions", string.Concat(instanceUrl, "/subscriptions"));

            string rootUrl = request.GetRootUrl();
            instance.AddLink<ObjectDefinition>(Request, "definition", string.Concat(rootUrl, "/objecttypes/definitions/", StringUtils.GuidEncode(objectDefinition.ObjectDefinitionID)));
        }

        [MethodDocumentation(
            Summary = "Retrieve an object instance.",
            ResponseTypes = new[] { typeof(ServiceModels.ObjectInstance) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }
        )]
        [HttpGet("/clients/{clientID}/objecttypes/{definitionID}/instances/{instanceID}")]
        public IActionResult GetObjectInstance(string clientID, string definitionID, string instanceID)
        {
            IActionResult result;

            Guid definitionIDGuid, clientIDGuid;
            if (StringUtils.GuidTryDecode(definitionID, out definitionIDGuid) && StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                int organisationID = User.GetOrganisationID();
                Model.ObjectDefinition definition = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(organisationID, definitionIDGuid);
                if (definition != null)
                {
                    Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                    if (client != null)
                    {
                        Model.Object instance = BusinessLogicFactory.Clients.GetObject(client, definition.ObjectDefinitionID, instanceID);
                        if (instance != null)
                        {
                            ServiceModels.ObjectInstance response = new ServiceModels.ObjectInstance(definition, instance);
                            string rootUrl = Request.GetRootUrl();
                            string instanceUrl = string.Concat(rootUrl, "/clients/", clientID, "/objecttypes/", StringUtils.GuidEncode(definition.ObjectDefinitionID), "/instances/", instanceID);

                            AddObjectInstanceLinks(Request, definition, response, instanceUrl);
                            result = response.GetAction();
                        }
                        else
                        {
                            result = new NotFoundResult();
                        }
                    }
                    else
                    {
                        result = new NotFoundResult();
                    }
                }
                else
                {
                    result = new NotFoundResult();
                }
            }
            else
            {
                result = new BadRequestResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Delete an object instance.",
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }
        )]
        [HttpDelete("/clients/{clientID}/objecttypes/{definitionID}/instances/{instanceID}")]
        public IActionResult RemoveObjectInstance(string clientID, string definitionID, string instanceID)
        {
            IActionResult result;

            Guid definitionIDGuid, clientIDGuid;
            if (StringUtils.GuidTryDecode(definitionID, out definitionIDGuid) && StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                int organisationID = User.GetOrganisationID();
                Model.ObjectDefinition definition = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(organisationID, definitionIDGuid);
                if (definition != null)
                {
                    Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                    if (client != null)
                    {
                        Model.Object instance = BusinessLogicFactory.Clients.GetObject(client, definition.ObjectDefinitionID, instanceID);
                        if (instance != null)
                        {
                            BusinessLogicFactory.Clients.SaveObject(client, instance, Model.TObjectState.Delete);
                            result = new NoContentResult();
                        }
                        else
                        {
                            result = new NotFoundResult();
                        }
                    }
                    else
                    {
                        result = new NotFoundResult();
                    }
                }
                else
                {
                    result = new NotFoundResult();
                }
            }
            else
            {
                result = new BadRequestResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Update an object instance.",
            RequestTypes = new[] { typeof(ServiceModels.ObjectInstance) },
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.BadRequest, HttpStatusCode.NotFound }
        )]
        [HttpPut("/clients/{clientID}/objecttypes/{definitionID}/instances/{instanceID}")]
        public IActionResult UpdateObjectInstance(string clientID, string definitionID, string instanceID)
        {
            IActionResult result;

            Guid definitionIDGuid, clientIDGuid;
            if (StringUtils.GuidTryDecode(definitionID, out definitionIDGuid) && StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                int organisationID = User.GetOrganisationID();
                Model.ObjectDefinition definition = BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(organisationID, definitionIDGuid);
                if (definition != null)
                {
                    // TODO: add error handling around deserialisation.
                    List<Model.Property> executeProperties = new List<Model.Property>();
                    Model.Object lwm2mObject = new ServiceModels.ObjectInstance(definition, Request).Resource;
                    lwm2mObject.InstanceID = instanceID;
                    int index = 0;
                    while (index < lwm2mObject.Properties.Count)
                    {
                        Model.PropertyDefinition propertyDefinition = definition.GetProperty(lwm2mObject.Properties[index].PropertyDefinitionID);
                        if (propertyDefinition.Access == Model.TAccessRight.Execute)
                        {
                            executeProperties.Add(lwm2mObject.Properties[index]);
                            lwm2mObject.Properties.RemoveAt(index);
                        }
                        else
                        {
                            index++;
                        }
                    }
                    if (lwm2mObject.Properties.Count > 0)
                        BusinessLogicFactory.Clients.SaveObject(client, lwm2mObject, Model.TObjectState.Update);
                    if (executeProperties.Count > 0)
                    {
                        BusinessLogicFactory.Clients.Execute(client, lwm2mObject, executeProperties);
                    }
                    result = new NoContentResult();
                }
                else
                {
                    result = new BadRequestResult();
                }
            }
            else
            {
                result = new BadRequestResult();
            }
            return result;
        }
    }
}

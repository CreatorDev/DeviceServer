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
using Imagination.Model;
using Imagination.ServiceModels;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using Imagination.Documentation;
using System.Net;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/subscriptions", DisplayName = "Subscriptions", Summary = "Retrieve a list of subscriptions for the current organisation.")]
    [RouteDocumentation(Route = "/clients/{clientID}/subscriptions", DisplayName = "Subscriptions", Summary = "Retrieve a list of subscriptions for an individual client.")]
    [RouteDocumentation(Route = "/clients/{clientID}/objecttypes/{definitionID}/instances/{instanceID}/subscriptions", DisplayName = "Subscriptions", Summary = "Retrieve a list of subscriptions for an individual object instance.")]
    [RouteDocumentation(Route = "/subscriptions/{subscriptionID}", DisplayName = "Subscription", Summary = "Retrieve an individual subscription.")]
    [NamedParameterDocumentation("clientID", "Client ID", TNamedParameterType.String, "A client's unique ID.")]
    [NamedParameterDocumentation("definitionID", "Object Definition ID", TNamedParameterType.String, "An object definition's unique ID.")]
    [NamedParameterDocumentation("instanceID", "Instance ID", TNamedParameterType.String, "An object instance's unique ID.")]
    [Authorize()]
    [Route("/subscriptions")]
    public class SubscriptionsController : Controller
    {
        public SubscriptionsController()
        {
        }

        [MethodDocumentation(
            Summary = "Create a new subscription.",
            RequestTypes = new[] { typeof(ServiceModels.Subscription) },
            ResponseTypes = new[] { typeof(ServiceModels.ResourceCreated) },
            StatusCodes = new[] { HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Conflict }
        )]
        [HttpPost()]
        public IActionResult AddSubscription([FromBody] ServiceModels.Subscription subscription)
        {
            return AddSubscription(null, null, null, subscription);
        }

        [MethodDocumentation(
            Summary = "Create a new subscription.",
            RequestTypes = new[] { typeof(ServiceModels.Subscription) },
            ResponseTypes = new[] { typeof(ServiceModels.ResourceCreated) },
            StatusCodes = new[] { HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Conflict }
        )]
        [HttpPost("/clients/{clientID}/subscriptions")]
        public IActionResult AddSubscription(string clientID, [FromBody] ServiceModels.Subscription subscription)
        {
            return AddSubscription(clientID, null, null, subscription);
        }

        /*[HttpPost("/clients/{clientID}/objecttypes/{definitionID}/subscriptions")]
        public IActionResult AddSubscription(string clientID, string definitionID, [FromBody] ServiceModels.Subscription subscription)
        {
            return AddSubscription(clientID, definitionID, null, subscription);
        }*/

        [MethodDocumentation(
            Summary = "Create a new subscription.",
            RequestTypes = new[] { typeof(ServiceModels.Subscription) },
            ResponseTypes = new[] { typeof(ServiceModels.ResourceCreated) },
            StatusCodes = new[] { HttpStatusCode.Created, HttpStatusCode.BadRequest, HttpStatusCode.Conflict }
        )]
        [HttpPost("/clients/{clientID}/objecttypes/{definitionID}/instances/{instanceID}/subscriptions")]
        public IActionResult AddSubscription(string clientID, string definitionID, string instanceID, [FromBody] ServiceModels.Subscription subscription)
        {
            IActionResult result;
            if (subscription != null)
            {
                Model.Subscription item = subscription.ToModel(Request, clientID, definitionID, instanceID);
                item.OrganisationID = User.GetOrganisationID();
                item.SubscriptionID = Guid.NewGuid();

                if (!ExistingSubscriptionMatches(item))
                {
                    BusinessLogicFactory.Subscriptions.SaveSubscription(item, Model.TObjectState.Add);

                    ServiceModels.ResourceCreated response = new ServiceModels.ResourceCreated();
                    response.ID = StringUtils.GuidEncode(item.SubscriptionID);
                    string rootUrl = Request.GetRootUrl();
                    response.AddSelfLink(string.Concat(rootUrl, "/subscriptions/", response.ID), false, false);
                    result = Request.GetObjectResult(response, System.Net.HttpStatusCode.Created);
                }
                else
                {
                    result = new StatusCodeResult((int)System.Net.HttpStatusCode.Conflict);
                }
            }
            else
            {
                result = new BadRequestResult();
            }
            return result;
        }

        private bool ExistingSubscriptionMatches(Model.Subscription item)
        {
            List<Model.Subscription> subscriptions = null;
            if (item.ClientID != Guid.Empty)
            {
                subscriptions = BusinessLogicFactory.Subscriptions.GetSubscriptions(item.ClientID);
            }
            else
            {
                subscriptions = BusinessLogicFactory.Subscriptions.GetSubscriptions(item.OrganisationID);
            }

            foreach (Model.Subscription subscription in subscriptions)
            {
                if (subscription.ClientID == item.ClientID && subscription.ObjectDefinitionID == item.ObjectDefinitionID && 
                    subscription.ObjectID == item.ObjectID && string.Compare(subscription.Url, item.Url, true) == 0 && 
                    subscription.PropertyDefinitionID == item.PropertyDefinitionID && subscription.SubscriptionType == item.SubscriptionType 
                    && subscription.SubscriptionID != item.SubscriptionID)
                    return true;
            }

            return false;
        }

        [MethodDocumentation(
            Summary = "Retrieve a list of all subscriptions created by the current organisation.",
            ResponseTypes = new[] { typeof(ServiceModels.Subscriptions) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpGet]
        public IActionResult GetSubscriptions()
        {
            return GetSubscriptions(null, null, null);
        }

        [MethodDocumentation(
            Summary = "Retrieve a list of subscriptions for an individual client.",
            ResponseTypes = new[] { typeof(ServiceModels.Subscriptions) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpGet("/clients/{clientID}/subscriptions")]
        public IActionResult GetSubscriptions(string clientID)
        {
            return GetSubscriptions(clientID, null, null);
        }

        /*[HttpGet("/clients/{clientID}/objecttypes/{definitionID}/subscriptions")]
        public IActionResult GetSubscriptions(string clientID, string definitionID)
        {
            return GetSubscriptions(clientID, definitionID, null);
        }*/

        [MethodDocumentation(
            Summary = "Retrieve a list of subscriptions for an individual object instance.",
            ResponseTypes = new[] { typeof(ServiceModels.Subscriptions) },
            StatusCodes = new[] { HttpStatusCode.OK }
        )]
        [HttpGet("/clients/{clientID}/objecttypes/{definitionID}/instances/{instanceID}/subscriptions")]
        public IActionResult GetSubscriptions(string clientID, string definitionID, string instanceID)
        {
            Guid clientIDGuid = Guid.Empty;
            Guid definitionIDGuid = Guid.Empty;
            IActionResult result = null;

            if (clientID != null && !StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                result = new BadRequestResult();
            }
            else if (clientID != null && BusinessLogicFactory.Clients.GetClient(clientIDGuid) == null)
            {
                result = new NotFoundResult();
            }
            else if (definitionID != null && !StringUtils.GuidTryDecode(definitionID, out definitionIDGuid))
            {
                result = new BadRequestResult();
            }
            else if (definitionID != null && BusinessLogicFactory.ObjectDefinitions.GetObjectDefinition(User.GetOrganisationID(), definitionIDGuid) == null)
            {
                result = new NotFoundResult();
            }
            else
            {
                string rootUrl = Request.GetRootUrl();
                if (clientID != null)
                    rootUrl = string.Concat(rootUrl, "/clients/", clientID);
                if (definitionID != null)
                    rootUrl = string.Concat(rootUrl, "/objecttypes/", definitionID);
                if (instanceID != null)
                    rootUrl = string.Concat(rootUrl, "/instances/", instanceID);

                ServiceModels.Subscriptions response = new ServiceModels.Subscriptions();
                response.AddLink("add", string.Concat(rootUrl, "/subscriptions"), null);

                
                List<Model.Subscription> subscriptions = null;
                if (clientID == null)
                {
                    int organisationID = User.GetOrganisationID();
                    subscriptions = BusinessLogicFactory.Subscriptions.GetSubscriptions(organisationID);
                }
                else
                {
                    List<Model.Subscription> unfilteredSubscriptions = BusinessLogicFactory.Subscriptions.GetSubscriptions(clientIDGuid);

                    if (definitionID != null)
                    {
                        subscriptions = new List<Model.Subscription>();

                        foreach (Model.Subscription subscription in unfilteredSubscriptions)
                        {
                            if (definitionID == null || definitionIDGuid == subscription.ObjectDefinitionID)
                            {
                                if (instanceID == null || instanceID.Equals(subscription.ObjectID))
                                {
                                    subscriptions.Add(subscription);
                                }
                            }
                        }
                    }
                    else
                    {
                        subscriptions = unfilteredSubscriptions;
                    }
                }

                response.PageInfo = Request.GetPageInfo(subscriptions.Count);
                int endIndex = response.PageInfo.StartIndex + response.PageInfo.ItemsCount;
                for (int index = response.PageInfo.StartIndex; index < endIndex; index++)
                {
                    ServiceModels.Subscription subscription = new ServiceModels.Subscription(subscriptions[index]);
                    subscription.AddSelfLink(string.Concat(rootUrl, "/subscriptions/", StringUtils.GuidEncode(subscriptions[index].SubscriptionID)), true, true);
                    response.Add(subscription);
                }
                result = Request.GetObjectResult(response);
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve an individual subscription.",
            ResponseTypes = new[] { typeof(ServiceModels.Subscription) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.BadRequest}
        )]
        [HttpGet ("{subscriptionID}")]
        public IActionResult GetSubscription(string subscriptionID)
        {
            IActionResult result;
            Guid subscriptionIDGuid;
            if (StringUtils.GuidTryDecode(subscriptionID, out subscriptionIDGuid))
            {
                Model.Subscription subscription = BusinessLogicFactory.Subscriptions.GetSubscription(subscriptionIDGuid);
                if (subscription == null)
                    result = new NotFoundResult();
                else
                {
                    ServiceModels.Subscription response = new ServiceModels.Subscription(subscription);
                    string rootUrl = Request.GetRootUrl();
                    response.AddSelfLink(string.Concat(rootUrl, "/subscriptions/", StringUtils.GuidEncode(subscription.SubscriptionID)), true, true);
                    result = Request.GetObjectResult(response);
                }
            }
            else
            {
                result = new BadRequestResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Remove a subscription.",
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest }
        )]
        [HttpDelete("{subscriptionID}")]
        public IActionResult RemoveSubscription(string subscriptionID)
        {
            IActionResult result;
            Guid subscriptionIDGuid;
            if (StringUtils.GuidTryDecode(subscriptionID, out subscriptionIDGuid))
            {
                Model.Subscription subscription = BusinessLogicFactory.Subscriptions.GetSubscription(subscriptionIDGuid);
                if (subscription == null)
                    result = new NotFoundResult();
                else
                {
                    BusinessLogicFactory.Subscriptions.SaveSubscription(subscription, TObjectState.Delete);
                    result = new NoContentResult();
                }
            }
            else
            {
                result = new BadRequestResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Update a subscription.",
            RequestTypes = new[] { typeof(ServiceModels.Subscription) },
            StatusCodes = new[] { HttpStatusCode.NoContent, HttpStatusCode.NotFound, HttpStatusCode.BadRequest }
        )]
        [HttpPut("{subscriptionID}")]
        public IActionResult UpdateSubscription(string subscriptionID, [FromBody] ServiceModels.Subscription subscription)
        {
            IActionResult result;
            Guid subscriptionIDGuid;
            if (StringUtils.GuidTryDecode(subscriptionID, out subscriptionIDGuid))
            {
                Model.Subscription subscriptionToUpdate = BusinessLogicFactory.Subscriptions.GetSubscription(subscriptionIDGuid);
                if (subscriptionToUpdate == null)
                    result = new NotFoundResult();
                else
                {
                    subscription.UpdateModel(subscriptionToUpdate);

                    if (!ExistingSubscriptionMatches(subscriptionToUpdate))
                    {
                        BusinessLogicFactory.Subscriptions.SaveSubscription(subscriptionToUpdate, TObjectState.Update);
                        result = new NoContentResult();
                    }
                    else
                    {
                        result = new StatusCodeResult((int)System.Net.HttpStatusCode.Conflict);
                    }
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

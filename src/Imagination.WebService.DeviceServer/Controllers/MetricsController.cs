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
using Imagination.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/metrics", DisplayName = "Metrics", Summary = "Retrieve usage metrics and statistics for the current organisation.")]
    [RouteDocumentation(Route = "/metrics/{metricID}", DisplayName = "Metric", Summary = "Retrieve an individual organisation-level metric.")]
    [RouteDocumentation(Route = "/clients/{clientID}/metrics", DisplayName = "Metrics", Summary = "Retrieve usage metrics and statistics for an individual client.")]
    [RouteDocumentation(Route = "/clients/{clientID}/metrics/{metricID}", DisplayName = "Metric", Summary = "Retrieve an individual client-level metric.")]
    [NamedParameterDocumentation("clientID", "Client ID", TNamedParameterType.String, "A client's unique ID.")]
    [NamedParameterDocumentation("metricID", "Metric ID", TNamedParameterType.String, "A metric's unique ID.")]
    [Authorize()]
    [Route("/metrics")]
    public class MetricsController : ControllerBase
    {
        [MethodDocumentation(
            Summary = "Retrieve a list of metrics for the current organisation.",
            ResponseTypes = new[] { typeof(ServiceModels.Metrics) },
            StatusCodes = new[] { HttpStatusCode.OK, }
        )]
        [HttpGet()]
        public IActionResult GetMetrics()
        {
            IActionResult result;
            ServiceModels.Metrics response = new ServiceModels.Metrics();
            List<Model.OrganisationMetric> metrics = BusinessLogicFactory.Metrics.GetMetrics(User.GetOrganisationID());
            AddPageInfo(Request, response, string.Concat(Request.GetRootUrl(), "/metrics/"), metrics);
            result = Request.GetObjectResult(response);
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve a list of metrics for an individual client.",
            ResponseTypes = new[] { typeof(ServiceModels.Metrics) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }
        )]
        [HttpGet("/clients/{clientID}/metrics")]
        public IActionResult GetMetrics(string clientID)
        {
            IActionResult result;
            Guid clientIDGuid;
            if (StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                Model.Client client = BusinessLogicFactory.Clients.GetClient(clientIDGuid);
                if (client != null)
                {
                    ServiceModels.Metrics response = new ServiceModels.Metrics();
                    List<Model.ClientMetric> metrics = BusinessLogicFactory.Metrics.GetMetrics(client.ClientID);
                    AddPageInfo(Request, response, string.Concat(Request.GetRootUrl(), "/clients/", clientID, "/metrics/"), metrics);
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

        private static void AddPageInfo<T>(Microsoft.AspNetCore.Http.HttpRequest request, ServiceModels.Metrics response, string baseUrl, List<T> metrics) where T: MetricBase
        {
            response.PageInfo = request.GetPageInfo(metrics.Count);
            int endIndex = response.PageInfo.StartIndex + response.PageInfo.ItemsCount;
            for (int index = response.PageInfo.StartIndex; index < endIndex; index++)
            {
                ServiceModels.Metric metric = new ServiceModels.Metric(metrics[index]);
                metric.AddSelfLink(string.Concat(baseUrl, metrics[index].Name), false, false);
                response.Add(metric);
            }
        }

        [MethodDocumentation(
            Summary = "Retrieve an individual metric for the current organisation.",
            ResponseTypes = new[] { typeof(ServiceModels.Metric) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }
        )]
        [HttpGet("{metricID}")]
        public IActionResult GetMetric(string metricID)
        {
            IActionResult result;
            string rootUrl = Request.GetRootUrl();
            OrganisationMetric metric = BusinessLogicFactory.Metrics.GetMetric(User.GetOrganisationID(), metricID);

            if (metric != null)
            {
                ServiceModels.Metric response = new ServiceModels.Metric(metric);
                response.AddSelfLink(string.Concat(rootUrl, "/metrics/", metricID), false, false);
                result = Request.GetObjectResult(response);
            }
            else
            {
                result = new NotFoundResult();
            }
            return result;
        }

        [MethodDocumentation(
            Summary = "Retrieve an individual metric for a single client.",
            ResponseTypes = new[] { typeof(ServiceModels.Metric) },
            StatusCodes = new[] { HttpStatusCode.OK, HttpStatusCode.NotFound }
        )]
        [HttpGet("/clients/{clientID}/metrics/{metricID}")]
        public IActionResult GetMetric(string clientID, string metricID)
        {
            IActionResult result;
            Guid clientIDGuid;
            if (StringUtils.GuidTryDecode(clientID, out clientIDGuid))
            {
                string rootUrl = Request.GetRootUrl();
                ClientMetric metric = BusinessLogicFactory.Metrics.GetMetric(clientIDGuid, metricID);
                if (metric != null)
                {
                    ServiceModels.Metric response = new ServiceModels.Metric(metric);
                    response.AddSelfLink(string.Concat(rootUrl, "/clients/", clientID, "/metrics/", metricID), false, false);
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
    }
}

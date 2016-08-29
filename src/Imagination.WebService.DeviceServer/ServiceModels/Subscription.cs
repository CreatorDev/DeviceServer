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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Xml;
using Imagination.Model.Subscriptions;
using System.Text.RegularExpressions;
using Imagination.Model;
using Imagination.DataAccess;

namespace Imagination.ServiceModels
{
    [ContentType("application/vnd.imgtec.subscription")]
    public class Subscription : LinkableResource
    {
        public string SubscriptionType { get; set; }

        public string Property { get; set; }

        public string Url { get; set; }

        public string AcceptContentType { get; set; }

        public class SubscriptionAttributes
        {
            public int? Pmin { get; set; }
            public int? Pmax { get; set; }
            public double? Step { get; set; }
            public double? LessThan { get; set; }
            public double? GreaterThan { get; set; }
        };

        public SubscriptionAttributes Attributes { get; set; }

        public Subscription()
        {
        }

        public Subscription(Model.Subscription item)
        {
            SubscriptionType = item.SubscriptionType.ToString();

            if (item.ObjectDefinitionID != null && item.PropertyDefinitionID != null)
            {
                Model.ObjectDefinition objectDefinition = DataAccessFactory.ObjectDefinitions.GetLookups().GetObjectDefinition(item.ObjectDefinitionID);
                if (objectDefinition != null)
                {
                    Model.PropertyDefinition propertyDefinition = objectDefinition.GetProperty(item.PropertyDefinitionID);
                    if (propertyDefinition != null)
                        Property = propertyDefinition.SerialisationName;
                }
            }
            Url = item.Url;
            AcceptContentType = item.AcceptContentType;

            if (item.NotificationParameters != null)
            {
                Attributes = new SubscriptionAttributes();
                Attributes.Pmin = item.NotificationParameters.MinimumPeriod;
                Attributes.Pmax = item.NotificationParameters.MaximumPeriod;
                Attributes.Step = item.NotificationParameters.Step;
                Attributes.LessThan = item.NotificationParameters.LessThan;
                Attributes.GreaterThan = item.NotificationParameters.GreaterThan;
            }
        }
        public void UpdateModel(Model.Subscription model)
        {
            if (Url != null)
                model.Url = Url;

            if (Attributes != null && model.NotificationParameters == null)
                model.NotificationParameters = new NotificationParameters();

            if (Attributes != null)
            {
                if (Attributes.Pmin.HasValue)
                    model.NotificationParameters.MinimumPeriod = Attributes.Pmin;
                if (Attributes.Pmax.HasValue)
                    model.NotificationParameters.MaximumPeriod = Attributes.Pmax;
                if (Attributes.Step.HasValue)
                    model.NotificationParameters.Step = Attributes.Step;
                if (Attributes.LessThan.HasValue)
                    model.NotificationParameters.LessThan = Attributes.LessThan;
                if (Attributes.GreaterThan.HasValue)
                    model.NotificationParameters.GreaterThan = Attributes.GreaterThan;
            }        
        }

        public Model.Subscription ToModel(HttpRequest request, string clientID, string definitionID, string instanceID)
        {
            Model.Subscription result = new Model.Subscription();
            TSubscriptionType subscriptionType;
            if (Enum.TryParse<TSubscriptionType>(SubscriptionType, true, out subscriptionType))
            {
                result.SubscriptionType = subscriptionType;
            }
            else
            {
                throw new BadRequestException();
            }

            if (AcceptContentType == null)
                result.AcceptContentType = request.ContentType.Contains("xml") ? "application/xml" : "application/json";
            else
                result.AcceptContentType = AcceptContentType;

            if (clientID != null)
                result.ClientID = StringUtils.GuidDecode(clientID);
            if (definitionID != null)
                result.ObjectDefinitionID = StringUtils.GuidDecode(definitionID);
            if (instanceID != null)
                result.ObjectID = instanceID;

            if (Attributes != null)
            {
                result.NotificationParameters = new NotificationParameters();
                result.NotificationParameters.MinimumPeriod = Attributes.Pmin;
                result.NotificationParameters.MaximumPeriod = Attributes.Pmax;
                result.NotificationParameters.Step = Attributes.Step;
                result.NotificationParameters.LessThan = Attributes.LessThan;
                result.NotificationParameters.GreaterThan = Attributes.GreaterThan;
            }

            if (Url != null)
            {
                result.Url = Url;
            }
            else
            {
                throw new BadRequestException();
            }

            if (Links != null)
            {
                foreach (Link link in Links)
                {
                    if (link.rel.Equals("object"))
                    {
                        string[] fields = link.href.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < fields.Length - 1; i++)
                        {
                            if (fields[i].Equals("clients") && result.ClientID == Guid.Empty)
                            {
                                result.ClientID = StringUtils.GuidDecode(fields[i + 1]);
                            }
                            else if (fields[i].Equals("objecttypes") && result.ObjectDefinitionID == Guid.Empty)
                            {
                                result.ObjectDefinitionID = StringUtils.GuidDecode(fields[i + 1]);
                            }
                            else if (fields[i].Equals("instances") && result.ObjectID == null)
                            {
                                result.ObjectID = fields[i + 1];
                                break;
                            }
                        }
                    }
                }
            }

            if (result.SubscriptionType == TSubscriptionType.Observation)
            {
                if (result.ClientID == null || result.ObjectDefinitionID == null)
                {
                    throw new BadRequestException();
                }

                if (Property != null)
                {
                    Model.PropertyDefinition propertyDefinition = DataAccessFactory.ObjectDefinitions.GetLookups().GetPropertyDefinitionFromNameOrID(result.ObjectDefinitionID, Property);
                    if (propertyDefinition != null)
                    {
                        result.PropertyDefinitionID = propertyDefinition.PropertyDefinitionID;
                    }
                    else
                    {
                        throw new BadRequestException();
                    }
                }
            }

            return result;
        }
    }
}

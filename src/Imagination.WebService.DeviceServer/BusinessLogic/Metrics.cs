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

using Imagination.DataAccess;
using Imagination.LWM2M;
using Imagination.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.BusinessLogic
{
    internal class Metrics
    {
        public Metrics()
        {
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("DeviceServer.", RouteKeys.CLIENT_METRICS), RouteKeys.CLIENT_METRICS, new DataAccess.MessageArrivedEventHandler(OnUpdateMetrics));

            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("DeviceServer.", RouteKeys.CLIENT_CONNECTED), RouteKeys.CLIENT_CONNECTED, new DataAccess.MessageArrivedEventHandler(UpdateClientsConnectedMetric));
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("DeviceServer.", RouteKeys.CLIENT_DISCONNECTED), RouteKeys.CLIENT_DISCONNECTED, new DataAccess.MessageArrivedEventHandler(UpdateClientsConnectedMetric));
            BusinessLogicFactory.ServiceMessages.Subscribe(string.Concat("DeviceServer.", RouteKeys.CLIENT_CONNECTION_EXPIRED), RouteKeys.CLIENT_CONNECTION_EXPIRED, new DataAccess.MessageArrivedEventHandler(UpdateClientsConnectedMetric));
        }

        public List<ClientMetric> GetMetrics(Guid clientID)
        {
            return DataAccessFactory.Metrics.GetMetrics(clientID);
        }

        public List<OrganisationMetric> GetMetrics(int organisationID)
        {
            return DataAccessFactory.Metrics.GetMetrics(organisationID);
        }

        public ClientMetric GetMetric(Guid clientID, string metricName)
        {
            return DataAccessFactory.Metrics.GetMetric(clientID, metricName);
        }

        public OrganisationMetric GetMetric(int organisationID, string metricName)
        {
            return DataAccessFactory.Metrics.GetMetric(organisationID, metricName);
        }

        private void OnUpdateMetrics(string server, ServiceEventMessage message)
        {
            Guid clientID = StringUtils.GuidDecode((string)message.Parameters["ClientID"]);
            int organisationID = (int)((long)message.Parameters["OrganisationID"]);

            List <ClientMetric> metrics = (List<ClientMetric>)message.Parameters["Metrics"];

            foreach (ClientMetric parameter in metrics)
            {
                string metricName = parameter.Name;
                long oldValue = 0;
                long newValue = parameter.Value;
                ClientMetric clientMetric = DataAccessFactory.Metrics.GetMetric(clientID, metricName);
                if (clientMetric == null)
                {
                    clientMetric = new ClientMetric();
                    clientMetric.Name = metricName;
                    clientMetric.Value = newValue;
                    clientMetric.ClientID = clientID;
                    clientMetric.Incremental = parameter.Incremental;
                    DataAccessFactory.Metrics.SaveMetric(clientMetric, TObjectState.Add);
                }
                else
                {
                    oldValue = clientMetric.Value;
                    if (clientMetric.Incremental)
                    {
                        clientMetric.Value += newValue;
                        newValue = clientMetric.Value;
                    }
                    else
                    {
                        clientMetric.Value = newValue;
                    }
                    DataAccessFactory.Metrics.SaveMetric(clientMetric, TObjectState.Update);
                }

                long change = newValue - oldValue;
                OrganisationMetric organisationMetric = DataAccessFactory.Metrics.GetMetric(organisationID, metricName);
                if (organisationMetric == null)
                {
                    organisationMetric = new OrganisationMetric();
                    organisationMetric.Name = metricName;
                    organisationMetric.Value = change;
                    organisationMetric.OrganisationID = organisationID;
                    DataAccessFactory.Metrics.SaveMetric(organisationMetric, TObjectState.Add);
                }
                else
                {
                    organisationMetric.Value += change;
                    DataAccessFactory.Metrics.SaveMetric(organisationMetric, TObjectState.Update);
                }
            }
        }

        private void UpdateClientsConnectedMetric(string server, ServiceEventMessage message)
        {
            int organisationID = (int)((long)message.Parameters["OrganisationID"]);
            OrganisationMetric clientsConnectedMetric = DataAccessFactory.Metrics.GetMetric(organisationID, MetricNames.NumberClients);
            int numClientsConnected = BusinessLogicFactory.Clients.GetConnectedClients(organisationID).Count;
            if (clientsConnectedMetric == null)
            {
                clientsConnectedMetric = new OrganisationMetric();
                clientsConnectedMetric.Name = MetricNames.NumberClients;
                clientsConnectedMetric.Value = Math.Max(numClientsConnected, 0);
                clientsConnectedMetric.OrganisationID = organisationID;
                DataAccessFactory.Metrics.SaveMetric(clientsConnectedMetric, TObjectState.Add);
            }
            else
            {
                clientsConnectedMetric.Value = numClientsConnected;
                DataAccessFactory.Metrics.SaveMetric(clientsConnectedMetric, TObjectState.Update);
            }
        }
    }
}

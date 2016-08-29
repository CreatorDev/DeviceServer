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
using Imagination.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.DataAccess.MongoDB
{
    public class DALMetrics : DALMongoBase, IDALMetrics
    {
        private const string DATABASE_NAME = "Metrics";
        private const string ORGANISATION_METRICS_COLLECTION = "OrganisationMetric";
        private const string CLIENT_METRICS_COLLECTION = "ClientMetric";

        //private GenericCache<Guid, Metric> _Metrics;

        public DALMetrics()
        {
            //_Metrics = new GenericCache<Guid, Metric>(1000);
            //SetupNotification("Metric", new NotificationEventHandler(OnNotification));
        }

        /*private void OnNotification(object sender, NotificationEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.ID))
                _Metrics.Remove(StringUtils.GuidDecode(e.ID));
        }*/

        public void SaveMetric(OrganisationMetric metric, TObjectState state)
        {
            Dictionary<string, BsonValue> organisationMetricValues = new Dictionary<string, BsonValue>();
            SaveMetric(metric, state, metric.OrganisationID, ORGANISATION_METRICS_COLLECTION, organisationMetricValues);
        }

        public void SaveMetric(ClientMetric metric, TObjectState state)
        {
            Dictionary<string, BsonValue> clientMetricValues = new Dictionary<string, BsonValue>();
            clientMetricValues.Add("Incremental", metric.Incremental);
            SaveMetric(metric, state, metric.ClientID.ToByteArray(), CLIENT_METRICS_COLLECTION, clientMetricValues);
        }

        private void SaveMetric(MetricBase metric, TObjectState state, BsonValue id, string collectionName, Dictionary<string, BsonValue> values)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(collectionName);

            if (state == TObjectState.Add)
            {
                BsonDocument newItem = new BsonDocument();
                BsonHelper.SetValue(newItem, "Name", metric.Name);
                BsonHelper.SetValue(newItem, "Value", metric.Value);
                foreach (KeyValuePair<string, BsonValue> pair in values)
                {
                    newItem[pair.Key] = pair.Value;
                }

                FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.AddToSet("Metrics", newItem);
                UpdateOptions options = new UpdateOptions();
                options.IsUpsert = true;
                collection.UpdateOne(filter, update, options);
            }
            else if (state == TObjectState.Update)
            {
                FilterDefinition<BsonDocument> clientOrOrganisationIDFilter = Builders<BsonDocument>.Filter.Eq("_id", id);
                FilterDefinition<BsonDocument> metricNameFilter = Builders<BsonDocument>.Filter.Eq("Metrics.Name", metric.Name);
                FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.And(clientOrOrganisationIDFilter, metricNameFilter);
                UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Set("Metrics.$.Value", metric.Value);
                collection.UpdateOne(filter, update);
            }
            else if (state == TObjectState.Delete)
            {
                FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", id);
                UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.PullFilter("Metrics", Builders<BsonDocument>.Filter.Eq("Name", metric.Name));
                collection.UpdateOne(filter, update);
            }
        }

        public List<OrganisationMetric> GetMetrics(int organisationID)
        {
            List<OrganisationMetric> result = new List<OrganisationMetric>();
            IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(ORGANISATION_METRICS_COLLECTION);
            BsonDocument doc = collection.Find(Builders<BsonDocument>.Filter.Eq("_id", organisationID)).FirstOrDefault();
            if (doc != null)
            {
                foreach (BsonDocument embeddedDocument in BsonHelper.GetArray(doc, "Metrics"))
                {
                    result.Add(LoadOrganisationMetric(organisationID, embeddedDocument));
                }
            }
            return result;
        }

        public List<ClientMetric> GetMetrics(Guid clientID)
        {
            List<ClientMetric> result = new List<ClientMetric>();
            IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(CLIENT_METRICS_COLLECTION);
            BsonDocument doc = collection.Find(Builders<BsonDocument>.Filter.Eq("_id", clientID.ToByteArray())).FirstOrDefault();
            if (doc != null)
            {
                foreach(BsonDocument embeddedDocument in BsonHelper.GetArray(doc, "Metrics"))
                {
                    result.Add(LoadClientMetric(clientID, embeddedDocument));
                }
            }
            return result;
        }

        private ClientMetric LoadClientMetric(Guid clientID, BsonDocument item)
        {
            ClientMetric result = new ClientMetric();
            result.ClientID = clientID;
            result.Incremental = BsonHelper.GetBoolean(item, "Incremental");
            LoadMetricBase(result, item);
            return result;
        }
        private OrganisationMetric LoadOrganisationMetric(int organisationID, BsonDocument item)
        {
            OrganisationMetric result = new OrganisationMetric();
            result.OrganisationID = organisationID;
            LoadMetricBase(result, item);
            return result;
        }

        private void LoadMetricBase(MetricBase result, BsonDocument item)
        {
            result.Name = BsonHelper.GetString(item, "Name");
            result.Value = BsonHelper.GetLong(item, "Value").Value;
        }

        private BsonDocument GetMetricDocument(BsonValue id, string metricName, string collectionName)
        {
            BsonDocument metricDocument = null;

            IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(collectionName);

            // FIXME: Filter by ID + MetricName rather than doing a linear search
            FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            BsonDocument doc = collection.Find(filter).FirstOrDefault();
            if (doc != null)
            {
                foreach (BsonDocument embeddedDocument in (doc["Metrics"] as BsonArray))
                {
                    if (metricName.Equals(BsonHelper.GetString(embeddedDocument, "Name")))
                    {
                        metricDocument = embeddedDocument;
                        break;
                    }
                }
            }
            return metricDocument;
        }

        ClientMetric IDALMetrics.GetMetric(Guid clientID, string metricName)
        {
            ClientMetric result = null;

            BsonDocument document = GetMetricDocument(clientID.ToByteArray(), metricName, CLIENT_METRICS_COLLECTION);
            if (document != null)
            {
                result = LoadClientMetric(clientID, document);
            }

            return result;
        }

        public OrganisationMetric GetMetric(int organisationID, string metricName)
        {
            OrganisationMetric result = null;

            BsonDocument document = GetMetricDocument(organisationID, metricName, ORGANISATION_METRICS_COLLECTION);
            if (document != null)
            {
                result = LoadOrganisationMetric(organisationID, document);
            }

            return result;
        }
    }
}

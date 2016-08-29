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
using Imagination.Model.Subscriptions;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Imagination.DataAccess.MongoDB
{
    public class DALSubscriptions : DALMongoBase, IDALSubscriptions
    {
        private const string DATABASE_NAME = "Subscriptions";
        private const string COLLECTION_NAME = "Subscription";

        private GenericCache<Guid, Subscription> _Subscriptions;

        public DALSubscriptions()
        {
            _Subscriptions = new GenericCache<Guid, Subscription>(1000);
            SetupNotification(COLLECTION_NAME, new NotificationEventHandler(OnNotification));
        }

        private List<Subscription> GetSubscriptions(FilterDefinition<BsonDocument> filter)
        {
            List<Subscription> result = new List<Subscription>();
            IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(COLLECTION_NAME);
            IAsyncCursor<BsonDocument> mongoCursor = collection.FindSync(filter);
            while (mongoCursor.MoveNext())
            {
                foreach (BsonDocument item in mongoCursor.Current)
                {
                    result.Add(LoadSubscription(item));
                }
            }
            return result;
        }

        public Subscription GetSubscription(Guid subscriptionID)
        {
            return GetSubscriptions(Builders<BsonDocument>.Filter.Eq("_id", subscriptionID.ToByteArray())).FirstOrDefault();
        }

        public List<Subscription> GetSubscriptions(int organisationID)
        {
            return GetSubscriptions(Builders<BsonDocument>.Filter.Eq("OrganisationID", organisationID));
        }

        public List<Subscription> GetSubscriptions(Guid clientID)
        {
            return GetSubscriptions(Builders<BsonDocument>.Filter.Eq("ClientID", clientID.ToByteArray()));
        }

        private Subscription LoadSubscription(BsonDocument item)
        {
            Subscription result = new Subscription();
            result.SubscriptionID = BsonHelper.GetGuid(item, "_id");
            result.OrganisationID = BsonHelper.GetInt32(item, "OrganisationID");
            result.ClientID = BsonHelper.GetGuid(item, "ClientID");
            result.ObjectDefinitionID = BsonHelper.GetGuid(item, "DefinitionID");
            result.ObjectID = BsonHelper.GetString(item, "ObjectID");
            result.SubscriptionType = (TSubscriptionType)BsonHelper.GetInt32(item, "SubscriptionType");
            result.PropertyDefinitionID = BsonHelper.GetGuid(item, "PropertyDefinitionID");
            result.Url = BsonHelper.GetString(item, "Url");
            result.AcceptContentType = BsonHelper.GetString(item, "AcceptContentType");

            byte[] serialisedNotificationParameters = StringUtils.Decode(BsonHelper.GetString(item, "NotificationParameters"));

            if (serialisedNotificationParameters != null)
            {
                result.NotificationParameters = NotificationParameters.Deserialise(new MemoryStream(serialisedNotificationParameters));
            }

            return result;
        }

        private void OnNotification(object sender, NotificationEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.ID))
                _Subscriptions.Remove(StringUtils.GuidDecode(e.ID));
        }

        public void SaveSubscription(Subscription subscription, TObjectState state)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(COLLECTION_NAME);
            EnsureIndexExists<BsonDocument>(collection, "OrganisationID");
            EnsureIndexExists<BsonDocument>(collection, "ClientID");

            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", subscription.SubscriptionID.ToByteArray());
            if ((state == TObjectState.Add) || (state == TObjectState.Update))
            {
                BsonDocument doc = new BsonDocument();
                BsonHelper.SetValue(doc, "_id", subscription.SubscriptionID);
                BsonHelper.SetValue(doc, "OrganisationID", subscription.OrganisationID);

                BsonHelper.SetValue(doc, "ClientID", subscription.ClientID);
                BsonHelper.SetValue(doc, "DefinitionID", subscription.ObjectDefinitionID);
                BsonHelper.SetValue(doc, "ObjectID", subscription.ObjectID);

                BsonHelper.SetValue(doc, "SubscriptionType", (int)subscription.SubscriptionType);
                BsonHelper.SetValue(doc, "PropertyDefinitionID", subscription.PropertyDefinitionID);
                BsonHelper.SetValue(doc, "Url", subscription.Url);
                BsonHelper.SetValue(doc, "AcceptContentType", subscription.AcceptContentType);

                if (subscription.NotificationParameters != null)
                {
                    MemoryStream stream = new MemoryStream();
                    subscription.NotificationParameters.Serialise(stream);
                    BsonHelper.SetValue(doc, "NotificationParameters", StringUtils.Encode(stream.ToArray()));
                }

                UpdateOptions options = new UpdateOptions();
                options.IsUpsert = true;
                collection.ReplaceOne(query, doc, options);
            }
            else if (state == TObjectState.Delete)
            {
                collection.DeleteOne(query);
            }
            BroadcastTableChange(COLLECTION_NAME, StringUtils.GuidEncode(subscription.SubscriptionID));
        }
    }
}
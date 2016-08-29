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
using System.Text;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using Imagination.Model;

namespace Imagination.DataAccess.MongoDB
{
    public class DALClients : DALMongoBase, IDALClients
    {
        private const string DATABASE_NAME = "LWM2MClients";

        private HashSet<Guid> _CachedBlackListedClients;

        public DALClients()
        {
            _CachedBlackListedClients = new HashSet<Guid>();
            SetupNotification("BlacklistedClient", new NotificationEventHandler(OnBlackListedClientNotification));
        }

        private Client LoadClientFromDoc(BsonDocument doc)
        {
            Client result = null;
            if (doc != null)
            {
                result = new Client();
                result.ClientID = BsonHelper.GetGuid(doc, "_id");
                result.Name = BsonHelper.GetString(doc, "Name");
                result.OrganisationID = BsonHelper.GetInt32(doc, "OrganisationID");
                result.Lifetime = BsonHelper.GetDateTime(doc, "Lifetime");
                string versionText = BsonHelper.GetString(doc, "Version");
                Version version;
                if (Version.TryParse(versionText, out version))
                    result.Version = version;
                result.BindingMode = (TBindingMode)BsonHelper.GetInt32(doc, "BindingMode");
                result.SMSNumber = BsonHelper.GetString(doc, "SMSNumber");
                result.Server = BsonHelper.GetString(doc, "Server");
                result.LastActivityTime = BsonHelper.GetDateTime(doc, "LastActivityTime");
                result.LastUpdateActivityTime = result.LastActivityTime;
                if (doc.Contains("SupportedTypes"))
                {
                    BsonArray array = doc["SupportedTypes"].AsBsonArray;
                    foreach (BsonValue arrayItem in array)
                    {
                        BsonDocument supportedTypeDoc = arrayItem.AsBsonDocument;
                        if (supportedTypeDoc != null)
                        {
                            ObjectType supportedType = new ObjectType();
                            supportedType.ObjectTypeID = BsonHelper.GetInt32(supportedTypeDoc, "_id");
                            supportedType.Path = BsonHelper.GetString(supportedTypeDoc, "Path");
                            if (supportedTypeDoc.Contains("Instances"))
                            {
                                BsonArray instances = supportedTypeDoc["Instances"].AsBsonArray;
                                foreach (BsonValue instance in instances)
                                {
                                    supportedType.Instances.Add(instance.AsInt32);
                                }
                            }
                            if (result.SupportedTypes == null)
                                result.SupportedTypes = new ObjectTypes();
                            result.SupportedTypes.AddObjectType(supportedType);
                        }
                    }
                }
            }
            return result;
        }

        public List<Client> GetClients(int organisationID)
        {
            List<Client> clients = new List<Client>();
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Client");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("OrganisationID", organisationID);
            IAsyncCursor<BsonDocument> mongoCursor = collection.FindSync(query);
            while (mongoCursor.MoveNext())
            {
                foreach (BsonDocument doc in mongoCursor.Current)
                {
                    Client client = LoadClientFromDoc(doc);
                    if (client != null)
                    {
                        clients.Add(client);
                    }
                }
            }
            return clients;
        }

        public List<Client> GetConnectedClients(int organisationID)
        {
            List<Client> clients = new List<Client>();
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Client");
            FilterDefinition<BsonDocument> organisationFilter = Builders<BsonDocument>.Filter.Eq("OrganisationID", organisationID);
            FilterDefinition<BsonDocument> connectedFilter = Builders<BsonDocument>.Filter.Gt("Lifetime", DateTime.UtcNow);

            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.And(organisationFilter, connectedFilter);
            IAsyncCursor<BsonDocument> mongoCursor = collection.FindSync(query);
            while (mongoCursor.MoveNext())
            {
                foreach (BsonDocument doc in mongoCursor.Current)
                {
                    Client client = LoadClientFromDoc(doc);
                    if (client != null)
                    {
                        clients.Add(client);
                    }
                }
            }
            return clients;
        }

        public Client GetClient(Guid clientID)
        {
            Client result = null;
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Client");
            BsonDocument doc = collection.Find(Builders<BsonDocument>.Filter.Eq("_id", clientID.ToByteArray())).FirstOrDefault();
            result = LoadClientFromDoc(doc);
            return result;
        }

        public void SaveClient(Client client, TObjectState state)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Client");
            EnsureIndexExists<BsonDocument>(collection, "OrganisationID");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", client.ClientID.ToByteArray());
            if ((state == TObjectState.Add) || (state == TObjectState.Update))
            {
                BsonDocument doc = new BsonDocument();
                BsonHelper.SetValue(doc, "_id", client.ClientID);
                BsonHelper.SetValue(doc, "Name", client.Name);
                BsonHelper.SetValue(doc, "OrganisationID", client.OrganisationID);
                BsonHelper.SetValue(doc, "Lifetime", client.Lifetime);
                BsonHelper.SetValue(doc, "Version", client.Version.ToString());
                BsonHelper.SetValue(doc, "BindingMode", (int)client.BindingMode);
                BsonHelper.SetValue(doc, "SMSNumber", client.SMSNumber);
                BsonHelper.SetValue(doc, "Server", client.Server);
                BsonHelper.SetValue(doc, "LastActivityTime", client.LastActivityTime);
                if (client.SupportedTypes.Count > 0)
                {
                    BsonArray array = new BsonArray();
                    foreach (ObjectType supportedType in client.SupportedTypes)
                    {
                        BsonDocument supportedTypeDoc = new BsonDocument();
                        BsonHelper.SetValue(supportedTypeDoc, "_id", supportedType.ObjectTypeID);
                        BsonHelper.SetValue(supportedTypeDoc, "Path", supportedType.Path);
                        if (supportedType.Instances.Count > 0)
                        {
                            BsonArray instances = new BsonArray();
                            foreach (int instance in supportedType.Instances)
                            {
                                instances.Add(instance);
                            }
                            supportedTypeDoc.Add("Instances", instances);
                        }
                        array.Add(supportedTypeDoc);
                    }
                    doc.Add("SupportedTypes", array);
                }
                UpdateOptions options = new UpdateOptions();
                options.IsUpsert = true;
                collection.ReplaceOne(query, doc, options);
            }
            else if (state == TObjectState.Delete)
            {
                collection.DeleteOne(query);
            }
        }

        public void UpdateClientActivity(Guid clientID, DateTime activityTime)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Client");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", clientID.ToByteArray());
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Set("LastActivityTime", activityTime);
            collection.UpdateOne(query, update);
        }

        public void UpdateClientLifetime(Guid clientID, DateTime lifeTime)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Client");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", clientID.ToByteArray());
            UpdateDefinition<BsonDocument> update = Builders<BsonDocument>.Update.Set("Lifetime", lifeTime);
            collection.UpdateOne(query, update);
        }

        public List<Guid> GetBlacklistedClientIDs(int organisationID)
        {
            List<Guid> blacklistedClientIDs = new List<Guid>();
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("BlacklistedClient");
            FilterDefinition <BsonDocument> query = Builders<BsonDocument>.Filter.Eq("OrganisationID", organisationID);
            IAsyncCursor<BsonDocument> mongoCursor = collection.FindSync(query);
            while (mongoCursor.MoveNext())
            {
                foreach (BsonDocument doc in mongoCursor.Current)
                {
                    Guid clientID = BsonHelper.GetGuid(doc, "_id");
                    blacklistedClientIDs.Add(clientID);
                }
            }
            return blacklistedClientIDs;
        }

        public bool IsBlacklisted(Guid clientID)
        {
            bool result = _CachedBlackListedClients.Contains(clientID);
            if (!result)
            {
                IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
                IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("BlacklistedClient");
                BsonDocument doc = collection.Find(Builders<BsonDocument>.Filter.Eq("_id", clientID.ToByteArray())).FirstOrDefault();
                if (doc != null)
                {
                    _CachedBlackListedClients.Add(clientID);
                    result = true;
                }
            }
            return result;
        }

        public void SaveBlacklistedClient(Client client, TObjectState state)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("BlacklistedClient");
            EnsureIndexExists<BsonDocument>(collection, "OrganisationID");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", client.ClientID.ToByteArray());
            if ((state == TObjectState.Add) || (state == TObjectState.Update))
            {
                BsonDocument doc = new BsonDocument();
                BsonHelper.SetValue(doc, "_id", client.ClientID);
                BsonHelper.SetValue(doc, "OrganisationID", client.OrganisationID);
                UpdateOptions options = new UpdateOptions();
                options.IsUpsert = true;
                collection.ReplaceOne(query, doc, options);
            }
            else if (state == TObjectState.Delete)
            {
                collection.DeleteOne(query);
            }
            BroadcastTableChange("BlacklistedClient", StringUtils.GuidEncode(client.ClientID));
        }

        private void OnBlackListedClientNotification(object sender, NotificationEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.ID))
                _CachedBlackListedClients.Remove(StringUtils.GuidDecode(e.ID));
        }
    }
}

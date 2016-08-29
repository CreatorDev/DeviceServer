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
    public class DALConfiguration : DALMongoBase, IDALConfiguration
    {
        private const string DATABASE_NAME = "Configuration";
        private List<BootstrapServer> _CachedBootstrapServers;

        public DALConfiguration()
        {
            SetupNotification("BootstrapServer", new NotificationEventHandler(OnNotification));
        }

        public void AllocateBootstrapServer(int organisationID, BootstrapServer bootstrapServer)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("OrganisationBootstrapServer");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", organisationID);
            BsonDocument doc = new BsonDocument();
            BsonHelper.SetValue(doc, "_id", organisationID);
            BsonHelper.SetValue(doc, "Url", bootstrapServer.Url);
            UpdateOptions options = new UpdateOptions();
            options.IsUpsert = true;
            collection.ReplaceOne(query, doc, options);
        }

        public BootstrapServer GetBootstrapServer(int organisationID)
        {
            BootstrapServer result = null;
            IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("OrganisationBootstrapServer");
            BsonDocument doc = collection.Find(Builders<BsonDocument>.Filter.Eq("_id", organisationID)).FirstOrDefault();
            if (doc != null)
            {
                string url = BsonHelper.GetString(doc, "Url");
                List<BootstrapServer> bootstrapServers = GetBootstrapServers();
                foreach (BootstrapServer item in bootstrapServers)
                {
                    if (string.Compare(url, item.Url,true) == 0)
                    {
                        result = item;
                        break;
                    }
                }
            }
            return result;
        }


        public List<BootstrapServer> GetBootstrapServers()
        {
            List<BootstrapServer> result = _CachedBootstrapServers;
            if (result == null)
            {
                lock (this)
                {
                    result = _CachedBootstrapServers;
                    if (result == null)
                    {
                        result = new List<BootstrapServer>();
                        IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
                        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("BootstrapServer");
                        IAsyncCursor<BsonDocument> mongoCursor = collection.FindSync(new BsonDocument());
                        while (mongoCursor.MoveNext())
                        {
                            foreach (BsonDocument item in mongoCursor.Current)
                            {
                                BootstrapServer bootstrapServer = new BootstrapServer();
                                bootstrapServer.Url = BsonHelper.GetString(item, "_id");
                                if (item.Contains("ServerIdentities"))
                                {
                                    BsonArray array = item["ServerIdentities"].AsBsonArray;
                                    foreach (BsonValue arrayItem in array)
                                    {
                                        BsonDocument pskIdentityDoc = arrayItem.AsBsonDocument;
                                        if (pskIdentityDoc != null)
                                        {
                                            PSKIdentity pskIdentity = new PSKIdentity();
                                            pskIdentity.Identity = BsonHelper.GetString(pskIdentityDoc, "_id");
                                            pskIdentity.Secret = BsonHelper.GetString(pskIdentityDoc, "Secret");
                                            bootstrapServer.AddServerIdentity(pskIdentity);
                                        }
                                    }
                                }
                                if (item.Contains("ServerCertificate"))
                                {
                                    BsonDocument serverCertificateDoc = item["ServerCertificate"].AsBsonDocument;
                                    if (serverCertificateDoc != null)
                                    {
                                        bootstrapServer.ServerCertificate = new Certificate();
                                        bootstrapServer.ServerCertificate.CertificateFormat = (TCertificateFormat)BsonHelper.GetInt32(serverCertificateDoc, "_id");
                                        bootstrapServer.ServerCertificate.RawCertificate = BsonHelper.GetString(serverCertificateDoc, "RawCertificate");
                                    }
                                }
                                result.Add(bootstrapServer);
                            }
                        }
                        _CachedBootstrapServers = result;
                    }
                }
            }
            return result;
        }

        private void OnNotification(object sender, NotificationEventArgs e)
        {
            _CachedBootstrapServers = null;
        }

        public void SaveBootstrapServer(BootstrapServer bootstrapServer, TObjectState state)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("BootstrapServer");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", bootstrapServer.Url);
            if ((state == TObjectState.Add) || (state == TObjectState.Update))
            {
                BsonDocument doc = new BsonDocument();
                BsonHelper.SetValue(doc, "_id", bootstrapServer.Url);
                if (bootstrapServer.ServerIdentities != null && bootstrapServer.ServerIdentities.Count > 0)
                {
                    BsonArray array = new BsonArray();
                    foreach (PSKIdentity pskIdentity in bootstrapServer.ServerIdentities)
                    {
                        BsonDocument pskIdentityDoc = new BsonDocument();
                        BsonHelper.SetValue(pskIdentityDoc, "_id", pskIdentity.Identity);
                        BsonHelper.SetValue(pskIdentityDoc, "Secret", pskIdentity.Secret);
                        array.Add(pskIdentityDoc);
                    }
                    doc.Add("ServerIdentities", array);
                }
                if (bootstrapServer.ServerCertificate != null)
                {
                    BsonDocument serverCertificateDoc = new BsonDocument();
                    BsonHelper.SetValue(serverCertificateDoc, "_id", (int)bootstrapServer.ServerCertificate.CertificateFormat);
                    BsonHelper.SetValue(serverCertificateDoc, "RawCertificate", bootstrapServer.ServerCertificate.RawCertificate);
                    doc.Add("ServerCertificate", serverCertificateDoc);
                }
                UpdateOptions options = new UpdateOptions();
                options.IsUpsert = true;
                collection.ReplaceOne(query, doc, options);
            }
            else if (state == TObjectState.Delete)
            {
                collection.DeleteOne(query);
            }
            BroadcastTableChange("BootstrapServer", string.Empty);
        }

    }
}

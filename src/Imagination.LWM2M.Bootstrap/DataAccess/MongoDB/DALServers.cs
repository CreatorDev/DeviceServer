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
using MongoDB.Driver;
using MongoDB.Bson;
using Imagination.Model;

namespace Imagination.DataAccess.MongoDB
{
    internal class DALServers : DALMongoBase, IDALServers
    {
        private const string DATABASE_NAME = "LWM2MServers";
        private List<Server> _CachedLWM2MServers;

        public DALServers()
        {
            SetupNotification("LWM2MServer", new NotificationEventHandler(OnNotification));
        }

        public List<Server> GetServers()
        {
            List<Server> result = _CachedLWM2MServers;
            if (result == null)
            {
                lock (this)
                {
                    result = _CachedLWM2MServers;
                    if (result == null)
                    {
                        result = new List<Server>();
                        IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
                        IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Server");
                        IAsyncCursor<BsonDocument> mongoCursor = collection.FindSync(new BsonDocument());
                        while (mongoCursor.MoveNext())
                        {
                            foreach (BsonDocument item in mongoCursor.Current)
                            {
                                LWM2MServer lwm2mServer = new LWM2MServer();
                                lwm2mServer.Url = BsonHelper.GetString(item, "_id");
                                lwm2mServer.Lifetime = (uint)BsonHelper.GetInt64(item, "Lifetime");
                                lwm2mServer.DefaultMinimumPeriod = (uint?)BsonHelper.GetLong(item, "DefaultMinimumPeriod");
                                lwm2mServer.DefaultMaximumPeriod = (uint?)BsonHelper.GetLong(item, "DefaultMaximumPeriod");
                                lwm2mServer.DisableTimeout = (uint?)BsonHelper.GetLong(item, "DisableTimeout");
                                lwm2mServer.NotificationStoringWhenOffline = BsonHelper.GetBoolean(item, "NotificationStoringWhenOffline");
                                lwm2mServer.Binding = (TBindingMode)BsonHelper.GetInt32(item, "Binding");
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
                                            lwm2mServer.AddServerIdentity(pskIdentity);
                                        }
                                    }
                                }
                                if (item.Contains("ServerCertificate"))
                                {
                                    BsonDocument serverCertificateDoc = item["ServerCertificate"].AsBsonDocument;
                                    if (serverCertificateDoc != null)
                                    {
                                        lwm2mServer.ServerCertificate = new Certificate();
                                        lwm2mServer.ServerCertificate.CertificateFormat = (TCertificateFormat)BsonHelper.GetInt32(serverCertificateDoc, "_id");
                                        lwm2mServer.ServerCertificate.RawCertificate = BsonHelper.GetString(serverCertificateDoc, "RawCertificate");
                                    }
                                }
                                Server server = new Server(lwm2mServer);
                                server.ShortServerID = result.Count + 1;
                                foreach (Model.Security endPoint in server.EndPoints)
                                {
                                    endPoint.ShortServerID = server.ShortServerID;
                                }
#if DEBUG
                                if (lwm2mServer.Url.ToLower().Contains(Environment.MachineName.ToLower()))
                                {
                                    result.Add(server);
                                }
#else
                                result.Add(server);
#endif
                            }
                        }
                        _CachedLWM2MServers = result;
                    }
                }
            }
            return result;
        }

        private void OnNotification(object sender, NotificationEventArgs e)
        {
            _CachedLWM2MServers = null;
        }

        public void SaveLWM2MServer(LWM2MServer lwm2mServer, TObjectState state)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Server");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", lwm2mServer.Url);
            if ((state == TObjectState.Add) || (state == TObjectState.Update))
            {
                BsonDocument doc = new BsonDocument();
                BsonHelper.SetValue(doc, "_id", lwm2mServer.Url);
                BsonHelper.SetValue(doc, "Lifetime", lwm2mServer.Lifetime);
                BsonHelper.SetValue(doc, "DefaultMinimumPeriod", lwm2mServer.DefaultMinimumPeriod);
                BsonHelper.SetValue(doc, "DefaultMaximumPeriod", lwm2mServer.DefaultMaximumPeriod);
                BsonHelper.SetValue(doc, "DisableTimeout", lwm2mServer.DisableTimeout);
                BsonHelper.SetValue(doc, "NotificationStoringWhenOffline", lwm2mServer.NotificationStoringWhenOffline);
                BsonHelper.SetValue(doc, "Binding", (int)lwm2mServer.Binding);
                if (lwm2mServer.ServerIdentities != null && lwm2mServer.ServerIdentities.Count > 0)
                {
                    BsonArray array = new BsonArray();
                    foreach (PSKIdentity pskIdentity in lwm2mServer.ServerIdentities)
                    {
                        BsonDocument pskIdentityDoc = new BsonDocument();
                        BsonHelper.SetValue(pskIdentityDoc, "_id", pskIdentity.Identity);
                        BsonHelper.SetValue(pskIdentityDoc, "Secret", pskIdentity.Secret);
                        array.Add(pskIdentityDoc);
                    }
                    doc.Add("ServerIdentities", array);
                }
                if (lwm2mServer.ServerCertificate != null)
                {
                    BsonDocument serverCertificateDoc = new BsonDocument();
                    BsonHelper.SetValue(serverCertificateDoc, "_id", (int)lwm2mServer.ServerCertificate.CertificateFormat);
                    BsonHelper.SetValue(serverCertificateDoc, "RawCertificate", lwm2mServer.ServerCertificate.RawCertificate);
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
            BroadcastTableChange("LWM2MServer", string.Empty);
        }

    }
}

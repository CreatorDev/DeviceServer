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
    public class DALIdentities : DALMongoBase, IDALIdentities
    {
        private const string DATABASE_NAME = "PSKIdentities";
        private const string COLLECTION_NAME = "PSKIdentity";

        private GenericCache<string, PSKIdentity> _PSKIdentities;

        public DALIdentities()
        {
            _PSKIdentities = new GenericCache<string, PSKIdentity>(1000);
            SetupNotification(COLLECTION_NAME, new NotificationEventHandler(OnNotification));
        }

        private PSKIdentity LoadPSKIdentityFromDoc(BsonDocument doc)
        {
            PSKIdentity result = null;
            if (doc != null)
            {
                result = new PSKIdentity();
                result.Identity = BsonHelper.GetString(doc, "_id");
                result.Secret = BsonHelper.GetString(doc, "Secret");
                result.OrganisationID = BsonHelper.GetInt32(doc, "OrganisationID");
            }
            return result;
        }

        public List<PSKIdentity> GetPSKIdentities(int organisationID)
        {
            List<PSKIdentity> pskIdentities = new List<PSKIdentity>();
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(COLLECTION_NAME);
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("OrganisationID", organisationID);
            IAsyncCursor<BsonDocument> mongoCursor = collection.FindSync(query);
            while (mongoCursor.MoveNext())
            {
                foreach (BsonDocument doc in mongoCursor.Current)
                {
                    PSKIdentity pskIdentity = LoadPSKIdentityFromDoc(doc);
                    if (pskIdentity != null)
                    {
                        pskIdentities.Add(pskIdentity);
                    }
                }
            }
            return pskIdentities;
        }

        public PSKIdentity GetPSKIdentity(string identity)
        {
            PSKIdentity result = null;
            if (!_PSKIdentities.TryGetItem(identity, out result))
            {
                IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
                IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(COLLECTION_NAME);
                BsonDocument doc = collection.Find(Builders<BsonDocument>.Filter.Eq("_id", identity)).FirstOrDefault();
                result = LoadPSKIdentityFromDoc(doc);
                _PSKIdentities.Add(identity, result);
            }
            return result;
        }

        private void OnNotification(object sender, NotificationEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.ID))
                _PSKIdentities.Remove(e.ID);
        }

        public void SavePSKIdentity(PSKIdentity pskIdentity, TObjectState state)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>(COLLECTION_NAME);
            EnsureIndexExists<BsonDocument>(collection, "OrganisationID");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", pskIdentity.Identity);
            if ((state == TObjectState.Add) || (state == TObjectState.Update))
            {
                BsonDocument doc = new BsonDocument();
                BsonHelper.SetValue(doc, "_id", pskIdentity.Identity);
                BsonHelper.SetValue(doc, "Secret", pskIdentity.Secret);
                BsonHelper.SetValue(doc, "OrganisationID", pskIdentity.OrganisationID);
                UpdateOptions options = new UpdateOptions();
                options.IsUpsert = true;
                collection.ReplaceOne(query, doc, options);
            }
            else if (state == TObjectState.Delete)
            {
                collection.DeleteOne(query);
            }
            BroadcastTableChange(COLLECTION_NAME, pskIdentity.Identity);
        }
    }
}

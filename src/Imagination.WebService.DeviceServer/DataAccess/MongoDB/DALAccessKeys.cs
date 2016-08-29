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
    public class DALAccessKeys: DALMongoBase, IDALAccessKeys
    {
        private const string DATABASE_NAME = "Organisations";
        private GenericCache<string, AccessKey> _AccessKeys;

        public DALAccessKeys()
        {
            _AccessKeys = new GenericCache<string, AccessKey>(1000);
            SetupNotification("AccessKey", new NotificationEventHandler(OnNotification));
        }

        public int GenerateOrganisationID()
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            return base.GetNextSequence(database, "OrganisationID");
        }

        public AccessKey GetAccessKey(string key)
        {
            AccessKey result;
            if (!_AccessKeys.TryGetItem(key, out result))
            {
                IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
                IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("AccessKey");
                BsonDocument doc = collection.Find(Builders<BsonDocument>.Filter.Eq("_id", key)).FirstOrDefault();
                if (doc != null)
                {
                    result = LoadAccessKey(doc);                    
                }
                _AccessKeys.Add(key, result);
            }
            return result;
        }

        public List<AccessKey> GetAccessKeys(int organisationID)
        {
            List<AccessKey> result = new List<AccessKey>();
            IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("AccessKey");
            IAsyncCursor<BsonDocument> mongoCursor = collection.FindSync(Builders<BsonDocument>.Filter.Eq("OrganisationID", organisationID));
            while (mongoCursor.MoveNext())
            {
                foreach (BsonDocument item in mongoCursor.Current)
                {
                    result.Add(LoadAccessKey(item));
                }
            }
            return result;
        }

        private AccessKey LoadAccessKey(BsonDocument item)
        {
            AccessKey result = new AccessKey();
            result.Key = BsonHelper.GetString(item, "_id");
            result.OrganisationID = BsonHelper.GetInt32(item, "OrganisationID");
            result.Name = BsonHelper.GetString(item, "Name");
            result.Secret = BsonHelper.GetString(item, "Secret");
            return result;
        }

        private void OnNotification(object sender, NotificationEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.ID))
                _AccessKeys.Remove(e.ID);
        }

        public void SaveAccessKey(AccessKey accessKey, TObjectState state)
        {
            IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("AccessKey");
            FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", accessKey.Key);
            if ((state == TObjectState.Add) || (state == TObjectState.Update))
            {
                BsonDocument doc = new BsonDocument();
                BsonHelper.SetValue(doc, "_id", accessKey.Key);
                BsonHelper.SetValue(doc, "OrganisationID", accessKey.OrganisationID);
                BsonHelper.SetValue(doc, "Name", accessKey.Name);
                BsonHelper.SetValue(doc, "Secret", accessKey.Secret);
                UpdateOptions options = new UpdateOptions();
                options.IsUpsert = true;
                collection.ReplaceOne(query, doc, options);
            }
            else if (state == TObjectState.Delete)
            {
                collection.DeleteOne(query);
            }
            BroadcastTableChange("AccessKey", accessKey.Key);
        }

    }
}

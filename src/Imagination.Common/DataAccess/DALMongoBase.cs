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
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Concurrent;
using MongoDB.Bson.Serialization.Attributes;

namespace Imagination.DataAccess.MongoDB
{
	public class DALMongoBase
	{
        private class Counter
        {
            [BsonId]
            public string Name { get; set; }
            public int Sequence { get; set; }
        }

        private static object _Lock = new object();
        private static DALChangeNotification _ChangeNotification;
        private GenericCache<string, ConcurrentDictionary<string, object>> _IndexCache = new GenericCache<string, ConcurrentDictionary<string, object>>(1000);

        private const string COUNTERS_COLLECTIONNAME = "counters";


        public void BroadcastTableChange(string tableName, string id)
        {
            CheckChangeNotificationSetup();
            if (_ChangeNotification != null)
                _ChangeNotification.BroadcastTableChange(tableName, id);
        }

        public void BroadcastTableChange(string tableName, bool purge, string id)
        {
            CheckChangeNotificationSetup();
            if (_ChangeNotification != null)
                _ChangeNotification.BroadcastTableChange(tableName, purge, id);
        }

        private void CheckChangeNotificationSetup()
        {
            if (_ChangeNotification == null)
            {
                lock (_Lock)
                {
                    if (_ChangeNotification == null)
                        _ChangeNotification = new DALChangeNotification(ServiceConfiguration.ChangeNotificationServers);
                }
            }
        }

        public void DropDatabase(string databaseName)
		{            
            MongoClient mongoClient = new MongoClient(GetConnectionUrl());
            mongoClient.DropDatabase(databaseName);
		}

		protected void EnsureIndexExists<TDocument>(IMongoCollection<TDocument> collection, params string[] keyNames)
		{
			this.EnsureIndexExists(collection, null, keyNames);
		}

		/// <summary>
		/// Creates and index if it doesn't already exist with options, e.g. ensure unique. Does not compare index options so a change to these must be handled manually.
		/// </summary>
		protected void EnsureIndexExists<TDocument>(IMongoCollection<TDocument> collection, CreateIndexOptions indexOptions, params string[] keyNames)
		{
			ConcurrentDictionary<string, object> indexes;

            List<IndexKeysDefinition<TDocument>> indexFields = new List<IndexKeysDefinition<TDocument>>();
            foreach (string item in  keyNames)
            {
                indexFields.Add(Builders<TDocument>.IndexKeys.Ascending(item));
            }
            IndexKeysDefinition<TDocument> index;
            if (indexFields.Count > 1)
                index = Builders<TDocument>.IndexKeys.Combine(indexFields);
            else
                index = indexFields[0];

            if (!_IndexCache.TryGetItem(collection.CollectionNamespace.FullName, out indexes))
			{
				indexes = new ConcurrentDictionary<string, object>();
				_IndexCache.Add(collection.CollectionNamespace.FullName, indexes);
			}

			StringBuilder indexNameBuilder = new StringBuilder();
			foreach (string item in keyNames)
			{
				if (indexNameBuilder.Length > 0)
					indexNameBuilder.Append("|");
				indexNameBuilder.Append(item);
			}
			string indexName = indexNameBuilder.ToString();
			if (!indexes.ContainsKey(indexName))
			{
				//if (!collection.Indexes.Exists(keyNames))  //nolonger supported, now only can get list of indexes
				{
					if (indexOptions == null)
					{
                        collection.Indexes.CreateOne(index);
					}
					else
					{
						collection.Indexes.CreateOne(index, indexOptions);
					}
				}
				indexes.TryAdd(indexName, null);
			}
		}


        public TDocument FindOne<TDocument>(IMongoCollection<TDocument> collection, FilterDefinition<TDocument> filter)
        {
            TDocument result = default(TDocument);
            IFindFluent<TDocument, TDocument> list = collection.Find<TDocument>(filter);
            list.Limit(1);            
            return result;
        }

        public IMongoDatabase GetDatabase(string databaseName)
		{
			return GetDatabase(databaseName, false);
		}

		public IMongoDatabase GetDatabase(string databaseName, bool usePrimary)
		{
			MongoClient mongoClient = new MongoClient(GetConnectionUrl());
            ReadPreference readPreference = ReadPreference.SecondaryPreferred;
            if (usePrimary)
                readPreference = ReadPreference.Primary;
            return mongoClient.GetDatabase(databaseName, new MongoDatabaseSettings() { ReadPreference = readPreference });
		}

        public virtual MongoUrl GetConnectionUrl()
        {
            return ServiceConfiguration.MongoConnection;
        }
        
        public int GetNextSequence(IMongoDatabase database, string name)
        {
            IMongoCollection<Counter> collection = database.GetCollection<Counter>(COUNTERS_COLLECTIONNAME);
            FindOneAndUpdateOptions<Counter> options = new FindOneAndUpdateOptions<Counter>();
            options.IsUpsert = true;
            options.ReturnDocument = ReturnDocument.After;
            Counter response = collection.FindOneAndUpdate(Builders<Counter>.Filter.Eq(item => item.Name, name), Builders<Counter>.Update.Inc(item => item.Sequence, 1), options);
            return response.Sequence;
        }
      

        public void SetupNotification(string tableName, NotificationEventHandler changeEventHandler)
        {
            CheckChangeNotificationSetup();
            _ChangeNotification.SetupNotification(tableName, changeEventHandler);
        }

        public static void Terminate()
        {
            if (_ChangeNotification != null)
            {
                _ChangeNotification.Terminate();
            }
        }
    }
}

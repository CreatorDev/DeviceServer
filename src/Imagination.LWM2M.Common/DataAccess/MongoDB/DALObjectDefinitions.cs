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
	public class DALObjectDefinitions : DALMongoBase, IDALObjectDefinitions
	{
		private const string DATABASE_NAME = "ObjectDefinitions"; 
		private ObjectDefinitionLookups _CachedLookups;

		public DALObjectDefinitions()
		{
			SetupNotification("ObjectDefinition", new NotificationEventHandler(OnNotification)); 
		}

		public ObjectDefinitionLookups GetLookups()
		{
			ObjectDefinitionLookups result = _CachedLookups;
			if (result == null)
			{
				result = new ObjectDefinitionLookups();
				IMongoDatabase database = GetDatabase(DATABASE_NAME, false);
				LoadObjectDefinition(database, result);
				_CachedLookups = result;
			}
			return result;
		}

        private void LoadObjectDefinition(IMongoDatabase database, ObjectDefinitionLookups lookups)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("ObjectDefinition");
            IAsyncCursor<BsonDocument> mongoCursor = collection.FindSync(new BsonDocument());
            while (mongoCursor.MoveNext())
            {
                foreach (BsonDocument item in mongoCursor.Current)
                {
                    ObjectDefinition objectDefinition = new ObjectDefinition();
                    objectDefinition.ObjectDefinitionID = BsonHelper.GetGuid(item, "_id");
                    objectDefinition.ObjectID = BsonHelper.GetString(item, "ObjectID");
                    objectDefinition.OrganisationID = BsonHelper.GetInteger(item, "OrganisationID");
                    if (objectDefinition.OrganisationID.HasValue && (objectDefinition.OrganisationID.Value == 0))
                        objectDefinition.OrganisationID = null;
                    objectDefinition.Name = BsonHelper.GetString(item, "Name");
                    objectDefinition.MIMEType = BsonHelper.GetString(item, "MIMEType");
                    objectDefinition.SerialisationName = BsonHelper.GetString(item, "SerialisationName");
                    objectDefinition.Singleton = BsonHelper.GetBoolean(item, "Singleton");
                    if (item.Contains("Properties"))
                    {
                        BsonArray array = item["Properties"].AsBsonArray;
                        foreach (BsonValue arrayItem in array)
                        {
                            BsonDocument propertyItem = arrayItem.AsBsonDocument;
                            if (propertyItem != null)
                            {
                                if (objectDefinition.Properties == null)
                                    objectDefinition.Properties = new List<PropertyDefinition>();
                                PropertyDefinition property = new PropertyDefinition();
                                property.PropertyDefinitionID = BsonHelper.GetGuid(propertyItem, "_id");
                                property.PropertyID = BsonHelper.GetString(propertyItem, "PropertyID");
                                property.Name = BsonHelper.GetString(propertyItem, "Name");
                                property.DataType = (TPropertyDataType)propertyItem["DataType"].AsInt32;
                                if (propertyItem.Contains("DataTypeLength"))
                                    property.DataTypeLength = propertyItem["DataTypeLength"].AsInt32;
                                property.MIMEType = BsonHelper.GetString(propertyItem, "MIMEType");
                                property.MinValue = BsonHelper.GetString(propertyItem, "MinValue");
                                property.MaxValue = BsonHelper.GetString(propertyItem, "MaxValue");
                                property.Units = BsonHelper.GetString(propertyItem, "Units");
                                property.IsCollection = BsonHelper.GetBoolean(propertyItem, "IsCollection");
                                property.IsMandatory = BsonHelper.GetBoolean(propertyItem, "IsMandatory");
                                property.Access = (TAccessRight)propertyItem["Access"].AsInt32;
                                if (propertyItem.Contains("SortOrder"))
                                    property.SortOrder = propertyItem["SortOrder"].AsInt32;
                                property.SerialisationName = BsonHelper.GetString(propertyItem, "SerialisationName");
                                property.CollectionItemSerialisationName = BsonHelper.GetString(propertyItem, "CollectionItemSerialisationName");
                                objectDefinition.Properties.Add(property);
                            }
                        }
                    }
                    lookups.AddObjectDefinition(objectDefinition);
                }
            }

		}


		private void OnNotification(object sender, NotificationEventArgs e)
		{
			_CachedLookups = null;
		}

		public void SaveObjectDefinitions(List<ObjectDefinition> items, TObjectState state)
		{
			IMongoDatabase database = GetDatabase(DATABASE_NAME, true);
			IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("ObjectDefinition");
			foreach (ObjectDefinition item in items)
			{
                if (item.ObjectDefinitionID == Guid.Empty)
                    item.ObjectDefinitionID = Guid.NewGuid();
                FilterDefinition<BsonDocument> query = Builders<BsonDocument>.Filter.Eq("_id", item.ObjectDefinitionID.ToByteArray());
                if ((state == TObjectState.Add) || (state == TObjectState.Update))
				{
					BsonDocument doc = new BsonDocument();
					BsonHelper.SetValue(doc, "_id", item.ObjectDefinitionID);
					BsonHelper.SetValue(doc, "ObjectID", item.ObjectID);
                    if (item.OrganisationID.HasValue && (item.OrganisationID.Value == 0))
                        item.OrganisationID = null;
                    BsonHelper.SetValue(doc, "OrganisationID", item.OrganisationID);
                    BsonHelper.SetValue(doc, "Name", item.Name);
					BsonHelper.SetValue(doc, "MIMEType", item.MIMEType);
					BsonHelper.SetValue(doc, "SerialisationName", item.SerialisationName);
					BsonHelper.SetValue(doc, "Singleton", item.Singleton);
					if ((item.Properties != null) && item.Properties.Count > 0)
					{
						BsonArray array = new BsonArray();
						foreach (PropertyDefinition property in item.Properties)
						{
							if (property.PropertyDefinitionID == Guid.Empty)
								property.PropertyDefinitionID = Guid.NewGuid();
							BsonDocument propertyDoc = new BsonDocument();
							BsonHelper.SetValue(propertyDoc, "_id", property.PropertyDefinitionID);
							BsonHelper.SetValue(propertyDoc, "PropertyID", property.PropertyID);
							BsonHelper.SetValue(propertyDoc, "Name", property.Name);
							BsonHelper.SetValue(propertyDoc, "DataType", (int)property.DataType);
							BsonHelper.SetValue(propertyDoc, "DataTypeLength", property.DataTypeLength);
							BsonHelper.SetValue(propertyDoc, "MIMEType", property.MIMEType);
							BsonHelper.SetValue(propertyDoc, "MinValue", property.MinValue);
							BsonHelper.SetValue(propertyDoc, "MaxValue", property.MaxValue);
							BsonHelper.SetValue(propertyDoc, "Units", property.Units);
							BsonHelper.SetValue(propertyDoc, "IsCollection", property.IsCollection);
							BsonHelper.SetValue(propertyDoc, "IsMandatory", property.IsMandatory);
							BsonHelper.SetValue(propertyDoc, "Access", (int)property.Access);
							BsonHelper.SetValue(propertyDoc, "SortOrder", property.SortOrder);
							BsonHelper.SetValue(propertyDoc, "SerialisationName", property.SerialisationName);
							BsonHelper.SetValue(propertyDoc, "CollectionItemSerialisationName", property.CollectionItemSerialisationName);
							array.Add(propertyDoc);
						}
						doc.Add("Properties", array);
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
			BroadcastTableChange("ObjectDefinition", string.Empty);
		}
	}
}

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
using System.IO;
using System.ServiceModel;
using Imagination.Model;
using Imagination.LWM2M;
using Imagination.Service;
using CoAP;
using Microsoft.Extensions.Logging;

namespace Imagination.LWM2M
{
	public class ServerAPI : ILWM2MServerService
	{
        private static DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public void CancelObserveObject(Guid clientID, Guid objectDefinitionID, string instanceID, bool useReset)
        {
            CancelObserveObjectProperty(clientID, objectDefinitionID, instanceID, Guid.Empty, useReset);
        }

        public void CancelObserveObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID, bool useReset)
        {
            try
            {
                LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
                if (client != null)
                {
                    ObjectDefinitionLookups lookups = BusinessLogicFactory.Clients.GetLookups();
                    ObjectDefinition objectDefinition = lookups.GetObjectDefinition(objectDefinitionID);
                    if (objectDefinition != null)
                    {
                        int objectID;
                        if (int.TryParse(objectDefinition.ObjectID, out objectID))
                        {
                            Model.ObjectType objectType = client.SupportedTypes.GetObjectType(objectID);
                            if (objectType != null)
                            {
                                if (propertyDefinitionID != Guid.Empty)
                                {
                                    PropertyDefinition propertyDefinition = objectDefinition.GetProperty(propertyDefinitionID);
                                    if (propertyDefinition != null)
                                    {
                                        client.CancelObserve(objectType, instanceID, propertyDefinition.PropertyID, useReset);
                                    }
                                }
                                else
                                    client.CancelObserve(objectType, instanceID, null, useReset);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public void CancelObserveObjects(Guid clientID, Guid objectDefinitionID, bool useReset)
        {
            CancelObserveObject(clientID, objectDefinitionID, null, useReset);
        }

        public void DeleteClient(Guid clientID)
        {
            LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
            if (client != null)
            {
                BusinessLogicFactory.Clients.DeleteClient(clientID);
                DataAccessFactory.Clients.SaveClient(client, TObjectState.Delete);
            }
        }

        public bool ExecuteResource(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID)
        {
            bool result = false;
            try
            {
                LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
                if (client != null)
                {
                    ObjectDefinitionLookups lookups = BusinessLogicFactory.Clients.GetLookups();
                    ObjectDefinition objectDefinition = lookups.GetObjectDefinition(objectDefinitionID);
                    if (objectDefinition != null)
                    {
                        int objectID;
                        if (int.TryParse(objectDefinition.ObjectID, out objectID))
                        {
                            Model.ObjectType objectType = client.SupportedTypes.GetObjectType(objectID);
                            if (objectType != null)
                            {
                                PropertyDefinition propertyDefinition = objectDefinition.GetProperty(propertyDefinitionID);
                                if (propertyDefinition != null)
                                {
                                    Request request = client.NewPostRequest(objectType, instanceID, propertyDefinition.PropertyID, -1, null);
                                    Response response = client.SendRequest(request).WaitForResponse(LWM2MClient.REQUEST_TIMEOUT);
                                    if (response == null)
                                    {
                                        throw new TimeoutException();
                                    }
                                    else
                                    {
                                        if (response.StatusCode == StatusCode.Changed)
                                        {
                                            result = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return result;
        }

        public List<Client> GetClients()
        {
            return BusinessLogicFactory.Clients.GetClients();
        }

		public DeviceConnectedStatus GetDeviceConnectedStatus(Guid clientID)
		{
			DeviceConnectedStatus result = null;
			LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
			if (client != null)
			{
				result = new DeviceConnectedStatus();
				result.Online = (client.Lifetime > DateTime.UtcNow);
				if (client.LastActivityTime > DateTime.MinValue)
					result.LastActivityTime = client.LastActivityTime;
			}
			return result;
		}

		public Model.Object GetObject(Guid clientID, Guid objectDefinitionID, string instanceID)
		{
            Model.Object result = null;
			try
			{
				LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
				if (client != null)
				{
					ObjectDefinitionLookups lookups = BusinessLogicFactory.Clients.GetLookups();
					ObjectDefinition objectDefinition = lookups.GetObjectDefinition(objectDefinitionID);
					if (objectDefinition != null)
					{
						int objectID;
						if (int.TryParse(objectDefinition.ObjectID, out objectID))
						{
							Model.ObjectType objectType = client.SupportedTypes.GetObjectType(objectID);
							if (objectType != null)
							{
								Request request = client.NewGetRequest(objectType, instanceID);
								Response response = client.SendRequest(request).WaitForResponse(LWM2MClient.REQUEST_TIMEOUT);
								if (response != null && response.StatusCode == StatusCode.Content)
								{
									BusinessLogicFactory.Clients.UpdateClientActivity(client);
									result = ParseObject(objectDefinition, request.Accept, response);
									if (result != null)
										result.InstanceID = instanceID;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
			}
			return result;
		}

        public Property GetObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID)
        {
            Property result = null;
            try
			{
				LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
				if (client != null)
				{
					ObjectDefinitionLookups lookups = BusinessLogicFactory.Clients.GetLookups();
					ObjectDefinition objectDefinition = lookups.GetObjectDefinition(objectDefinitionID);
					if (objectDefinition != null)
					{
						int objectID;
						if (int.TryParse(objectDefinition.ObjectID, out objectID))
						{
							Model.ObjectType objectType = client.SupportedTypes.GetObjectType(objectID);
							if (objectType != null)
							{
                                PropertyDefinition propertyDefinition = objectDefinition.GetProperty(propertyDefinitionID);
                                if (propertyDefinition != null)
                                {
                                    Request request = client.NewGetRequest(objectType, instanceID, propertyDefinition.PropertyID);
                                    Response response = client.SendRequest(request).WaitForResponse(LWM2MClient.REQUEST_TIMEOUT);
                                    if (response != null && response.StatusCode == StatusCode.Content)
                                    {
                                        BusinessLogicFactory.Clients.UpdateClientActivity(client);
										result = ParseProperty(objectDefinition, propertyDefinition, request.Accept, response);
                                    }
                                }
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
			}
            return result;
        }

		public List<Model.Object> GetObjects(Guid clientID, Guid objectDefinitionID)
		{
            List<Model.Object> result = null;
			try
			{
				LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
				if (client != null)
				{
					ObjectDefinitionLookups lookups = BusinessLogicFactory.Clients.GetLookups();
					ObjectDefinition objectDefinition = lookups.GetObjectDefinition(objectDefinitionID);
					if (objectDefinition != null)
					{
						int objectID;
						if (int.TryParse(objectDefinition.ObjectID, out objectID))
						{
							Model.ObjectType objectType = client.SupportedTypes.GetObjectType(objectID);
							if (objectType != null)
							{
								Request request = client.NewGetRequest(objectType, null);
								Response response = client.SendRequest(request).WaitForResponse(LWM2MClient.REQUEST_TIMEOUT);
								if (response != null && response.StatusCode == StatusCode.Content)
								{
									BusinessLogicFactory.Clients.UpdateClientActivity(client);
									result = ParseObjects(objectDefinition, request.Accept, response);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
			}
			return result;
		}


        public void ObserveObject(Guid clientID, Guid objectDefinitionID, string instanceID)
        {
            ObserveObjectProperty(clientID, objectDefinitionID, instanceID, Guid.Empty);
        }

        public void ObserveObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID)
        {
            try
            {
                LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
                if (client != null)
                {
                    ObjectDefinitionLookups lookups = BusinessLogicFactory.Clients.GetLookups();
                    ObjectDefinition objectDefinition = lookups.GetObjectDefinition(objectDefinitionID);
                    if (objectDefinition != null)
                    {
                        int objectID;
                        if (int.TryParse(objectDefinition.ObjectID, out objectID))
                        {
                            Model.ObjectType objectType = client.SupportedTypes.GetObjectType(objectID);
                            if (objectType != null)
                            {
                                if (propertyDefinitionID != Guid.Empty)
                                {
                                    PropertyDefinition propertyDefinition = objectDefinition.GetProperty(propertyDefinitionID);
                                    if (propertyDefinition != null)
                                    {
                                        client.Observe(objectDefinition, objectType, instanceID, propertyDefinition);
                                    }
                                }
                                else
                                {
                                    client.Observe(objectDefinition, objectType, instanceID, null);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
        }

        public void ObserveObjects(Guid clientID, Guid objectDefinitionID)
        {
            ObserveObject(clientID, objectDefinitionID, null);
        }

        private Model.Object ParseObject(ObjectDefinition objectDefinition, int requestContentType, Response response)
		{
            Model.Object result = null;
			int contentType;
			if (response.ContentType == -1)
			{
				contentType = requestContentType;
			}
			else
			{
				contentType = response.ContentType;
			}
			if (contentType == TlvConstant.CONTENT_TYPE_TLV)
			{
				TlvReader reader = new TlvReader(response.Payload);
                result = ObjectUtils.ParseObject(objectDefinition, reader);
			}
			else if (contentType == MediaType.ApplicationJson)
			{
				JsonReader reader = new JsonReader(new MemoryStream(response.Payload));
				result = ObjectUtils.ParseObject(objectDefinition, reader);
			}
			return result;
		}

		private List<Model.Object> ParseObjects(ObjectDefinition objectDefinition, int requestContentType, Response response)
		{
            List<Model.Object> result = new List<Model.Object>();
			int contentType;
			if (response.ContentType == -1)
			{
				contentType = requestContentType;
			}
			else
			{
				contentType = response.ContentType;
			}
			if (contentType == TlvConstant.CONTENT_TYPE_TLV)
			{
				TlvReader reader = new TlvReader(response.Payload);
				while (reader.Read())
				{
					if (reader.TlvRecord.TypeIdentifier == TTlvTypeIdentifier.ObjectInstance)
					{
						TlvReader objectReader = new TlvReader(reader.TlvRecord.Value);
                        Model.Object item = ObjectUtils.ParseObject(objectDefinition, objectReader);
						if (item != null)
						{
							item.InstanceID = reader.TlvRecord.Identifier.ToString();
							result.Add(item);
						}
					}
				}
			}
			return result;
		}

		private Property ParseProperty(ObjectDefinition objectDefinition, PropertyDefinition propertyDefinition, int requestContentType, Response response)
        {
            Property result = null;
			int contentType;
			if (response.ContentType == -1)
			{
				contentType = requestContentType;
			}
			else
			{
				contentType = response.ContentType;
			}
            if (contentType == TlvConstant.CONTENT_TYPE_TLV)
            {
                TlvReader reader = new TlvReader(response.Payload);
                Model.Object lwm2mObject = ObjectUtils.ParseObject(objectDefinition, reader);
                if ((lwm2mObject != null) && (lwm2mObject.Properties.Count > 0))
                {
                    foreach (Property item in lwm2mObject.Properties)
                    {
                        if (item.PropertyDefinitionID == propertyDefinition.PropertyDefinitionID)
                        {
                            result = item;
                            break;
                        }
                    }
                }
            }
            else if ((contentType == MediaType.TextPlain) || (contentType == TlvConstant.CONTENT_TYPE_PLAIN))
            {
                string text = Encoding.UTF8.GetString(response.Payload);
                result = new Property();
                result.PropertyDefinitionID = propertyDefinition.PropertyDefinitionID;
                result.PropertyID = propertyDefinition.PropertyID;
                result.Value = new PropertyValue(ObjectUtils.GetValue(text, propertyDefinition));
            }
			else if ((contentType == MediaType.ApplicationJson) || (contentType == TlvConstant.CONTENT_TYPE_JSON))
			{
				JsonReader reader = new JsonReader(new MemoryStream(response.Payload));
				//LWM2MObject lwm2mObject = ObjectUtils.ParseObject(objectDefinition, reader);
				//if ((lwm2mObject != null) && (lwm2mObject.Properties.Count > 0))
				//{
				//    foreach (LWM2MProperty item in lwm2mObject.Properties)
				//    {
				//        if (item.PropertyDefinitionID == propertyDefinition.PropertyDefinitionID)
				//        {
				//            result = item;
				//            break;
				//        }
				//    }
				//}
				result = ObjectUtils.ParseProperty(propertyDefinition, reader);
			}
            return result;
        }

		public string SaveObject(Guid clientID, Model.Object item, TObjectState state)
		{
			string result = null;
			LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
			if (client == null)
			{
                ApplicationEventLog.Write(LogLevel.Warning, string.Concat("SaveObject - Client not found ", clientID.ToString()));
				throw new NoLongerAvailableException("Device not connected");
			}
			else
			{
				ObjectDefinitionLookups lookups = BusinessLogicFactory.Clients.GetLookups();
				ObjectDefinition objectDefinition = lookups.GetObjectDefinition(item.ObjectDefinitionID);
				if (objectDefinition == null)
				{
                    ApplicationEventLog.Write(LogLevel.Warning, string.Concat("SaveObject - Metadata not found ", item.ObjectDefinitionID.ToString(), " client ", client.Address.ToString()));
				}
				else
				{
					int objectID;
					if (int.TryParse(objectDefinition.ObjectID, out objectID))
					{
						Model.ObjectType objectType = client.SupportedTypes.GetObjectType(objectID);
						if (objectType != null)
						{
							Request request = null;
							switch (state)
							{
								case TObjectState.NotChanged:
									break;
								case TObjectState.Add:
									ushort instanceID;
									if (ushort.TryParse(item.InstanceID, out instanceID))
									{
										request = client.NewPostRequest(objectType, null, null, TlvConstant.CONTENT_TYPE_TLV, SerialiseObject(objectDefinition, item, instanceID));
									}
									else
									{
										request = client.NewPostRequest(objectType, null, null, TlvConstant.CONTENT_TYPE_TLV, SerialiseObject(objectDefinition, item));
									}
									break;
								case TObjectState.Update:
									request = client.NewPostRequest(objectType, item.InstanceID, null, TlvConstant.CONTENT_TYPE_TLV, SerialiseObject(objectDefinition, item));
									break;
								case TObjectState.Delete:
									request = client.NewDeleteRequest(objectType, item.InstanceID, null);
									break;
								default:
									break;
							}
                            ApplicationEventLog.Write(LogLevel.Information, string.Concat("SaveObject - Send request ", string.Concat(objectType.Path, "/", item.InstanceID), " client ", client.Address.ToString()));

							Response response = client.SendRequest(request).WaitForResponse(LWM2MClient.REQUEST_TIMEOUT);
							if (response == null)
							{
								throw new TimeoutException();
							}
							else
							{
								BusinessLogicFactory.Clients.UpdateClientActivity(client);
								if (response.StatusCode == StatusCode.Created)
								{
									if (!string.IsNullOrEmpty(response.LocationPath) && (response.LocationPath.StartsWith(request.UriPath)))
									{
										int startIndex = request.UriPath.Length + 1;
										if (startIndex < response.LocationPath.Length)
											result = response.LocationPath.Substring(startIndex);
									}
								}
                                else if (response.StatusCode == StatusCode.Changed)
                                {

                                }
                                else if ((response.StatusCode == StatusCode.NotFound) &&  (state == TObjectState.Delete))
                                {

                                }
                                else
                                {
                                    throw new BadRequestException();
                                }
							}
						}
					}
				}
			}
			return result;
		}

		public void SaveObjectProperty(Guid clientID, Guid objectDefinitionID, string instanceID, Property property, TObjectState state)
		{
			LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
			if (client == null)
			{
                ApplicationEventLog.Write(LogLevel.Warning, string.Concat("SaveObject - Client not found ", clientID.ToString()));
				throw new NoLongerAvailableException("Device not connected");
			}
			else
			{
				ObjectDefinitionLookups lookups = BusinessLogicFactory.Clients.GetLookups();
				ObjectDefinition objectDefinition = lookups.GetObjectDefinition(objectDefinitionID);
				if (objectDefinition != null)
				{
					int objectID;
					if (int.TryParse(objectDefinition.ObjectID, out objectID))
					{
						Model.ObjectType objectType = client.SupportedTypes.GetObjectType(objectID);
						if (objectType != null)
						{
							PropertyDefinition propertyDefinition = objectDefinition.GetProperty(property.PropertyDefinitionID);						
							if (propertyDefinition != null)
							{
								byte[] payload = null;
                                int contentType = TlvConstant.CONTENT_TYPE_TLV;
								if (state != TObjectState.Delete)
								{
									if ((property.Value != null) || (property.Values != null))
									{
                                        if ((property.Value != null) && (LWM2MClient.DataFormat == MediaType.TextPlain))
                                        {
                                            contentType = LWM2MClient.DataFormat;
											//contentType = TlvConstant.CONTENT_TYPE_PLAIN;
											string text = SerialiseProperty(propertyDefinition, property);
                                            if (text != null)
                                                payload = Encoding.UTF8.GetBytes(text);
                                        }
                                        else
                                        {
                                            Model.Object lwm2mObject = new Model.Object();
										    lwm2mObject.Properties.Add(property);
										    payload = SerialiseObject(objectDefinition, lwm2mObject);
                                        }
									}
								}
								Request request = null;
								switch (state)
								{
									case TObjectState.NotChanged:
										break;
									case TObjectState.Add:
                                        request = client.NewPostRequest(objectType, instanceID, propertyDefinition.PropertyID, contentType, payload);
										break;
									case TObjectState.Update:
                                        request = client.NewPutRequest(objectType, instanceID, propertyDefinition.PropertyID, contentType, payload);
										break;
									case TObjectState.Delete:
										request = client.NewDeleteRequest(objectType, instanceID, propertyDefinition.PropertyID);
										break;
									default:
										break;
								}
								Response response = client.SendRequest(request).WaitForResponse(LWM2MClient.REQUEST_TIMEOUT);
                                if (response == null)
                                {
                                    throw new TimeoutException();
                                }
                                else
                                {
                                    BusinessLogicFactory.Clients.UpdateClientActivity(client);
                                    if (response.StatusCode != StatusCode.Changed)
                                    {
                                        throw new BadRequestException();
                                    }
                                }
							}
						}
					}
				}
			}
		}


		private byte[] SerialiseObject(ObjectDefinition objectDefinition, Model.Object item)
		{
			byte[] result = null;
			TlvWriter arrayWriter = null;
			MemoryStream arraystream = null;
			using (MemoryStream steam = new MemoryStream())
			{
				TlvWriter writer = new TlvWriter(steam);
				foreach (Property property in item.Properties)
				{
					PropertyDefinition propertyDefinition = objectDefinition.GetProperty(property.PropertyID);
					if (propertyDefinition != null)
					{
						if (propertyDefinition.IsCollection)
						{
							if (property.Values != null)
							{
								ushort identifier;
								if (ushort.TryParse(propertyDefinition.PropertyID, out identifier))
								{
									if (arrayWriter == null)
									{
										arraystream = new MemoryStream();
										arrayWriter = new TlvWriter(arraystream);
									}
									arraystream.SetLength(0);
									foreach (PropertyValue propertyValue in property.Values)
									{
										WriteValue(arrayWriter, TTlvTypeIdentifier.ResourceInstance, propertyDefinition.DataType, propertyValue.PropertyValueID, propertyValue.Value);
									}
									byte[] arrayItems = arraystream.ToArray();
									writer.Write(TTlvTypeIdentifier.MultipleResources, identifier, arrayItems);
								}
							}
						}
						else if (property.Value != null)
						{
							WriteValue(writer, TTlvTypeIdentifier.ResourceWithValue, propertyDefinition.DataType, propertyDefinition.PropertyID, property.Value.Value);
						}
					}
				}
				result = steam.ToArray();
			}
			return result;
		}

		private byte[] SerialiseObject(ObjectDefinition objectDefinition, Model.Object item, ushort objectInstanceID)
		{
			byte[] result = null;
			using (MemoryStream steam = new MemoryStream())
			{
				TlvWriter writer = new TlvWriter(steam);
				byte[] objectTLV = SerialiseObject(objectDefinition, item);
				int length = objectTLV.Length;
				writer.WriteType(TTlvTypeIdentifier.ObjectInstance, objectInstanceID, length);
				steam.Write(objectTLV, 0, length);
				result = steam.ToArray();
			}
			return result;
		}

        private string SerialiseProperty(PropertyDefinition propertyDefinition, Property property)
        {
            string result = null;
            switch (propertyDefinition.DataType)
            {
                case TPropertyDataType.NotSet:
                    break;
                case TPropertyDataType.String:
                    result = property.Value.Value;
                    break;
                case TPropertyDataType.Boolean:
                    bool boolValue;
                    if (bool.TryParse(property.Value.Value, out boolValue))
                    {
                        if (boolValue)
                            result = "1";
                        else
                            result = "0";
                    }
                    break;
                case TPropertyDataType.Integer:
                case TPropertyDataType.Float:
                    result = property.Value.Value;
                    break;
                case TPropertyDataType.DateTime:
                    try
                    {
                        DateTime dateTimeValue = System.Xml.XmlConvert.ToDateTime(property.Value.Value, System.Xml.XmlDateTimeSerializationMode.Utc);
                        TimeSpan diff = dateTimeValue.Subtract(_Epoch);
                        long seconds = (long)diff.TotalSeconds;
                        result = seconds.ToString();
                    }
                    catch
                    {

                    }
                    break;
                case TPropertyDataType.Opaque:
                    result = property.Value.Value;
                    break;
                case TPropertyDataType.Object:
                    break;
                default:
                    break;
            }
            return result;
        }

        public void SetDataFormat(TDataFormat dataFormat)
        {
            switch (dataFormat)
            {
                case TDataFormat.NotSet:
                    break;
                case TDataFormat.PlainText:
                    LWM2MClient.DataFormat = MediaType.TextPlain;
					//LWM2MClient.DataFormat = TlvConstant.CONTENT_TYPE_PLAIN;
                    break;
                case TDataFormat.TLV:
                    LWM2MClient.DataFormat = TlvConstant.CONTENT_TYPE_TLV;
                    break;
                case TDataFormat.JSON:
                    //LWM2MClient.DataFormat = MediaType.ApplicationJson;
					LWM2MClient.DataFormat = TlvConstant.CONTENT_TYPE_JSON;
                    break;
                default:
                    break;
            }
        }

        public bool SetNotificationParameters(Guid clientID, Guid objectDefinitionID, string instanceID, Guid propertyDefinitionID, NotificationParameters notificationParameters)
        {
            bool result = false;
            try
            {
                LWM2MClient client = BusinessLogicFactory.Clients.GetClient(clientID);
                if (client != null)
                {
                    ObjectDefinitionLookups lookups = BusinessLogicFactory.Clients.GetLookups();
                    ObjectDefinition objectDefinition = lookups.GetObjectDefinition(objectDefinitionID);
                    if (objectDefinition != null)
                    {
                        int objectID;
                        if (int.TryParse(objectDefinition.ObjectID, out objectID))
                        {
                            Model.ObjectType objectType = client.SupportedTypes.GetObjectType(objectID);
                            if (objectType != null)
                            {
                                PropertyDefinition propertyDefinition = objectDefinition.GetProperty(propertyDefinitionID);
                                if (propertyDefinition != null)
                                {
                                    Request request = client.NewPutRequest(objectType, instanceID, propertyDefinition.PropertyID, -1, null);
                                    if (notificationParameters.MinimumPeriod.HasValue)
                                        request.AddUriQuery(string.Concat("pmin=",notificationParameters.MinimumPeriod.Value.ToString()));
                                    if (notificationParameters.MaximumPeriod.HasValue)
                                        request.AddUriQuery(string.Concat("pmax=", notificationParameters.MaximumPeriod.Value.ToString()));
                                    if (notificationParameters.GreaterThan.HasValue)
                                        request.AddUriQuery(string.Concat("gt=", notificationParameters.GreaterThan.Value.ToString("0.0")));
                                    if (notificationParameters.LessThan.HasValue)
                                        request.AddUriQuery(string.Concat("lt=", notificationParameters.LessThan.Value.ToString("0.0")));
                                    if (notificationParameters.Step.HasValue)
                                        request.AddUriQuery(string.Concat("stp=", notificationParameters.Step.Value.ToString("0.0")));
                                    Response response = client.SendRequest(request).WaitForResponse(LWM2MClient.REQUEST_TIMEOUT);
                                    if (response == null)
                                    {
                                        throw new TimeoutException();
                                    }
                                    else
                                    {
                                        if (response.StatusCode == StatusCode.Changed)
                                        {
                                            result = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ApplicationEventLog.WriteEntry("Flow", ex.ToString(), System.Diagnostics.EventLogEntryType.Error);
            }
            return result;
        }

		private void WriteValue(TlvWriter writer, TTlvTypeIdentifier typeIdentifier, TPropertyDataType dataType, string instanceID, string value)
		{
			ushort identifier;
			if (ushort.TryParse(instanceID, out identifier))
			{

				switch (dataType)
				{
					case TPropertyDataType.NotSet:
						break;
					case TPropertyDataType.String:
						writer.Write(typeIdentifier, identifier, value);
						break;
					case TPropertyDataType.Boolean:
						bool boolValue;
						if (bool.TryParse(value, out boolValue))
							writer.Write(typeIdentifier, identifier, boolValue);
						break;
					case TPropertyDataType.Integer:
						long longValue;
						if (long.TryParse(value, out longValue))
							writer.Write(typeIdentifier, identifier, longValue);
						break;
					case TPropertyDataType.Float:
						double doubleValue;
						if (double.TryParse(value, out doubleValue))
							writer.Write(typeIdentifier, identifier, doubleValue);
						break;
					case TPropertyDataType.DateTime:
						try
						{
							DateTime dateTimeValue = System.Xml.XmlConvert.ToDateTime(value, System.Xml.XmlDateTimeSerializationMode.Utc);
							writer.Write(typeIdentifier, identifier, dateTimeValue);
						}
						catch
						{

						}
						break;
					case TPropertyDataType.Opaque:
						byte[] buffer = Convert.FromBase64String(value);
						writer.Write(typeIdentifier, identifier, buffer);
						break;
					case TPropertyDataType.Object:
						break;
					default:
						break;
				}
			}
		}
	}
}

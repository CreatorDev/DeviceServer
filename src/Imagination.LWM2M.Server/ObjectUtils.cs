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
using Imagination.Model;

namespace Imagination.LWM2M
{
    internal class ObjectUtils
    {
        private static DateTime _Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static Model.Object ParseObject(ObjectDefinition objectDefinition, TlvReader reader)
        {
            Model.Object result = null;
            while (reader.Read())
            {
                if (reader.TlvRecord.TypeIdentifier == TTlvTypeIdentifier.ObjectInstance)
                {
                    TlvReader objectReader = new TlvReader(reader.TlvRecord.Value);
                    result = ParseObject(objectDefinition, objectReader);
                    if (result != null)
                    {
                        result.InstanceID = reader.TlvRecord.Identifier.ToString();
                    }
                    break;
                }
                if ((reader.TlvRecord.TypeIdentifier != TTlvTypeIdentifier.ObjectInstance) && (reader.TlvRecord.TypeIdentifier != TTlvTypeIdentifier.NotSet))
                {
                    if (result == null)
                    {
                        result = new Model.Object();
                        result.ObjectID = objectDefinition.ObjectID;
                        result.ObjectDefinitionID = objectDefinition.ObjectDefinitionID;
                    }
                    if (reader.TlvRecord.TypeIdentifier == TTlvTypeIdentifier.ResourceWithValue)
                    {
                        string propertyID = reader.TlvRecord.Identifier.ToString();
                        PropertyDefinition property = objectDefinition.GetProperty(propertyID);
                        if (property != null)
                        {
                            Property lwm2mProperty = new Property();
                            lwm2mProperty.PropertyDefinitionID = property.PropertyDefinitionID;
                            lwm2mProperty.PropertyID = property.PropertyID;
                            lwm2mProperty.Value = new PropertyValue(GetValue(reader, property));
                            result.Properties.Add(lwm2mProperty);
                        }
                    }
                    else if (reader.TlvRecord.TypeIdentifier == TTlvTypeIdentifier.MultipleResources)
                    {
                        string propertyID = reader.TlvRecord.Identifier.ToString();
                        PropertyDefinition property = objectDefinition.GetProperty(propertyID);
                        if (property != null)
                        {
                            Property lwm2mProperty = new Property();
                            lwm2mProperty.PropertyDefinitionID = property.PropertyDefinitionID;
                            lwm2mProperty.PropertyID = property.PropertyID;
                            result.Properties.Add(lwm2mProperty);
                            TlvReader arrayReader = new TlvReader(reader.TlvRecord.Value);
                            while (arrayReader.Read())
                            {
                                if (arrayReader.TlvRecord.TypeIdentifier == TTlvTypeIdentifier.ResourceInstance)
                                {
                                    string value = GetValue(arrayReader, property);
                                    if (value != null)
                                    {
                                        if (lwm2mProperty.Values == null)
                                            lwm2mProperty.Values = new List<PropertyValue>();
                                        PropertyValue propertyValue = new PropertyValue();
                                        propertyValue.PropertyValueID = arrayReader.TlvRecord.Identifier.ToString();
                                        propertyValue.Value = value;
                                        lwm2mProperty.Values.Add(propertyValue);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

		public static Model.Object ParseObject(ObjectDefinition objectDefinition, JsonReader reader)
		{
            Model.Object result = null;
			while (reader.Read())
			{
				if ((reader.State == TJsonReaderState.Member) && (string.Compare(reader.Text,"e") == 0))
				{
					if (result == null)
					{
						result = new Model.Object();
						result.ObjectID = objectDefinition.ObjectID;
						result.ObjectDefinitionID = objectDefinition.ObjectDefinitionID;
					}
					if (reader.Read())
					{
						if (reader.State == TJsonReaderState.Array)
						{
							while (reader.Read())
							{
								if (reader.State == TJsonReaderState.Object)
								{
									Property property = ParseProperty(objectDefinition, reader);
									if (property != null)
									{
										bool found = false;
										foreach (Property item in result.Properties)
										{
											if (item.PropertyDefinitionID == property.PropertyDefinitionID)
											{
												if ((item.Values != null) && (property.Values != null))
												{
													item.Values.Add(property.Values[0]);
												}
												found = true;
												break;
											}
										}
										if (!found)
											result.Properties.Add(property);
									}
								}
								if (reader.State == TJsonReaderState.EndArray)
									break;
							}
						}
					}
				}
			}
			return result;
		}

		public static Property ParseProperty(ObjectDefinition objectDefinition, JsonReader reader)
		{
			Property result = new Property();
			string propertyValueID = null;
			while (reader.Read())
			{
				if (reader.State == TJsonReaderState.Member)
				{
					if (string.Compare(reader.Text, "n") == 0)
					{						
						if (reader.Read() && !string.IsNullOrEmpty(reader.Text))
						{
							string[] fields = reader.Text.Split('/');
							PropertyDefinition property = objectDefinition.GetProperty(fields[0]);
							if (property != null)
							{
								result.PropertyDefinitionID = property.PropertyDefinitionID;
								result.PropertyID = property.PropertyID;
								if (fields.Length > 1)
								{
									propertyValueID = fields[1];
									result.Values = new List<PropertyValue>();
								}
							}
						}
					}
					else if ((string.Compare(reader.Text, "v") == 0) || (string.Compare(reader.Text, "sv") == 0))
					{
						if (reader.Read() && !string.IsNullOrEmpty(reader.Text))
						{
							if (string.IsNullOrEmpty(propertyValueID))
							{
                                result.Value = new PropertyValue(reader.Text);
							}
							else
							{
								PropertyValue value = new PropertyValue();
								value.PropertyValueID = propertyValueID;
								value.Value = reader.Text;
								result.Values.Add(value);
							}
						}
					}
					else if (string.Compare(reader.Text, "bv") == 0)
					{
						if (reader.Read())
						{
							if (string.IsNullOrEmpty(propertyValueID))
							{
								result.Value = new PropertyValue(reader.AsBoolean.ToString());
							}
							else
							{
								PropertyValue value = new PropertyValue();
								value.PropertyValueID = propertyValueID;
								value.Value = reader.AsBoolean.ToString();
								result.Values.Add(value);
							}

						}
					}
				}
				if (reader.State == TJsonReaderState.EndObject)
					break;
			}
			return result;
		}

		public static Property ParseProperty(PropertyDefinition propertyDefinition, JsonReader reader)
		{
			Property result = null;
			while (reader.Read())
			{
				if ((reader.State == TJsonReaderState.Member) && (string.Compare(reader.Text, "e") == 0))
				{
					if (result == null)
					{
						result = new Property();
						result.PropertyDefinitionID = propertyDefinition.PropertyDefinitionID;
						result.PropertyID = propertyDefinition.PropertyID;
					}
					if (reader.Read())
					{
						if (reader.State == TJsonReaderState.Array)
						{
							while (reader.Read())
							{
								if (reader.State == TJsonReaderState.Object)
								{
									string propertyValueID = null;
									string value = null; 
									while (reader.Read())
									{
										if (reader.State == TJsonReaderState.Member)
										{
											if (string.Compare(reader.Text, "n") == 0)
											{
												if (reader.Read() && !string.IsNullOrEmpty(reader.Text))
												{
													string[] fields = reader.Text.Split('/');
													if (fields.Length > 1)
													{
														propertyValueID = fields[1];
														result.Values = new List<PropertyValue>();
													}
												}
											}
											else if ((string.Compare(reader.Text, "v") == 0) || (string.Compare(reader.Text, "sv") == 0))
											{
												if (reader.Read() && !string.IsNullOrEmpty(reader.Text))
												{
													value = reader.Text;
												}
											}
											else if (string.Compare(reader.Text, "bv") == 0)
											{
												if (reader.Read())
												{
													value = reader.AsBoolean.ToString();
												}
											}
										}
										if (reader.State == TJsonReaderState.EndObject)
										{
											if (propertyDefinition.IsCollection)
											{
												PropertyValue propertyValue = new PropertyValue();
												propertyValue.PropertyValueID = propertyValueID;
												propertyValue.Value = value;
												result.Values.Add(propertyValue);
											}
											else
											{
												result.Value = new PropertyValue(value);
											}
											break;
										}
									}
								}
								if (reader.State == TJsonReaderState.EndArray)
									break;
							}
						}
					}
				}
			}
			return result;
		}

        public static string GetValue(TlvReader reader, PropertyDefinition property)
        {
            string result = null;
            switch (property.DataType)
            {
                case TPropertyDataType.NotSet:
                    break;
                case TPropertyDataType.String:
                    result = reader.TlvRecord.ValueAsString();
                    break;
                case TPropertyDataType.Boolean:
                    result = reader.TlvRecord.ValueAsBoolean().ToString();
                    break;
                case TPropertyDataType.Integer:
                    result = reader.TlvRecord.ValueAsInt64().ToString();
                    break;
                case TPropertyDataType.Float:
                    result = reader.TlvRecord.ValueAsDouble().ToString();
                    break;
                case TPropertyDataType.DateTime:
                    result = reader.TlvRecord.ValueAsDateTime().ToString(XmlHelper.XMLDATEFORMAT);
                    break;
                case TPropertyDataType.Opaque:
                    result = Convert.ToBase64String(reader.TlvRecord.Value);
                    break;
                case TPropertyDataType.Object:
                    break;
                default:
                    break;
            }
            return result;
        }

        public static string GetValue(string text, PropertyDefinition property)
        {
            string result = null;
            switch (property.DataType)
            {
                case TPropertyDataType.NotSet:
                    break;
                case TPropertyDataType.String:
                    result = text;
                    break;
                case TPropertyDataType.Boolean:
                    if (string.Compare(text, "1") == 0)
                        result = true.ToString();
                    else
                        result = false.ToString();
                    break;
                case TPropertyDataType.Integer:
                case TPropertyDataType.Float:
                    result = text;
                    break;
                case TPropertyDataType.DateTime:
                    long seconds = 0;
					if (long.TryParse(text, out seconds))
                    {
                        result = _Epoch.AddSeconds(seconds).ToString(XmlHelper.XMLDATEFORMAT);
                    }
                    break;
                case TPropertyDataType.Opaque:
                    result = text;
                    break;
                case TPropertyDataType.Object:
                    break;
                default:
                    break;
            }
            return result;
        }

    }
}

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
using System.Xml;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Imagination.ServiceModels
{
	public class ObjectInstance: LinkableResource
	{
        public Model.ObjectDefinition ObjectDefinition { get; private set; }
        
        private Model.Object _Resource;
        public Model.Object Resource { get { return _Resource; } }

        public ObjectInstance(Model.ObjectDefinition objectDefinition, Model.Object resource)
		{
			ObjectDefinition = objectDefinition;
			_Resource = resource;
		}

        public ObjectInstance(Model.ObjectDefinition objectDefinition, HttpRequest request)
        {
            ObjectDefinition = objectDefinition;

            if (request.ContentType.Contains("xml"))
            {
                using (XmlReader reader = XmlReader.Create(request.Body))
                {
                    Deserialise(reader);
                }
            }
            else
            {
                using (JsonReader reader = new JsonReader(request.Body))
                {
                    Deserialise(reader);
                }
            }
        }

        public ObjectInstance(Model.ObjectDefinition objectDefinition, XmlReader reader)
        {
            ObjectDefinition = objectDefinition;
            Deserialise(reader);
        }

        public ObjectInstance(Model.ObjectDefinition objectDefinition, JsonReader reader)
        {
            ObjectDefinition = objectDefinition;
            Deserialise(reader);
        }

        public ObjectInstanceAction GetAction()
        {
            return new ObjectInstanceAction(this);
        }

        private void Deserialise(JsonReader reader)
        {
            Model.PropertyDefinition propertyDefinition = null;
            Model.Property property = null;
            bool isID = false;
            while (reader.Read())
            {
                switch (reader.State)
                {
                    case TJsonReaderState.NotSet:
                        break;
                    case TJsonReaderState.Array:
                        break;
                    case TJsonReaderState.BOF:
                        break;
                    case TJsonReaderState.Boolean:
                        if (propertyDefinition != null)
                        {
                            Model.PropertyValue propertyValue = new Model.PropertyValue(reader.AsBoolean.ToString());
                            if (propertyDefinition.IsCollection)
                            {
                                if (property.Values == null)
                                    property.Values = new List<Model.PropertyValue>();
                                property.Values.Add(propertyValue);
                            }
                            else
                            {
                                property.Value = new Model.PropertyValue(propertyValue.Value);
                            }
                        }
                        break;
                    case TJsonReaderState.EndArray:
                        propertyDefinition = null;
                        break;
                    case TJsonReaderState.EndObject:
                        break;
                    case TJsonReaderState.EOF:
                        break;
                    case TJsonReaderState.Member:
                        if (reader.Text.Equals("Links"))
                        {
                            Links = new List<Link>();
                            Links.Deserialise(reader);
                        }
                        else
                        {
                            propertyDefinition = ObjectDefinition.GetPropertyBySerialisationName(reader.Text);
                            if (propertyDefinition == null)
                            {
                                isID = string.Compare(reader.Text, "InstanceID", true) == 0;
                            }
                            else
                            {
                                isID = false;
                                property = new Model.Property();
                                property.PropertyDefinitionID = propertyDefinition.PropertyDefinitionID;
                                property.PropertyID = propertyDefinition.PropertyID;
                                if (_Resource != null)
                                    _Resource.Properties.Add(property);
                            }
                        }
                        break;
                    case TJsonReaderState.Null:
                        break;
                    case TJsonReaderState.Number:
                        if (propertyDefinition != null)
                        {
                            Model.PropertyValue propertyValue = new Model.PropertyValue(reader.Text);
                            if (propertyDefinition.IsCollection)
                            {
                                if (property.Values == null)
                                    property.Values = new List<Model.PropertyValue>();
                                property.Values.Add(propertyValue);
                            }
                            else
                            {
                                property.Value = new Model.PropertyValue(propertyValue.Value);
                            }
                        }
                        break;
                    case TJsonReaderState.Object:
                        if (_Resource == null)
                        {
                            _Resource = new Model.Object();
                            _Resource.ObjectDefinitionID = ObjectDefinition.ObjectDefinitionID;
                            _Resource.ObjectID = ObjectDefinition.ObjectID;
                        }
                        break;
                    case TJsonReaderState.String:
                        if (propertyDefinition == null)
                        {
                            if (isID)
                                _Resource.InstanceID = reader.Text;
                        }
                        else
                        {
                            string text;
                            if (propertyDefinition.DataType == Model.TPropertyDataType.DateTime)
                            {
                                text = GetDateTimeString(reader.Text);
                            }
                            else
                            {
                                text = reader.Text;
                            }
                            Model.PropertyValue propertyValue = new Model.PropertyValue(text);
                            if (propertyDefinition.IsCollection)
                            {
                                if (property.Values == null)
                                    property.Values = new List<Model.PropertyValue>();
                                property.Values.Add(propertyValue);
                            }
                            else
                            {
                                property.Value = new Model.PropertyValue(propertyValue.Value);
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void Deserialise(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (_Resource == null)
                    {
                        if (string.Compare(ObjectDefinition.SerialisationName, reader.Name, true) == 0)
                        {
                            _Resource = new Model.Object();
                            _Resource.ObjectDefinitionID = ObjectDefinition.ObjectDefinitionID;
                            _Resource.ObjectID = ObjectDefinition.ObjectID;
                        }
                    }
                    else
                    {
                        if (reader.Name.Equals("Links"))
                        {
                            Links = new List<Link>();
                            Links.Deserialise(reader);
                        }
                        else
                        {
                            Model.PropertyDefinition propertyDefinition = ObjectDefinition.GetPropertyBySerialisationName(reader.Name);
                            if (propertyDefinition == null)
                            {
                                if (string.Compare(reader.Name, "InstanceID", true) == 0)
                                {
                                    reader.Read();
                                    _Resource.InstanceID = reader.Value;
                                }
                            }
                            else
                            {
                                Model.Property property = new Model.Property();
                                property.PropertyDefinitionID = propertyDefinition.PropertyDefinitionID;
                                property.PropertyID = propertyDefinition.PropertyID;
                                if (propertyDefinition.IsCollection)
                                {
                                    XmlReader collectionReader = reader.ReadSubtree();
                                    DeserialiseItems(propertyDefinition, property, collectionReader);
                                }
                                else
                                {
                                    property.Value = new Model.PropertyValue();
                                    property.Value.Value = GetValue(propertyDefinition.DataType, reader.ReadInnerXml());
                                }
                                _Resource.Properties.Add(property);
                            }
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && string.Compare(ObjectDefinition.SerialisationName, reader.Name, true) == 0)
                {
                    break;
                }
            }
        }

        private string GetDateTimeString(string text)
        {
            DateTime datetimeValue;
            string format;
            bool hasTimeZone = (text.IndexOf("+") > 0) || (text.IndexOf("-", 10) > 0);
            bool hasMilliSeconds = text.IndexOf(".") > 0;
            if (!hasTimeZone && !text.EndsWith("Z"))
            {
                text = string.Concat(text, "Z");
            }
            if (hasMilliSeconds)
                format = "yyyy-MM-ddTHH:mm:ss.FFFFFFK";
            else
                format = "yyyy-MM-ddTHH:mm:ssK";
            if (DateTime.TryParseExact(text, format, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out datetimeValue))
            {
                text = datetimeValue.ToString(Model.XmlHelper.XMLDATEFORMAT);
            }
            else
            {
                if (DateTime.TryParse(text, null, System.Globalization.DateTimeStyles.AssumeUniversal, out datetimeValue))
                {
                    datetimeValue = datetimeValue.ToUniversalTime();
                    text = datetimeValue.ToString(Model.XmlHelper.XMLDATEFORMAT);
                }
                else
                    throw new BadRequestException();
            }
            return text;
        }

        private string GetValue(Model.TPropertyDataType dataType, string text)
        {
            string result = text;
            switch (dataType)
            {
                case Model.TPropertyDataType.NotSet:
                    break;
                case Model.TPropertyDataType.String:
                case Model.TPropertyDataType.Opaque:
                    break;
                case Model.TPropertyDataType.Boolean:
                    bool boolValue;
                    if (bool.TryParse(text, out boolValue))
                        result = text;
                    else
                        throw new BadRequestException();
                    break;
                case Model.TPropertyDataType.Integer:
                    long longValue;
                    if (long.TryParse(text, out longValue))
                        result = text;
                    else
                        throw new BadRequestException();
                    break;
                case Model.TPropertyDataType.Float:
                    double doubleValue;
                    if (double.TryParse(text, out doubleValue))
                        result = text;
                    else
                        throw new BadRequestException();
                    break;
                case Model.TPropertyDataType.DateTime:
                    result = GetDateTimeString(text);
                    break;                
                case Model.TPropertyDataType.Object:
                    break;
                default:
                    break;
            }
            return result;
        }

        private void DeserialiseItems(Model.PropertyDefinition propertyDefinition, Model.Property property, XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (string.Compare(propertyDefinition.CollectionItemSerialisationName, reader.Name, true) == 0)
                    {
                        Model.PropertyValue propertyValue = new Model.PropertyValue();
                        propertyValue.Value = reader.ReadInnerXml();
                        if (property.Values == null)
                            property.Values = new List<Model.PropertyValue>();
                        property.Values.Add(propertyValue);
                    }
                }
            }
        }

        public void Serialise(JsonWriter writer)
        {
            writer.WriteObject();
            if (Links != null)
            {
                Links.Serialise(writer);
            }
            if (!string.IsNullOrEmpty(_Resource.InstanceID))
            {
                writer.WriteMember("InstanceID");
                writer.WriteValue(_Resource.InstanceID);
            }
            foreach (Model.PropertyDefinition propertyMetadata in ObjectDefinition.Properties)
            {
                Model.Property property = _Resource.GetProperty(propertyMetadata.PropertyID);
                if (property != null)
                {
                    if (propertyMetadata.IsCollection)
                    {
                        if ((property.Values != null) && (property.Values.Count > 0))
                        {
                            writer.WriteMember(propertyMetadata.SerialisationName);
                            writer.WriteArray();
                            foreach (Model.PropertyValue item in property.Values)
                            {
                                Serialise(writer, propertyMetadata.DataType, item);
                            }
                            writer.WriteEndArray();
                        }
                    }
                    else
                    {
                        writer.WriteMember(propertyMetadata.SerialisationName);
                        Serialise(writer, propertyMetadata.DataType, property.Value);
                    }
                }
            }
            writer.WriteEndObject();
        }

        public void Serialise(JsonWriter writer, Model.TPropertyDataType dataType, Model.PropertyValue item)
        {
            switch (dataType)
            {
                case Model.TPropertyDataType.NotSet:
                    break;
                case Model.TPropertyDataType.String:
                    writer.WriteValue(item.Value);
                    break;
                case Model.TPropertyDataType.Boolean:
                    writer.WriteValue(item.ValueAsBoolean());
                    break;
                case Model.TPropertyDataType.Integer:
                    writer.WriteValue(item.ValueAsInt64());
                    break;
                case Model.TPropertyDataType.Float:
                    writer.WriteValue(item.ValueAsDouble());
                    break;
                case Model.TPropertyDataType.DateTime:
                    writer.WriteValue(item.ValueAsDateTime());
                    break;
                case Model.TPropertyDataType.Opaque:
                    writer.WriteValue(item.Value);
                    break;
                case Model.TPropertyDataType.Object:
                    break;
                default:
                    break;
            }
        }

        public void Serialise(XmlWriter writer)
        {
            writer.WriteStartElement(ObjectDefinition.SerialisationName);
            if (Links != null)
            {
                Links.Serialise(writer);
            }
            if (!string.IsNullOrEmpty(_Resource.InstanceID))
            {
                Model.XmlHelper.WriteElement(writer, "InstanceID", _Resource.InstanceID);
            }
            foreach (Model.PropertyDefinition propertyDefinition in ObjectDefinition.Properties)
            {
                Model.Property property = _Resource.GetProperty(propertyDefinition.PropertyID);
                if (property != null)
                {
                    if (propertyDefinition.IsCollection)
                    {
                        if ((property.Values != null) && (property.Values.Count > 0))
                        {
                            string itemName = propertyDefinition.CollectionItemSerialisationName;
                            if (string.IsNullOrEmpty(itemName))
                            {
                                itemName = StringUtils.ToSingular(propertyDefinition.SerialisationName);
                            }
                            writer.WriteStartElement(propertyDefinition.SerialisationName);
                            foreach (Model.PropertyValue item in property.Values)
                            {
                                Model.XmlHelper.WriteElement(writer, itemName, item.Value);
                            }
                            writer.WriteEndElement();
                        }
                    }
                    else
                    {
                        Model.XmlHelper.WriteElement(writer, propertyDefinition.SerialisationName, property.Value.Value);
                    }
                }
            }
            writer.WriteEndElement();
        }

        public void ToJson(Stream stream)
        {
            JsonWriter writer = new JsonWriter(stream);
            Serialise(writer);
            writer.Flush();
            stream.Position = 0;
        }

        public void ToXml(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.CheckCharacters = false;

            XmlWriter writer = XmlWriter.Create(stream, settings);
            Serialise(writer);
            writer.Flush();
            stream.Position = 0;
        }
    }

    public class ObjectInstanceAction : ActionResult
    {
        ObjectInstance _Instance;

        public ObjectInstanceAction(ObjectInstance instance)
        {
            _Instance = instance;
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;

            string acceptType = request.GetContentFormat();

            MemoryStream stream = new MemoryStream(16384);
            string format = "+json";
            if (acceptType != null && acceptType.Contains("xml"))
            {
                format = "+xml";
                _Instance.ToXml(stream);
            }
            else
            {
                _Instance.ToJson(stream);
            }
            response.ContentType = _Instance.ObjectDefinition.MIMEType + format;
            return stream.CopyToAsync(response.Body);
        }
    }
}

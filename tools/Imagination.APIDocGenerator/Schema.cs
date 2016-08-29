using Imagination.ServiceModels;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Imagination.Tools.APIDocGenerator
{
    public class Schema
    {
        private const string XML_NAMESPACE = "http://www.w3.org/2001/XMLSchema";

        public Dictionary<TDataExchangeFormat, string> Content { get; private set; }
        public Type Object { get; private set; }

        public Schema(Type objectType)
        {
            Object = objectType;
            Content = new Dictionary<TDataExchangeFormat, string>();
            Content.Add(TDataExchangeFormat.Json, GenerateJsonSchema());
            Content.Add(TDataExchangeFormat.Xml, GenerateXmlSchema());
        }

        private string GenerateJsonSchema()
        {
            List<string> required = new List<string>();
            MemoryStream stream = new MemoryStream();
            JsonWriter writer = new JsonWriter(stream);
            writer.WriteObject();

            writer.WriteMember("$schema");
            writer.WriteValue("http://json-schema.org/draft-04/schema");

            writer.WriteMember("title");
            writer.WriteValue(Object.Name);

            writer.WriteMember("type");
            writer.WriteValue("object");

            writer.WriteMember("properties");

            AddJsonProperties(writer, Object.GetProperties(), false);

            if (required.Count > 0)
            {
                writer.WriteMember("required");
                writer.WriteArray();
                foreach (string requirement in required)
                {
                    writer.WriteValue(requirement);
                }
                writer.WriteEndArray();
            }

            writer.WriteEndObject();

            writer.Flush();
            stream.Position = 0;

            string unformattedJsonBody = new StreamReader(stream).ReadToEnd();
            object parsedJson = JsonConvert.DeserializeObject(unformattedJsonBody);
            return JsonConvert.SerializeObject(parsedJson, Newtonsoft.Json.Formatting.Indented);
        }

        private void AddJsonProperties(JsonWriter writer, PropertyInfo[] properties, bool inArray)
        {
            writer.WriteObject();
            foreach (PropertyInfo property in properties)
            {
                if (IsValidProperty(property))
                {
                    string jsonPropertyType = GetPropertyTypeName(property.PropertyType, TDataExchangeFormat.Json);

                    if (!inArray)
                    {
                        writer.WriteMember(property.Name);
                        writer.WriteObject();
                    }

                    writer.WriteMember("type");
                    writer.WriteValue(jsonPropertyType);

                    if (jsonPropertyType.Equals("object") || jsonPropertyType.Equals("array"))
                    {
                        bool inArray2 = false;
                        if (jsonPropertyType.Equals("object"))
                        {
                            writer.WriteMember("properties");
                            inArray2 = false;
                        }
                        else
                        {
                            writer.WriteMember("items");
                            inArray2 = true;
                        }

                        AddJsonProperties(writer, property.PropertyType.GetProperties(), inArray2);

                    }
                    if (!inArray)
                        writer.WriteEndObject();
                }
            }
            writer.WriteEndObject();
        }

        private string GenerateXmlSchema()
        {
            XmlSchema xmlSchema = new XmlSchema();
            //xmlSchema.TargetNamespace = "http://www.w3.org/2001/XMLSchema";
            //xmlSchema.AttributeFormDefault = XmlSchemaForm.Unqualified;
            //xmlSchema.ElementFormDefault = XmlSchemaForm.Qualified;

            XmlSchemaElement objectElement = new XmlSchemaElement();
            xmlSchema.Items.Add(objectElement);
            objectElement.Name = Object.Name;

            AddXMLProperties(null, xmlSchema.Items, null, Object.GetProperties());

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add(xmlSchema);
            schemaSet.Compile();

            MemoryStream stream = new MemoryStream();
            foreach (XmlSchema compiledSchema in schemaSet.Schemas())
            {
                compiledSchema.Write(stream);
            }

            stream.Position = 0;
            StreamReader streamReader = new StreamReader(stream);
            streamReader.ReadLine();  // skip <?xml version="1.0"?>
            return streamReader.ReadToEnd();
        }


        private void AddXMLProperties(XmlSchemaElement parent, XmlSchemaObjectCollection items, XmlSchemaObjectCollection attributes, PropertyInfo[] properties)
        {
            foreach (PropertyInfo property in properties)
            {
                if (IsValidProperty(property))
                {
                    string propertyName = property.Name;
                    if (propertyName.Equals("Item") && property.DeclaringType.Name.StartsWith("List`"))
                    {
                        if (!parent.Name.Equals("Items"))
                            propertyName = StringUtils.ToSingular(parent.Name);
                        else
                            propertyName = property.PropertyType.Name;
                    }

                    string xmlPropertyType = GetPropertyTypeName(property.PropertyType, TDataExchangeFormat.Xml);

                    if (property.GetCustomAttributes<XmlAttributeAttribute>().FirstOrDefault() != null)
                    {
                        XmlSchemaAttribute attribute = new XmlSchemaAttribute();
                        attribute.Name = propertyName;
                        attribute.SchemaTypeName = new XmlQualifiedName(xmlPropertyType, XML_NAMESPACE);
                        if (attribute.Name.Equals("type"))
                            attribute.Use = XmlSchemaUse.Optional;
                        attributes.Add(attribute);
                    }
                    else
                    {
                        XmlSchemaElement propertyElement = new XmlSchemaElement();
                        propertyElement.Name = propertyName;

                        if (xmlPropertyType.Equals("array") || xmlPropertyType.Equals("object"))
                        {
                            XmlSchemaComplexType complexType = new XmlSchemaComplexType();
                            propertyElement.SchemaType = complexType;

                            XmlSchemaGroupBase sequence = null;
                            if (xmlPropertyType.Equals("array"))
                            {
                                sequence = new XmlSchemaSequence();
                                sequence.MinOccursString = "0";
                                sequence.MaxOccursString = "unbounded";

                                if (parent != null)
                                {
                                    // nested empty collections shouldn't have to exist.
                                    propertyElement.UnhandledAttributes = new XmlAttribute[1];
                                    propertyElement.UnhandledAttributes[0] = new XmlDocument().CreateAttribute("minOccurs");
                                    propertyElement.UnhandledAttributes[0].Value = "0";
                                }
                            }
                            else
                            {
                                sequence = new XmlSchemaAll();
                            }

                            AddXMLProperties(propertyElement, sequence.Items, complexType.Attributes, property.PropertyType.GetProperties());

                            if (sequence.Items.Count > 0)
                            {
                                complexType.Particle = sequence;
                            }
                        }
                        else
                        {
                            propertyElement.SchemaTypeName = new XmlQualifiedName(xmlPropertyType, XML_NAMESPACE);
                        }
                        items.Add(propertyElement);
                    }
                }
            }
        }

        private bool IsValidProperty(PropertyInfo property)
        {
            return property.CanWrite && (property.Name.Equals("Item") || !property.DeclaringType.Name.StartsWith("List`"));
        }

        private string GetPropertyTypeName(Type propertyType, TDataExchangeFormat format)
        {
            // JSON: string integer number boolean object array
            // XML: string integer decimal boolean date time

            string propertyTypeName = propertyType.Name;
            if (propertyTypeName.StartsWith("Nullable`"))
            {
                propertyTypeName = ((TypeInfo)propertyType).DeclaredFields.ToArray()[1].FieldType.Name;
            }

            if (propertyTypeName.Equals("String"))
            {
                propertyTypeName = "string";
            }
            else if (propertyTypeName.Equals("Int32") || propertyTypeName.Equals("Int64"))
            {
                propertyTypeName = "integer";
            }
            else if (propertyTypeName.Equals("Float") || propertyTypeName.Equals("Double"))
            {
                if (format == TDataExchangeFormat.Json)
                    propertyTypeName = "number";
                else
                    propertyTypeName = "decimal";
            }
            else if (propertyTypeName.Equals("Boolean"))
            {
                propertyTypeName = "boolean";
            }
            else if (propertyTypeName.StartsWith("List`"))
            {
                propertyTypeName = "array";
            }
            else
            {
                //Console.WriteLine("GetPropertyType: skipped " + propertyTypeName);
                propertyTypeName = "object";
            }
            return propertyTypeName;
        }
    }
}

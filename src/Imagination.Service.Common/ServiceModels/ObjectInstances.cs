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
using System.Threading.Tasks;
using System.IO;

namespace Imagination.ServiceModels
{
    public class ObjectInstances : CollectionBase<ObjectInstance>
    {
        internal Model.ObjectDefinition _ObjectDefinition;

        public ObjectInstances(Model.ObjectDefinition objectDefinition)
        {
            _ObjectDefinition = objectDefinition;
        }

        public ObjectInstances(Model.ObjectDefinition objectDefinition, Stream content, string contentType) : this(objectDefinition)
        {
            if (contentType != null && contentType.Contains("xml"))
            {
                using (XmlReader reader = XmlReader.Create(content))
                {
                    Deserialise(reader);
                }
            }
            else
            {
                using (JsonReader reader = new JsonReader(content))
                {
                    Deserialise(reader);
                }
            }
        }

        private void Deserialise(JsonReader reader)
        {
            if (reader.Read() && reader.State == TJsonReaderState.Object)
            {
                reader.Read();
                    
                if (reader.Text.Equals("Links"))
                {
                    Links = new List<Link>();
                    Links.Deserialise(reader);
                    reader.Read();
                }
                if (reader.Text.Equals("PageInfo"))
                {
                    PageInfo = new PageInfo();
                    PageInfo.Deserialise(reader);
                    reader.Read();
                }

                Items = new List<ObjectInstance>();
                if (reader.Text.Equals("Items"))
                {
                    while (reader.State != TJsonReaderState.EOF)
                    {
                        Items.Add(new ObjectInstance(_ObjectDefinition, reader));
                    }
                }
            }
        }

        private void Deserialise(XmlReader reader)
        {
            if (reader.Read() && reader.NodeType == XmlNodeType.Element)
            {
                if (string.Compare(StringUtils.ToPlural(_ObjectDefinition.SerialisationName), reader.Name, true) == 0)
                {
                    reader.Read();
                    if (reader.Name.Equals("Links"))
                    {
                        Links = new List<Link>();
                        Links.Deserialise(reader);
                        reader.Read();
                    }
                    if (reader.Name.Equals("PageInfo"))
                    {
                        PageInfo = new PageInfo();
                        PageInfo.Deserialise(reader);
                        reader.Read();
                    }

                    Items = new List<ObjectInstance>();
                    if (reader.Name.Equals("Items"))
                    {
                        while (!reader.EOF)
                        {
                            Items.Add(new ObjectInstance(_ObjectDefinition, reader));
                        }
                    }
                }
            }
        }

        public ObjectInstancesAction GetAction()
        {
            return new ObjectInstancesAction(this);
        }
    }

    public class ObjectInstancesAction : ActionResult
    {
        ObjectInstances _Instances;

        public ObjectInstancesAction(ObjectInstances instances)
        {
            _Instances = instances;
        }

        private void ToJson(Stream stream)
        {
            JsonWriter writer = new JsonWriter(stream);
            writer.WriteObject();
            _Instances.Links.Serialise(writer);
            _Instances.PageInfo.Serialise(writer);

            writer.WriteMember("Items");
            writer.WriteArray();
            if (_Instances.Items != null)
            {
                foreach (ObjectInstance instance in _Instances.Items)
                {
                    instance.Serialise(writer);
                }
            }
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.Flush();

            stream.Position = 0;
        }

        private void ToXml(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.CheckCharacters = false;
            
            XmlWriter writer = XmlWriter.Create(stream, settings);
            writer.WriteStartElement(StringUtils.ToPlural(_Instances._ObjectDefinition.SerialisationName));
            _Instances.Links.Serialise(writer);
            _Instances.PageInfo.Serialise(writer);

            writer.WriteStartElement("Items");
            if (_Instances.Items != null)
            {
                foreach (ObjectInstance instance in _Instances.Items)
                {
                    instance.Serialise(writer);
                }
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.Flush();

            stream.Position = 0;
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
                ToXml(stream);
            }
            else
            {
                ToJson(stream);
            }
            response.ContentType = StringUtils.ToPlural(_Instances._ObjectDefinition.MIMEType) + format;
            return stream.CopyToAsync(response.Body);
        }

    }

}

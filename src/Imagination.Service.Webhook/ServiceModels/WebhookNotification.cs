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
using Imagination.Model;
using System.IO;
using Imagination.DataAccess;
using Microsoft.Extensions.Logging;

namespace Imagination.ServiceModels
{
    [ContentType("application/vnd.imgtec.notification")]
    public class WebhookNotification: LinkableResource
    {
        private Model.WebhookNotification _WebhookNotification;
        
        public WebhookNotification(Model.WebhookNotification webhookNotification)
		{
            _WebhookNotification = webhookNotification;
		}

        public WebhookNotification(Model.WebhookNotification webhookNotification, ServiceEventMessage message)
        {
            _WebhookNotification = webhookNotification;
            _WebhookNotification.AcceptContentType = (string)message.Parameters["AcceptContentType"]; 
            _WebhookNotification.SubscriptionID = (string)message.Parameters["SubscriptionID"];
            _WebhookNotification.ClientID = (string)message.Parameters["ClientID"];
            _WebhookNotification.SubscriptionType = (string)message.Parameters["SubscriptionType"];
            _WebhookNotification.TimeTriggered = (DateTime)message.Parameters["TimeTriggered"];
            if (message.Parameters.ContainsKey("Object"))
            {
                _WebhookNotification.ChangedObject = (Model.Object)message.Parameters["Object"];

                _WebhookNotification.ObjectDefinition = DataAccessFactory.ObjectDefinitions.GetLookups().GetObjectDefinition(_WebhookNotification.ChangedObject.ObjectDefinitionID);
                if (_WebhookNotification.ObjectDefinition == null)
                {
                    ApplicationEventLog.Write(LogLevel.Warning, $"Could not lookup object definition {_WebhookNotification.ChangedObject.ObjectDefinitionID}");
                }
            }
        }

        public void Serialise(Stream stream)
        {
            Serialise(stream, _WebhookNotification.AcceptContentType);
        }

        public void Serialise(Stream stream, string acceptContentType)
        {
            if (acceptContentType != null && acceptContentType.Contains("xml"))
            {
                ToXml(stream);
            }
            else
            {
                ToJson(stream);
            }
        }

        private List<Link> GetLinks()
        {
            List<Link> links = new List<Link>();
            // FIXME: Do not hardcode types - take from service models

            links.Add(new Link { rel = "subscription", href = string.Concat("/subscriptions/", _WebhookNotification.SubscriptionID), type = "application/vnd.imgtec.subscription" });
            links.Add(new Link { rel = "client", href = string.Concat("/clients/", _WebhookNotification.ClientID), type = "application/vnd.imgtec.client" });

            if (_WebhookNotification.ChangedObject != null && _WebhookNotification.ObjectDefinition != null)
            {
                links.Add(new Link { rel = "object", href = string.Concat("/clients/", _WebhookNotification.ClientID, "/objecttypes/", _WebhookNotification.ObjectDefinition.ObjectDefinitionID, "/instances/", _WebhookNotification.ChangedObject.InstanceID), type = "application/vnd.imgtec.object" });
                links.Add(new Link { rel = "definition", href = string.Concat("/objecttypes/definitions/", _WebhookNotification.ObjectDefinition.ObjectDefinitionID), type = "application/vnd.imgtec.objectdefinition" });
            }
            return links;
        }

        private void ToJson(Stream stream)
        {
            using (JsonWriter jsonWriter = new JsonWriter(stream))
            {
                jsonWriter.WriteObject();

                jsonWriter.WriteMember("Items");
                jsonWriter.WriteArray();

                jsonWriter.WriteObject();
                LinkExtensions.Serialise(GetLinks(), jsonWriter);

                jsonWriter.WriteMember("TimeTriggered");
                jsonWriter.WriteValue(_WebhookNotification.TimeTriggered);

                jsonWriter.WriteMember("SubscriptionType");
                jsonWriter.WriteValue(_WebhookNotification.SubscriptionType);

                if (_WebhookNotification.ObjectDefinition != null)
                {
                    jsonWriter.WriteMember("Value");
                    ObjectInstance instance = new ObjectInstance(_WebhookNotification.ObjectDefinition, _WebhookNotification.ChangedObject);
                    instance.Links = new List<Link>();
                    instance.Serialise(jsonWriter);
                }
                
                jsonWriter.WriteEndObject();

                jsonWriter.WriteEndArray();
                jsonWriter.WriteEndObject();
                jsonWriter.Flush();
            }
            stream.Position = 0;
        }

        private void ToXml(Stream stream)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.CheckCharacters = false;

            using (XmlWriter xmlWriter = XmlWriter.Create(stream, settings))
            {
                xmlWriter.WriteStartElement("Notifications");
                xmlWriter.WriteStartElement("Items");
                xmlWriter.WriteStartElement("Notification");

                LinkExtensions.Serialise(GetLinks(), xmlWriter);

                Model.XmlHelper.WriteElement(xmlWriter, "TimeTriggered", _WebhookNotification.TimeTriggered);
                Model.XmlHelper.WriteElement(xmlWriter, "SubscriptionType", _WebhookNotification.SubscriptionType);

                if (_WebhookNotification.ObjectDefinition != null)
                {
                    xmlWriter.WriteStartElement("Value");
                    ObjectInstance instance = new ObjectInstance(_WebhookNotification.ObjectDefinition, _WebhookNotification.ChangedObject);
                    instance.Links = new List<Link>();
                    instance.Serialise(xmlWriter);
                    xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();  // Notification
                xmlWriter.WriteEndElement();  // Items
                xmlWriter.WriteEndElement();  // Notifications
                xmlWriter.Flush();
            }
            stream.Position = 0;
        }
    }

}

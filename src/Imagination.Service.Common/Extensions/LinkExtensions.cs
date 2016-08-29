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

using Imagination.ServiceModels;
using System;
using System.Collections.Generic;
using System.Xml;

namespace Imagination
{
    public static class LinkExtensions
    {
        public static void Serialise(this List<Link> links, JsonWriter writer)
        {
            if (links.Count > 0)
            {
                writer.WriteMember("Links");
                writer.WriteArray();

                foreach (Link link in links)
                {
                    link.Serialise(writer);
                }

                writer.WriteEndArray();
            }
        }

        public static void Serialise(this Link link, JsonWriter writer)
        {
            writer.WriteObject();
            writer.WriteMember("rel");
            writer.WriteValue(link.rel);
            writer.WriteMember("href");
            writer.WriteValue(link.href);
            writer.WriteEndObject();
        }

        public static void Serialise(this List<Link> links, XmlWriter writer)
        {
            writer.WriteStartElement("Links");
            foreach (Link link in links)
            {
                link.Serialise(writer);
            }
            writer.WriteEndElement();
        }

        public static void Serialise(this Link link, XmlWriter writer)
        {
            writer.WriteStartElement("Link");
            writer.WriteAttributeString("rel", link.rel);
            writer.WriteAttributeString("href", link.href);
            writer.WriteEndElement();
        }

        public static void Deserialise(this List<Link> links, XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("Links"))
            {
                while (reader.Read() && reader.NodeType == XmlNodeType.Element && reader.Name.Equals("Link"))
                {
                    Link link = new Link();
                    link.rel = reader.GetAttribute("rel");
                    link.href = reader.GetAttribute("href");
                    links.Add(link);
                }
            }
        }

        public static void Deserialise(this List<Link> links, JsonReader reader)
        {
            if (reader.Read() && reader.State == TJsonReaderState.Array)
            {
                Link link = null;
                while (reader.Read() && reader.State != TJsonReaderState.EndArray)
                {
                    switch (reader.State)
                    {
                        case TJsonReaderState.Object:
                            link = new Link();
                            links.Add(link);
                            break;
                        case TJsonReaderState.Member:
                            string attribute = reader.Text;
                            reader.Read();
                            if (string.Compare(attribute, "rel", true) == 0)
                                link.rel = reader.Text;
                            else if (string.Compare(attribute, "href", true) == 0)
                                link.href = reader.Text;
                            else
                                throw new NotSupportedException("Unsupported attribute: " + attribute);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
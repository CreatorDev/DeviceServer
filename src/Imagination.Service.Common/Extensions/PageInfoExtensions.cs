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
    public static class PageInfoExtensions
    {
        public static void Serialise(this PageInfo pageInfo, JsonWriter writer)
        {
            writer.WriteMember("PageInfo");
            writer.WriteObject();
            writer.WriteMember("TotalCount");
            writer.WriteValue(pageInfo.TotalCount.ToString());
            writer.WriteMember("ItemsCount");
            writer.WriteValue(pageInfo.ItemsCount.ToString());
            writer.WriteMember("StartIndex");
            writer.WriteValue(pageInfo.StartIndex.ToString());
            writer.WriteEndObject();
        }

        public static void Serialise(this PageInfo pageInfo, XmlWriter writer)
        {
            writer.WriteStartElement("PageInfo");
            writer.WriteElementString("TotalCount", pageInfo.TotalCount.ToString());
            writer.WriteElementString("ItemsCount", pageInfo.ItemsCount.ToString());
            writer.WriteElementString("StartIndex", pageInfo.StartIndex.ToString());
            writer.WriteEndElement();
        }

        public static void Deserialise(this PageInfo pageInfo, XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("PageInfo"))
            {
                while (reader.Read() && reader.NodeType == XmlNodeType.Element)
                {
                    string elementName = reader.Name;
                    if (reader.Read() && reader.NodeType == XmlNodeType.Text)
                    {
                        int value;
                        if (int.TryParse(reader.Value, out value))
                        {
                            switch (elementName)
                            {
                                case "TotalCount":
                                    pageInfo.TotalCount = value;
                                    break;
                                case "ItemsCount":
                                    pageInfo.ItemsCount = value;
                                    break;
                                case "StartIndex":
                                    pageInfo.StartIndex = value;
                                    break;
                            }
                        }
                    }
                    reader.Read();  // end element
                }
            }
        }

        public static void Deserialise(this PageInfo pageInfo, JsonReader reader)
        {
            if (reader.Read() && reader.State == TJsonReaderState.Object)
            {
                //Link link = null;
                while (reader.Read() && reader.State != TJsonReaderState.EndObject)
                {
                    switch (reader.State)
                    {
                        case TJsonReaderState.Member:
                            string elementName = reader.Text;
                            reader.Read();
                            int value;
                            if (int.TryParse(reader.Text, out value))
                            {
                                switch (elementName)
                                {
                                    case "TotalCount":
                                        pageInfo.TotalCount = value;
                                        break;
                                    case "ItemsCount":
                                        pageInfo.ItemsCount = value;
                                        break;
                                    case "StartIndex":
                                        pageInfo.StartIndex = value;
                                        break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
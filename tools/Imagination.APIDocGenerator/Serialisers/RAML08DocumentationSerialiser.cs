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

using Imagination.Documentation;
using Imagination.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace Imagination.Tools.APIDocGenerator.Serialisers
{
    public class RAML08DocumentationSerialiser : IDocumentationSerialiser
    {
        private const int TAB_SIZE = 2;
        private const int RESOURCE_INDENT = 0;

        public string GetDescription()
        {
            return "RAML 0.8";
        }

        public void Serialise(StreamWriter writer, ResourceNode tree, DocumentationHeaderSettings headerSettings, SchemaStore schemaStore, ExampleStore exampleStore)
        {
            WriteHeader(writer, headerSettings, schemaStore);
            WriteResources(writer, tree, headerSettings, schemaStore, exampleStore, -1);
        }

        private void WriteHeader(StreamWriter writer, DocumentationHeaderSettings headerSettings, SchemaStore schemaStore)
        {
            writer.WriteLine("#%RAML 0.8");
            writer.WriteLine("---");
            writer.WriteLine(string.Concat("title: ", headerSettings.Title));
            writer.WriteLine(string.Concat("version: ", headerSettings.Version));
            writer.WriteLine(string.Concat("baseUri: ", headerSettings.BaseURI));
            writer.WriteLine(string.Concat("mediaType: ", headerSettings.MediaType));
            writer.WriteLine("securedBy: [oauth_2_0]");
            WriteSecuritySchemes(writer, headerSettings);
            WriteIntroductionDocumentation(writer, headerSettings);
            WriteSchemas(writer, schemaStore);
            writer.WriteLine();
        }

        private void WriteSchemas(StreamWriter writer, SchemaStore schemaStore)
        {
            writer.WriteLine("schemas: ");

            foreach (Schema schema in schemaStore.Schemas.Values)
            {
                foreach (KeyValuePair<TDataExchangeFormat, string> pair in schema.Content)
                {
                    writer.WriteLine(string.Concat(GetIndentString(1), "- ", GetSchemaKey(schema.Object, pair.Key), ": |"));
                    List<string> lines = SerialisationUtils.SplitLines(pair.Value);
                    foreach (string line in lines)
                    {
                        writer.WriteLine(string.Concat(GetIndentString(3), line));
                    }
                }
            }
        }

        private static void WriteIntroductionDocumentation(StreamWriter writer, DocumentationHeaderSettings headerSettings)
        {
            writer.WriteLine("documentation: ");
            foreach (KeyValuePair<string, string> pair in headerSettings.Introduction)
            {
                writer.WriteLine(string.Concat(GetIndentString(1), "- title: ", pair.Key));
                writer.WriteLine(string.Concat(GetIndentString(2), "content: ", pair.Value));
            }
        }

        private void WriteSecuritySchemes(StreamWriter writer, DocumentationHeaderSettings headerSettings)
        {
            writer.WriteLine("securitySchemes:");
            writer.WriteLine(string.Concat(GetIndentString(1), "- oauth_2_0:"));
            writer.WriteLine(string.Concat(GetIndentString(3), "description: ", headerSettings.OAuth20Description));
            writer.WriteLine(string.Concat(GetIndentString(3), "type: OAuth 2.0"));
            /*writer.WriteLine(string.Concat(GetIndentString(3), "describedBy:"));
            writer.WriteLine(string.Concat(GetIndentString(4), "responses:"));
            writer.WriteLine(string.Concat(GetIndentString(5), "401:"));
            WriteDefaultDescription(writer, headerSettings, HttpStatusCode.Unauthorized, 6);
            writer.WriteLine(string.Concat(GetIndentString(5), "403:"));
            WriteDefaultDescription(writer, headerSettings, HttpStatusCode.Forbidden, 6);*/
            writer.WriteLine(string.Concat(GetIndentString(3), "settings:"));
            writer.WriteLine(string.Concat(GetIndentString(4), "accessTokenUri: ", headerSettings.BaseURI, headerSettings.AuthorisationEndpoint));
            writer.WriteLine(string.Concat(GetIndentString(4), "authorizationUri: ", headerSettings.BaseURI, headerSettings.AuthorisationEndpoint));
            writer.WriteLine(string.Concat(GetIndentString(4), "authorizationGrants: [owner]"));
        }

        private void WriteResources(StreamWriter writer, ResourceNode node, DocumentationHeaderSettings headerSettings, SchemaStore schemaStore, ExampleStore exampleStore, int indent)
        {
            if (node.Parent != null)
            {
                string key = node.Key;
                if (node.Method == null)
                {
                    key = string.Concat("/", key);
                }
                writer.WriteLine(string.Concat(GetIndentString(indent), key, ":"));

                if (node.Class != null && node.HasMethods())
                {
                    // top level resource
                    RouteDocumentationAttribute attribute = node.Class.GetCustomAttributes<RouteDocumentationAttribute>().Where(a => a.Route != null && a.Route.EndsWith(node.GetRoute())).FirstOrDefault();
                    if (attribute != null)
                    {
                        if (attribute.DisplayName != null)
                        {
                            writer.WriteLine(string.Concat(GetIndentString(indent + 1), "displayName: ", attribute.DisplayName));
                        }
                        if (attribute.Summary != null)
                        {
                            writer.WriteLine(string.Concat(GetIndentString(indent + 1), "description: ", attribute.Summary));
                        }
                    }
                    else
                    {
                        SerialisationLog.Warning(string.Concat("No route documentation for route '", node.GetRoute(), "' in ", node.Class.Name));
                    }
                }

                if (node.Method != null)
                {
                    WriteMethod(writer, node, headerSettings, schemaStore, exampleStore, indent);
                }
                else if (node.Class != null)
                {
                    NamedParameterDocumentationAttribute attribute = node.Class.GetCustomAttributes<NamedParameterDocumentationAttribute>()
                        .Where(a => string.Concat("{", a.Name, "}").Equals(node.Key)).FirstOrDefault();
                    if (attribute != null)
                    {
                        writer.WriteLine(string.Concat(GetIndentString(indent + 1), "uriParameters:"));
                        writer.WriteLine(string.Concat(GetIndentString(indent + 2), attribute.Name, ":"));

                        if (attribute.DisplayName != null)
                        {
                            writer.WriteLine(string.Concat(GetIndentString(indent + 3), "displayName: ", attribute.DisplayName));
                        }
                        if (attribute.Type != TNamedParameterType.NotSet)
                        {
                            writer.WriteLine(string.Concat(GetIndentString(indent + 3), "type: ", attribute.Type.ToString().ToLower()));
                        }
                        if (attribute.Description != null)
                        {
                            writer.WriteLine(string.Concat(GetIndentString(indent + 3), "description: ", attribute.Description));
                        }
                    }
                }
            }
            foreach (ResourceNode child in node.Children.Values)
            {
                WriteResources(writer, child, headerSettings, schemaStore, exampleStore, indent + 1);
            }
        }

        private void WriteMethod(StreamWriter writer, ResourceNode node, DocumentationHeaderSettings headerSettings, SchemaStore schemaStore, ExampleStore exampleStore, int indent)
        {
            MethodDocumentationAttribute attribute = node.Method.GetCustomAttributes<MethodDocumentationAttribute>().FirstOrDefault();

            if (node.AllowsAnonymous())
            {
                string securitySchemes = "null";
                if (attribute != null && attribute.AllowMultipleSecuritySchemes)
                {
                    securitySchemes = string.Concat(securitySchemes, ", oauth_2_0");
                }
                writer.WriteLine(string.Concat(GetIndentString(indent + 1), "securedBy: [", securitySchemes, "]"));
            }

            if (attribute != null)
            {
                if (attribute.Summary != null)
                {
                    writer.WriteLine(string.Concat(GetIndentString(indent + 1), "description: |"));
                    writer.WriteLine(string.Concat(GetIndentString(indent + 2), attribute.Summary));
                    Example example = exampleStore.GetExample(node.Class, node.Method);
                    if (example != null)
                    {
                        writer.WriteLine(GetIndentString(indent + 2));
                        writer.WriteLine(string.Concat(GetIndentString(indent + 2), "For more information, go to ", headerSettings.RepositoryFilesURI, "/", example.DocFilename, "#", example.DocHeading));
                    }
                    else if (attribute.RequestTypes != null)
                    {
                        SerialisationLog.Warning(string.Concat("No example to retrieve link to more information for ", node.Class.Name, ".", node.Method.Name));
                    }
                }
                else
                {
                    SerialisationLog.Warning(string.Concat("No summary for ", node.Class.Name, ".", node.Method.Name));
                }

                if (attribute.ResponseTypes != null && attribute.ResponseTypes.Any(r => r.GetProperties().Any(p => p.PropertyType == typeof(PageInfo))))
                {
                    writer.WriteLine(string.Concat(GetIndentString(indent + 1), "queryParameters: "));
                    foreach (KeyValuePair<string, Dictionary<string, string>> field in headerSettings.PagingFields)
                    {
                        writer.WriteLine(string.Concat(GetIndentString(indent + 2), field.Key, ":"));
                        writer.WriteLine(string.Concat(GetIndentString(indent + 3), "displayName: ", field.Value["displayName"]));
                        writer.WriteLine(string.Concat(GetIndentString(indent + 3), "description: ", field.Value["description"]));
                        writer.WriteLine(string.Concat(GetIndentString(indent + 3), "type: ", field.Value["type"]));
                        writer.WriteLine(string.Concat(GetIndentString(indent + 3), "required: ", field.Value["required"]));
                    }
                }

                if (attribute.RequestTypes != null)
                {
                    writer.WriteLine(string.Concat(GetIndentString(indent + 1), "body: "));

                    foreach (Type requestType in attribute.RequestTypes)
                    {
                        ContentTypeAttribute contentTypeAttribute = requestType.GetCustomAttributes<ContentTypeAttribute>().FirstOrDefault();
                        if (contentTypeAttribute == null)
                        {
                            SerialisationLog.Warning(string.Concat("No ContentTypeAttribute for ", requestType.FullName));
                        }

                        if (attribute.RequestTypeNames != null)
                        {
                            foreach (string contentType in attribute.RequestTypeNames)
                            {
                                TDataExchangeFormat dataExchangeFormat = SerialisationUtils.GetDataExchangeFormatFromContentType(contentType);

                                if (dataExchangeFormat != TDataExchangeFormat.None)
                                {
                                    WriteBody(writer, exampleStore, node, dataExchangeFormat, contentType, TMessageType.Request, requestType, indent + 2);
                                }
                                else
                                {
                                    SerialisationLog.Warning(string.Concat("No supported data exchange format for ", contentType)); 
                                }
                            }
                        }
                        else
                        {
                            foreach (TDataExchangeFormat dataExchangeFormat in Enum.GetValues(typeof(TDataExchangeFormat)))
                            {
                                if (SerialisationUtils.IsStandardDataExchangeFormat(dataExchangeFormat))
                                {
                                    string contentType = GetContentType(contentTypeAttribute, dataExchangeFormat);
                                    WriteBody(writer, exampleStore, node, dataExchangeFormat, contentType, TMessageType.Request, requestType, indent + 2);
                                }
                            }
                        }
                    }
                }

                writer.WriteLine(string.Concat(GetIndentString(indent + 1), "responses:"));

                HttpStatusCode[] statusCodes = attribute.StatusCodes;
                if (node.Class.GetCustomAttributes<AuthorizeAttribute>().FirstOrDefault() != null)
                {
                    statusCodes = statusCodes.Concat(new[] { HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden }).ToArray();
                }
                foreach (HttpStatusCode statusCode in statusCodes)
                {
                    writer.WriteLine(string.Concat(GetIndentString(indent + 2), (int)statusCode, ":"));
                    WriteDefaultDescription(writer, headerSettings, statusCode, indent + 3);

                    if (SerialisationUtils.IsSuccessStatusCode(statusCode) && attribute.ResponseTypes != null)
                    {
                        writer.WriteLine(string.Concat(GetIndentString(indent + 3), "body:"));
                        foreach (Type responseType in attribute.ResponseTypes)
                        {
                            ContentTypeAttribute contentTypeAttribute = responseType.GetCustomAttributes<ContentTypeAttribute>().FirstOrDefault();
                            if (contentTypeAttribute == null)
                            {
                                SerialisationLog.Warning(string.Concat("No ContentTypeAttribute for ", responseType.FullName));
                            }

                            foreach (TDataExchangeFormat dataExchangeFormat in Enum.GetValues(typeof(TDataExchangeFormat)))
                            {
                                if (SerialisationUtils.IsStandardDataExchangeFormat(dataExchangeFormat))
                                {
                                    string contentType = GetContentType(contentTypeAttribute, dataExchangeFormat);
                                    WriteBody(writer, exampleStore, node, dataExchangeFormat, contentType, TMessageType.Response, responseType, indent + 4);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                SerialisationLog.Warning(string.Concat("No method-level documentation for ", node.Class.Name, ".", node.Method.Name));
            }
        }

        private string GetContentType(ContentTypeAttribute contentTypeAttribute, TDataExchangeFormat dataExchangeFormat)
        {
            string contentType = null;
            if (contentTypeAttribute != null)
            {
                contentType = string.Concat(contentTypeAttribute.ContentType, "+", dataExchangeFormat.ToString().ToLower());
            }
            else
            {
                contentType = string.Concat("application/", dataExchangeFormat.ToString());
            }
            return contentType;
        }

        private void WriteBody(StreamWriter writer, ExampleStore exampleStore, ResourceNode node, TDataExchangeFormat dataExchangeFormat, string contentType, TMessageType messageType, Type bodyObjectType, int indent)
        {
            writer.WriteLine(string.Concat(GetIndentString(indent), contentType, ":"));
            if (SerialisationUtils.IsStandardDataExchangeFormat(dataExchangeFormat))
            {
                writer.WriteLine(string.Concat(GetIndentString(indent + 1), "schema: ", GetSchemaKey(bodyObjectType, dataExchangeFormat)));
            }
            WriteExample(writer, exampleStore, node.Class, node.Method, messageType, dataExchangeFormat, indent + 1);
        }

        private object GetSchemaKey(Type objectType, TDataExchangeFormat dataExchangeFormat)
        {
            return string.Concat(objectType.Name, dataExchangeFormat.ToString());
        }

        private void WriteDefaultDescription(StreamWriter writer, DocumentationHeaderSettings headerSettings, HttpStatusCode statusCode, int indent)
        {
            writer.WriteLine(string.Concat(GetIndentString(indent), "description: |"));
            writer.WriteLine(string.Concat(GetIndentString(indent + 1), SerialisationUtils.PrettifyHttpStatusCode(statusCode)));
            string defaultDescription = null;
            headerSettings.DefaultStatusDescriptions.TryGetValue(((int)statusCode).ToString(), out defaultDescription);
            if (defaultDescription != null)
            {
                writer.WriteLine(string.Concat(GetIndentString(indent + 1), defaultDescription));
            }
            else
            {
                SerialisationLog.Warning(string.Concat("No default description for Http Status Code ", (int)statusCode, ":", statusCode));
            }
        }

        private void WriteExample(StreamWriter writer, ExampleStore exampleStore, Type classType, MethodInfo methodInfo, TMessageType exampleType, TDataExchangeFormat dataExchangeFormat, int indent)
        {
            string exampleText = exampleStore.GetExampleContent(classType, methodInfo, exampleType, dataExchangeFormat);
            if (exampleText != null)
            {
                writer.WriteLine(string.Concat(GetIndentString(indent), "example: |"));
                List<string> lines = SerialisationUtils.SplitLines(exampleText);
                foreach (string line in lines)
                {
                    writer.WriteLine(string.Concat(GetIndentString(indent + 1), line));
                }
            }
            else if (dataExchangeFormat != TDataExchangeFormat.Xml)
            {
                SerialisationLog.Warning(string.Concat("No example for ", classType.Name, ".", methodInfo.Name, " ", dataExchangeFormat, " ", exampleType.ToString().ToLower()));
            }
        }

        private static string GetIndentString(int indent)
        {
            return new String(' ', RESOURCE_INDENT + indent * TAB_SIZE);
        }
    }
}

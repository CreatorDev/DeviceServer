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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Imagination
{
    public static class RequestExtensions
    {

        private static bool _IsDevelopment = false;

        public static void SetEnvironment(bool isDevelopment)
        {
            _IsDevelopment = isDevelopment;
        }

        public static Uri GetRequestUri(this HttpRequest request)
        {
            StringBuilder result = new StringBuilder();
            string protocol = request.Headers["X-Forwarded-Proto"];
            if (string.IsNullOrEmpty(protocol))
                result.Append(request.Scheme);
            else
                result.Append(protocol);
            result.Append("://");
            result.Append(request.Host);
            if (!string.IsNullOrEmpty(request.PathBase))
            {
                result.Append("/");
                result.Append(request.PathBase);
            }
            return new Uri(result.ToString());
        }

        public static PageInfo GetPageInfo(this HttpRequest request, int totalCount)
        {
            return PageInfo.ParseRequest(request, totalCount); 
        }
        
        public static string GetRootUrl(this HttpRequest request)
        {
            bool? secure = null;
            if (!_IsDevelopment)
                secure = true;
            return GetRootUrl(request, secure);
        }

        public static string GetRootUrl(this HttpRequest request, bool? secure)
        {
            StringBuilder result = new StringBuilder();
            if (request != null)
            {
                if (secure.HasValue)
                {
                    result.Append("http");

#if DEBUG
                    if (!request.Host.Value.StartsWith("localhost", StringComparison.InvariantCultureIgnoreCase) && secure.Value)
#else
                    if (secure.Value)
#endif
                    {
                        result.Append("s");
                    }
                }
                else
                {
                    string protocol = request.Headers["X-Forwarded-Proto"];
                    if (string.IsNullOrEmpty(protocol))
                        result.Append(request.Scheme);
                    else
                        result.Append(protocol);
                }
                result.Append("://");
                result.Append(request.Host.ToString());
                if (!string.IsNullOrEmpty(request.PathBase))
                {
                    result.Append("/");
                    result.Append(request.PathBase);
                }
            }
            return result.ToString();
        }


        public static ObjectResult GetObjectResult(this HttpRequest request, object item)
        {
            ObjectResult result = new ObjectResult(item);
            result.ContentTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(request.GetContentType(item)));
            return result;
        }

        public static ObjectResult GetObjectResult(this HttpRequest request, object item, HttpStatusCode statusCode)
        {
            ObjectResult result = new ObjectResult(item);
            result.ContentTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue(request.GetContentType(item)));
            result.StatusCode = (int)statusCode;
            return result;
        }

        public static string GetContentType(this HttpRequest request, object item)
        {
            Type type = item.GetType();
            return request.GetContentType(type);
        }

        public static string GetContentType(this HttpRequest request, string contentType)
        {
            string result = "application/json";
            string format = request.GetContentFormat();
           
            if (format != null)
                result = string.Concat(contentType, format);

            return result;
        }

        public static string GetContentType(this HttpRequest request, Type type)
        {
            string result = "application/json";
            if (type != null)
            {
                object[] attributes = type.GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    if (attribute is ContentTypeAttribute)
                    {
                        ContentTypeAttribute contentType = (ServiceModels.ContentTypeAttribute)attribute;
                        string format = request.GetContentFormat();
                      
                        if (format != null)
                            result = string.Concat(contentType.ContentType, format);
                        break;
                    }
                }
            }
            return result;
        }

        public static T Deserialise<T>(this HttpRequest request)
        {
            T deserialisedObject = default(T);
            if (request.ContentType.Contains("xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                try
                {
                    deserialisedObject = (T)serializer.Deserialize(request.Body);
                }
                catch (InvalidOperationException)
                {
                    throw new BadRequestException();
                }
            }
            else
            {
                try
                {
                    string body = new StreamReader(request.Body).ReadToEnd();
                    deserialisedObject = JsonConvert.DeserializeObject<T>(body);
                }
                catch (JsonReaderException)
                {
                    throw new BadRequestException();
                }
            }
            return deserialisedObject;
        }
    }
}
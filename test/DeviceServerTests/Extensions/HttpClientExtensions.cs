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

using DeviceServerTests.Utilities;
using Imagination.ServiceModels;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;


namespace DeviceServerTests.Extensions
{
    public static class HttpClientExtensions
    {
        static HttpClientExtensions()
        {
            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
        }
        private static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            // disable SSL certificate checking
            return true;
        }

        public static async Task<T> GetModel<T>(this HttpClient httpClient, HttpRequestMessage request, OAuthToken token = null)
        {
            HttpResponseMessage response = await SendRequest(httpClient, request, null, httpClient.GetContentType(typeof(T)), token);
            return await httpClient.GetModelFromResponse<T>(response);
        }

        public static async Task<T> GetModelFromResponse<T>(this HttpClient httpClient, HttpResponseMessage response)
        {
            T result = default(T);
            if (response.IsSuccessStatusCode)
            {
                string contentType = response.Content.Headers.ContentType.MediaType;

                if (contentType.Contains("xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    result = (T)serializer.Deserialize(await response.Content.ReadAsStreamAsync());
                }
                else
                {
                    string rawContent = await response.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<T>(rawContent);
                }
            }
            return result;
        }

        public static async Task<HttpContent> GetContent(this HttpClient httpClient, HttpRequestMessage request, Type serviceModelType = null, string mimeType = null, OAuthToken token = null)
        {
            string acceptContentType = null;
            if (mimeType != null)
            {
                acceptContentType = string.Concat(mimeType, "+", TestConfiguration.TestData.RestAPI.ContentType);
            }
            else if (serviceModelType != null)
            {
                acceptContentType = httpClient.GetContentType(serviceModelType);
            }

            HttpResponseMessage response = await SendRequest(httpClient, request, null, acceptContentType, token);
            HttpContent content = null;
            if (response.IsSuccessStatusCode)
            {
                content = response.Content;
            }
            return content;
        }

        public static async Task<HttpResponseMessage> SendRequest(this HttpClient httpClient, HttpRequestMessage request, string contentType, string acceptContentType, OAuthToken token = null)
        {
            if (token != null)
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(token.token_type, token.access_token);
            }

            if (contentType != null)
            {
                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            }

            if (acceptContentType == null)
            {
                acceptContentType = $"application/{TestConfiguration.TestData.RestAPI.ContentType}";
            }
            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(acceptContentType));

            HttpResponseMessage response = await httpClient.SendAsync(request);
            return response;
        }

        public static string GetContentType(this HttpClient httpClient, Type type)
        {
            string result = string.Concat("application/", TestConfiguration.TestData.RestAPI.ContentType);
            if (type != null)
            {
                object[] attributes = type.GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    if (attribute is ContentTypeAttribute)
                    {
                        ContentTypeAttribute contentType = (ContentTypeAttribute)attribute;
                        result = string.Concat(contentType.ContentType, "+", TestConfiguration.TestData.RestAPI.ContentType);
                        break;
                    }
                }
            }
            return result;
        }
    }
}
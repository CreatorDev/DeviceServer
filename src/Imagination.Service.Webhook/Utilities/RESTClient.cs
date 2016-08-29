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
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Imagination
{
    public class RESTClient
    {
        public class RESTResponse
        {
            public int StatusCode { get; set; }

            public string Content { get; set; }
        }

        public static async Task<RESTResponse> GetAsync(string url, string accept, string sessionToken)
        {
            RESTResponse result = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = accept;
            if (!string.IsNullOrEmpty(sessionToken))
            {
                request.Headers[HttpRequestHeader.Authorization] = string.Concat("Bearer ", sessionToken);
            }
            result = await ProcessResponse(request);
            return result;
        }

        private static async Task<RESTResponse> ProcessResponse(HttpWebRequest request)
        {
            RESTResponse result = new RESTResponse();
            try
            {
                using (HttpWebResponse response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    result.StatusCode = (int)response.StatusCode;
                    using (Stream stream = response.GetResponseStream())
                    {
                        result.Content = new StreamReader(stream, System.Text.Encoding.UTF8).ReadToEnd();
                    }
                }
            }
            catch (WebException ex)
            {
                if ((ex.Status == WebExceptionStatus.ProtocolError) && (ex.Response != null))
                {
                    HttpWebResponse response = ex.Response as HttpWebResponse;
                    result.StatusCode = (int)response.StatusCode;
                    using (Stream stream = response.GetResponseStream())
                    {
                        result.Content = new StreamReader(stream, System.Text.Encoding.UTF8).ReadToEnd();
                    }
                }
            }
            return result;
        }

        public static async Task<RESTResponse> PostAsync(string url, string accept, string sessionToken, string contentType, string content)
        {
            RESTResponse result = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (!string.IsNullOrEmpty(sessionToken))
            {
                request.Headers[HttpRequestHeader.Authorization] = string.Concat("Bearer ", sessionToken);
            }
            request.Accept = accept;
            request.ContentType = contentType;
            request.Method = HttpMethod.POST;
            if (!string.IsNullOrEmpty(content))
            {
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    using (StreamWriter writer = new StreamWriter(requestStream))
                    {
                        writer.Write(content);
                        writer.Flush();
                    }
                }
            }
            result = await ProcessResponse(request);
            return result;
        }

        public static async Task<RESTResponse> PutAsync(string url, string accept, string sessionToken, string contentType, string content)
        {
            RESTResponse result = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (!string.IsNullOrEmpty(sessionToken))
            {
                request.Headers[HttpRequestHeader.Authorization] = string.Concat("Bearer ", sessionToken);
            }
            request.Accept = accept;
            request.ContentType = contentType;
            request.Method = HttpMethod.PUT;
            if (!string.IsNullOrEmpty(content))
            {
                using (Stream requestStream = await request.GetRequestStreamAsync())
                {
                    using (StreamWriter writer = new StreamWriter(requestStream))
                    {
                        writer.Write(content);
                        writer.Flush();
                    }
                }
            }
            result = await ProcessResponse(request);
            return result;
        }

        public static async Task<RESTResponse> DeleteAsync(string url, string accept, string sessionToken)
        {
            RESTResponse result = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            if (!string.IsNullOrEmpty(sessionToken))
            {
                request.Headers[HttpRequestHeader.Authorization] = string.Concat("Bearer ", sessionToken);
            }
            request.Accept = accept;
            request.Method = HttpMethod.DELETE;
            result = await ProcessResponse(request);
            return result;
        }
    }
}
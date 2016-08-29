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
using Imagination.ServiceModels;
using System.Reflection;
using System.Net.Http;
using DeviceServerTests.Utilities;
using System.Threading.Tasks;
using System.IO;
using DeviceServerTests.Extensions;
using System.Net;
using Imagination;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace DeviceServerTests.Fixtures
{
    public class DeviceServerClientFixture : IDisposable
    {
        public HttpClient HttpClient { get; private set; }
        private OAuthToken _OAuthToken;

        public DeviceServerClientFixture()
        {
            HttpClient = new HttpClient();
            HttpClient.BaseAddress = new Uri(TestConfiguration.TestData.RestAPI.URI);
        }

        public void Dispose()
        {
            HttpClient.Dispose();
        }

        public async Task<OAuthToken> Login()
        {
            return await Login(TestConfiguration.TestData.RestAPI.Authentication.Key, TestConfiguration.TestData.RestAPI.Authentication.Secret);
        }

        public async Task<OAuthToken> Login(string key, string secret)
        {
            string body = string.Concat("grant_type=password&username=", WebUtility.UrlEncode(key), "&password=", WebUtility.UrlEncode(secret));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/oauth/token");
            request.Content = new StringContent(body);
            HttpResponseMessage response = await HttpClient.SendRequest(request, "application/x-www-form-urlencoded", HttpClient.GetContentType(typeof(OAuthToken)));

            _OAuthToken = await HttpClient.GetModelFromResponse<OAuthToken>(response);
            return _OAuthToken;
        }

        public async Task<OAuthToken> RefreshAccessToken(string refreshToken)
        {
            string body = string.Concat("grant_type=refresh_token&refresh_token=", WebUtility.UrlEncode(refreshToken));
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "/oauth/token");
            request.Content = new StringContent(body);
            HttpResponseMessage response = await HttpClient.SendRequest(request, "application/x-www-form-urlencoded", HttpClient.GetContentType(typeof(OAuthToken)));

            _OAuthToken = await HttpClient.GetModelFromResponse<OAuthToken>(response);
            return _OAuthToken;
        }


        public async Task<string> GetClientResource(string lwm2mClientID, string objectTypeID, string objectInstanceID, string resourceID)
        {
            string result = null;
            ObjectInstance matchedObjectInstance = await GetObjectInstanceModel(lwm2mClientID, objectTypeID, objectInstanceID);

            if (matchedObjectInstance != null)
            {
                foreach (Imagination.Model.Property property in matchedObjectInstance.Resource.Properties)
                {
                    System.Console.WriteLine(property.PropertyID);
                    if (property.PropertyID.Equals(resourceID))
                    {
                        result = property.Value.Value;
                        break;
                    }
                }
            }

            return result;
        }

        public Imagination.Model.PropertyDefinition GetResourceDefinition(Imagination.Model.ObjectDefinition objectDefinition, string resourceID)
        {
            foreach (Imagination.Model.PropertyDefinition property in objectDefinition.Properties)
            {
                if (property.PropertyID.Equals(resourceID))
                {
                    return property;
                }
            }
            return null;
        }

        public async Task<HttpResponseMessage> SetClientObject(string url, ObjectInstance objectInstance)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);

            MemoryStream stream = new MemoryStream(16384);
            string contentType = string.Concat(objectInstance.ObjectDefinition.MIMEType, "+", TestConfiguration.TestData.RestAPI.ContentType);

            if (contentType.Contains("xml"))
            {
                objectInstance.ToXml(stream);
            }
            else
            {
                objectInstance.ToJson(stream);
            }

            request.Content = new StreamContent(stream);
            
            return await HttpClient.SendRequest(request, contentType, null, _OAuthToken);
        }

        public async Task<HttpResponseMessage> Subscribe(string url, Subscription subscription)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Content = FromModel<Subscription>(subscription);
            string contentType = HttpClient.GetContentType(typeof(Subscription));
            return await HttpClient.SendRequest(request, contentType, null, _OAuthToken);
        }

        private HttpContent FromModel<T>(T model)
        {
            MemoryStream stream = new MemoryStream(16384);

            if (TestConfiguration.TestData.RestAPI.ContentType.Contains("xml"))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, model);
            }
            else
            {
                JsonConvert.SerializeObject(model);
            }

            return new StreamContent(stream);
        }

        public async Task<ObjectInstance> GetObjectInstanceModel(string lwm2mClientID, string objectTypeID, string objectInstanceID)
        {
            ObjectInstance objectInstance = null;
            APIEntryPoint entryPoint = await GetAPIEntryPoint();
            if (entryPoint != null)
            {
                Client client = await GetClient(lwm2mClientID, entryPoint);
                if (client != null)
                {
                    ObjectType objectType = await GetObjectType(client, objectTypeID);
                    if (objectType != null)
                    {
                        ObjectDefinition objectDefinition = await GetObjectDefinition(objectType);
                        if (objectDefinition != null)
                        {
                            objectInstance = await GetObjectInstance(objectDefinition, objectType, objectInstanceID);
                        }
                    }
                }
            }
            return objectInstance;
        }

        private async Task<ObjectInstance> GetObjectInstance(ObjectDefinition objectDefinition, ObjectType objectType, string objectInstanceID)
        {
            Link instancesLink = objectType.GetLink("instances");
            ObjectInstance matchedObjectInstance = null;

            HttpContent objectInstancesContent = await HttpClient.GetContent(new HttpRequestMessage(HttpMethod.Get, instancesLink.href), null, StringUtils.ToPlural(objectDefinition.MIMEType), _OAuthToken);
            
            if (objectInstancesContent != null)
            {
                ObjectInstances objectInstances = new ObjectInstances(objectDefinition.ToModel(), await objectInstancesContent.ReadAsStreamAsync(), objectInstancesContent.Headers.ContentType.MediaType);

                foreach (ObjectInstance objectInstance in objectInstances.Items)
                {
                    Link selfLink = objectInstance.GetLink("self");
                    if (selfLink != null)
                    {
                        string matchedObjectInstanceID = selfLink.href.Substring(selfLink.href.LastIndexOf('/') + 1);

                        if (matchedObjectInstanceID.Equals(objectInstanceID))
                        {
                            matchedObjectInstance = objectInstance;
                            break;
                        }
                    }
                }
            }

            return matchedObjectInstance;
        }

        private async Task<ObjectDefinition> GetObjectDefinition(ObjectType objectType)
        {
            Link metadataLink = objectType.GetLink("definition");
            ObjectDefinition objectDefinition = await HttpClient.GetModel<ObjectDefinition>(new HttpRequestMessage(HttpMethod.Get, metadataLink.href), _OAuthToken);
            return objectDefinition;
        }

        public async Task<APIEntryPoint> GetAPIEntryPoint()
        {
            return await HttpClient.GetModel<APIEntryPoint>(new HttpRequestMessage(HttpMethod.Get, "/"), _OAuthToken);
        }

        private async Task<Client> GetClient(string lwm2mClientID, APIEntryPoint entryPoint)
        {
            Link clientsLink = entryPoint.GetLink("clients");
            Clients clients = await HttpClient.GetModel<Clients>(new HttpRequestMessage(HttpMethod.Get, clientsLink.href), _OAuthToken);
            Client matchedClient = null;
            if (clients != null)
            {
                foreach (Client client in clients.Items)
                {
                    Link selfLink = client.GetLink("self");
                    string clientID = selfLink.href.Substring(selfLink.href.LastIndexOf('/') + 1);

                    if (clientID.Equals(lwm2mClientID))
                    {
                        matchedClient = client;
                        break;
                    }
                }
            }
            return matchedClient;
        }

        private async Task<ObjectType> GetObjectType(Client client, string objectTypeID)
        {
            ObjectType matchedObjectType = null;
            Link objectTypesLink = client.GetLink("objecttypes");
            ObjectTypes objectTypes = await HttpClient.GetModel<ObjectTypes>(new HttpRequestMessage(HttpMethod.Get, objectTypesLink.href), _OAuthToken);

            if (objectTypes != null)
            {
                foreach (ObjectType objectType in objectTypes.Items)
                {
                    if (objectType.ObjectTypeID.Equals(objectTypeID))
                    {
                        matchedObjectType = objectType;
                        break;
                    }
                }
            }
            return matchedObjectType;
        }
    }
}

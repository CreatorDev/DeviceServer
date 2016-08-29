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

using System.Net.Http;
using Xunit;
using DeviceServerTests.Fixtures;
using Imagination.ServiceModels;
using Imagination.Model.Subscriptions;
using Imagination;
using System.Text;
using System;

namespace DeviceServerTests.FunctionalTests
{
    public class SubscriptionTests : IClassFixture<DeviceServerClientFixture>, IClassFixture<LWM2MTestClientFixture>, IClassFixture<TestWebAppFixture>
    {
        private readonly DeviceServerClientFixture _HttpClientFixture;
        private readonly Imagination.LWM2M.Client _TestClient;
        private readonly TestWebAppFixture _TestWebAppFixture;
        public SubscriptionTests(DeviceServerClientFixture httpClientFixture, LWM2MTestClientFixture lwm2mTestClientFixture, TestWebAppFixture testWebAppFixture)
        {
            _HttpClientFixture = httpClientFixture;
            _TestClient = lwm2mTestClientFixture.Client;
            _TestWebAppFixture = testWebAppFixture;
        }
        public void Dispose()
        {

        }

        [Fact(Skip = "TODO")]
        public async void Post_CreateSubscription()
        {
            // Arrange
            string objectTypeID = "3";
            string objectInstanceID = "0";
            string resourceID = "9";

            await _HttpClientFixture.Login();

            ObjectInstance matchedObjectInstance = await _HttpClientFixture.GetObjectInstanceModel(_TestClient.ClientID, objectTypeID, objectInstanceID);
            Assert.NotNull(matchedObjectInstance);

            Imagination.Model.PropertyDefinition propertyDefinition = _HttpClientFixture.GetResourceDefinition(matchedObjectInstance.ObjectDefinition, resourceID);
            Assert.NotNull(propertyDefinition);

            Link subscriptionsLink = matchedObjectInstance.GetLink("subscriptions");
            Assert.NotNull(subscriptionsLink);

            string id = StringUtils.Encode(Encoding.ASCII.GetBytes(Environment.StackTrace));

            Subscription subscription = new Subscription();
            subscription.Url = "http://localhost:56789/subscriptions?testid="+id;
            subscription.Property = propertyDefinition.SerialisationName;
            subscription.SubscriptionType = TSubscriptionType.Observation.ToString();

            HttpResponseMessage response = await _HttpClientFixture.Subscribe(subscriptionsLink.href, subscription);
            Assert.True(response.IsSuccessStatusCode);

            // TODO: Change resource value on client.
            // TODO: Read from TestWebAppFixture client on the subscription URL

            response = await _TestWebAppFixture.Client.GetAsync(subscription.Url);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal("ABC", await response.Content.ReadAsStringAsync());
        }
    }
}

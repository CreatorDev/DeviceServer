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
using Imagination.LWM2M;
using DeviceServerTests.Fixtures;
using Imagination.LWM2M.Resources;
using Imagination.ServiceModels;
using DeviceServerTests.Extensions;
using DeviceServerTests.Utilities;
using Imagination;
using System.Threading.Tasks;
using System.Linq;
using CoAP.Server.Resources;
using Imagination.Model;
using System;

namespace DeviceServerTests.FunctionalTests
{
    public class InstancesTests : IClassFixture<DeviceServerClientFixture>, IClassFixture<LWM2MTestClientFixture>
    {
        private readonly DeviceServerClientFixture _HttpClientFixture;
        private readonly Imagination.LWM2M.Client _TestClient;

        public InstancesTests(DeviceServerClientFixture httpClientFixture, LWM2MTestClientFixture lwm2mTestClientFixture)
        {
            _HttpClientFixture = httpClientFixture;
            _TestClient = lwm2mTestClientFixture.Client;
        }
        public void Dispose()
        {

        }

        [Fact]
        public async void ReadStringResource()
        {
            // Arrange
            string expectedValue = "Imagination Technologies 123";
            string objectTypeID = "3";
            string objectInstanceID = "0";
            string resourceID = "0";

            await _HttpClientFixture.Login();

            IResource resource = _TestClient.GetResource($"{objectTypeID}/{objectInstanceID}/{resourceID}");
            Assert.NotNull(resource);
            (resource as LWM2MResource).SetValue(expectedValue);

            // Act
            string actualValue = await _HttpClientFixture.GetClientResource(_TestClient.ClientID, objectTypeID, objectInstanceID, resourceID);

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public async void SetStringResource()
        {
            // Arrange
            string expectedValue = "Imagination Technologies 123";
            string objectTypeID = "3";
            string objectInstanceID = "0";
            string resourceID = "0";

            await _HttpClientFixture.Login();

            ObjectInstance matchedObjectInstance = await _HttpClientFixture.GetObjectInstanceModel(_TestClient.ClientID, objectTypeID, objectInstanceID);
            Assert.NotNull(matchedObjectInstance);
            
            Imagination.Model.PropertyDefinition propertyDefinition = _HttpClientFixture.GetResourceDefinition(matchedObjectInstance.ObjectDefinition, resourceID);
            Assert.NotNull(propertyDefinition);

            Imagination.Model.Object objectModel = new Imagination.Model.Object();
            Imagination.Model.Property propertyToSet = new Imagination.Model.Property();
            propertyToSet.PropertyDefinitionID = propertyDefinition.PropertyDefinitionID;
            propertyToSet.PropertyID = propertyDefinition.PropertyID;
            propertyToSet.Value = new Imagination.Model.PropertyValue(expectedValue);
            objectModel.Properties.Add(propertyToSet);

            ObjectInstance objectInstanceToSet = new ObjectInstance(matchedObjectInstance.ObjectDefinition, objectModel);

            Link selfLink = matchedObjectInstance.GetLink("self");
            Assert.NotNull(selfLink);

            HttpResponseMessage response = await _HttpClientFixture.SetClientObject(selfLink.href, objectInstanceToSet);
            Assert.True(response.IsSuccessStatusCode);

            // Act
            IResource resource = _TestClient.GetResource($"{objectTypeID}/{objectInstanceID}/{resourceID}");
            Assert.NotNull(resource);
            string actualValue = (resource as LWM2MResource).ToString();

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }


        
    }
}

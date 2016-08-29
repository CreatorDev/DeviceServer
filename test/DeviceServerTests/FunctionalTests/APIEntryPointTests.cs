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
using System.Net.Http;
using Xunit;
using DeviceServerTests.Extensions;
using DeviceServerTests.Fixtures;
using DeviceServerTests.Utilities;

namespace DeviceServerTests.FunctionalTests
{
    public class APIEntryPointTests : IClassFixture<DeviceServerClientFixture>
    {
        private readonly DeviceServerClientFixture _HttpClientFixture;
        public APIEntryPointTests(DeviceServerClientFixture fixture)
        {
            _HttpClientFixture = fixture;
        }
        public void Dispose()
        {

        }

        [Fact]
        public async void AnonymousLinks()
        {
            // Arrange
            string contentType = TestConfiguration.TestData.RestAPI.ContentType;
            LinkableResource expected = new LinkableResource();
            expected.AddLink("authenticate", null, $"application/vnd.imgtec.oauthtoken+{contentType}");
            expected.AddLink("versions", null, $"application/vnd.imgtec.versions+{contentType}");

            // Act
            APIEntryPoint entryPoint = await _HttpClientFixture.GetAPIEntryPoint();

            // Assert
            ExpectLinks(entryPoint, expected);
        }

        [Fact]
        public async void AuthenticatedLinks()
        {
            // Arrange
            string contentType = TestConfiguration.TestData.RestAPI.ContentType;
            LinkableResource expected = new LinkableResource();
            expected.AddLink("authenticate", null, $"application/vnd.imgtec.oauthtoken+{contentType}");
            expected.AddLink("versions", null, $"application/vnd.imgtec.versions+{contentType}");
            expected.AddLink("accesskeys", null, $"application/vnd.imgtec.accesskeys+{contentType}");
            expected.AddLink("configuration", null, $"application/vnd.imgtec.configuration+{contentType}");
            expected.AddLink("clients", null, $"application/vnd.imgtec.clients+{contentType}");
            expected.AddLink("identities", null, $"application/vnd.imgtec.identities+{contentType}");
            expected.AddLink("objectdefinitions", null, $"application/vnd.imgtec.objectdefinitions+{contentType}");
            expected.AddLink("subscriptions", null, $"application/vnd.imgtec.subscriptions+{contentType}");
            expected.AddLink("metrics", null, $"application/vnd.imgtec.metrics+{contentType}");

            // Act
            await _HttpClientFixture.Login();
            APIEntryPoint entryPoint = await _HttpClientFixture.GetAPIEntryPoint();

            //Assert
            ExpectLinks(entryPoint, expected);
        }

        private void ExpectLinks(APIEntryPoint entryPoint, LinkableResource expected)
        {
            Assert.NotNull(entryPoint);
            Assert.NotNull(entryPoint.Links);
            Assert.Equal(expected.Links.Count, entryPoint.Links.Count);

            int matches = 0;

            foreach (Link actualLink in entryPoint.Links)
            {
                Assert.NotNull(actualLink.rel);
                Assert.NotNull(actualLink.href);
                Assert.NotEmpty(actualLink.href);
                Assert.NotNull(actualLink.type);

                foreach (Link expectedLink in expected.Links)
                {
                    if (actualLink.rel.Equals(expectedLink.rel))
                    {
                        Assert.Equal(expectedLink.type, actualLink.type);
                        matches++;
                    }
                }
            }

            Assert.Equal(matches, entryPoint.Links.Count);
        }
    }
}

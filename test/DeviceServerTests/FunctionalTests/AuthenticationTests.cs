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
using System.Threading.Tasks;

namespace DeviceServerTests.FunctionalTests
{
    public class AuthenticationTests : IClassFixture<DeviceServerClientFixture>
    {
        private readonly DeviceServerClientFixture _HttpClientFixture;
        public AuthenticationTests(DeviceServerClientFixture fixture)
        {
            _HttpClientFixture = fixture;

        }
        public void Dispose()
        {

        }

        [Fact]
        public async void AuthenticateWithPassword()
        {
            // Act
            OAuthToken token = await _HttpClientFixture.Login();

            // Assert
            Assert.NotNull(token);
            Assert.NotNull(token.access_token);
            Assert.NotEmpty(token.access_token);
            Assert.NotNull(token.refresh_token);
            Assert.NotEmpty(token.refresh_token);
            Assert.Equal("Bearer", token.token_type);
            Assert.True(token.expires_in > 0);
        }

        [Fact]
        public async void AuthenticateWithRefreshToken()
        {
            // Arrange
            OAuthToken token = await _HttpClientFixture.Login();
            Assert.NotNull(token);
            Assert.NotNull(token.access_token);
            Assert.NotNull(token.refresh_token);
            await Task.Delay(1000);  // otherwise the same refresh token may be returned

            // Act
            OAuthToken newToken = await _HttpClientFixture.RefreshAccessToken(token.refresh_token);

            // Assert
            Assert.NotNull(newToken);
            Assert.NotNull(newToken.access_token);
            Assert.NotNull(newToken.refresh_token);
            Assert.NotEqual(token.access_token, newToken.access_token);
            Assert.NotEqual(token.refresh_token, newToken.refresh_token);
            Assert.Equal("Bearer", newToken.token_type);
            Assert.True(newToken.expires_in > 0);
        }
    }
}

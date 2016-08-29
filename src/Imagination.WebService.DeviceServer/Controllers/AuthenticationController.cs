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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Imagination.BusinessLogic;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Imagination.Documentation;

namespace Imagination.Controllers
{
    [RouteDocumentation(Route = "/oauth/token", DisplayName = "Authentication", Summary = "Retrieve refresh tokens and access tokens to authenticate with the Device Server REST API.")]
    [AllowAnonymous]
    [Route("/oauth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly ILogger _logger;
        private JwtBearerOptions _AuthOptions;
        private SigningCredentials _SigningCredentials;
        private const int ACCESS_TOKEN_EXPIRY = 3600;  // one hour
        private const int REFRESH_TOKEN_EXPIRY = 7 * 24 * 3600;  // one week
        private const string RefreshTokenClaim = "RT";
        private const string RefreshTokenExists = "1";

        public AuthenticationController(ILogger<AuthenticationController> logger, IOptions<JwtBearerOptions> authOptions, SigningCredentials signingCredentials)
        {
            _logger = logger;
            _AuthOptions = authOptions.Value;
            _SigningCredentials = signingCredentials;
        }

        [MethodDocumentation(
            Summary = "Create an access token or a refresh token based on the request grant type.",
            RequestTypeNames = new[] { "application/x-www-form-urlencoded" },
            RequestTypes = new[] { typeof(OAuthTokenRequest) },
            ResponseTypes = new[] { typeof(OAuthToken) },
            StatusCodes = new[] { HttpStatusCode.Created, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest }
        )]
        [HttpPost("/oauth/token")]
        public IActionResult CreateAccessToken([FromForm]OAuthTokenRequest tokenRequest)
        {
            IActionResult result = null;

            if (string.Compare(tokenRequest.grant_type, "password", true) == 0)
            {
                Model.AccessKey accessKey = BusinessLogicFactory.AccessKeys.GetAccessKey(tokenRequest.username);
                if (accessKey != null)
                {
                    if (string.Compare(tokenRequest.password, accessKey.Secret) == 0)
                    {
                        OAuthToken token = CreateOAuthToken(accessKey.OrganisationID);
                        result = new ObjectResult(token) { StatusCode = (int)HttpStatusCode.Created };
                    }
                    else
                    {
                        _logger.LogDebug($"Incorrect Secret for Organisation {accessKey.OrganisationID} with access key: {accessKey.Name}");
                        result = new UnauthorizedResult();
                    }
                }
                else
                {
                    _logger.LogDebug($"No organisation with key: {tokenRequest.username}");
                    result = new UnauthorizedResult();
                }
            }
            else if (string.Compare(tokenRequest.grant_type, "refresh_token", true) == 0)
            {
                OrganisationSecurityTokenHandler handler = _AuthOptions.SecurityTokenValidators.OfType<OrganisationSecurityTokenHandler>().FirstOrDefault();
                JwtSecurityToken securityToken = handler.ReadJwtToken(tokenRequest.refresh_token);

                if (securityToken != null)
                {
                    Claim organisationClaim = securityToken.Claims.ToList().Find(c => c.Type.Equals(OrganisationIdentity.OrganisationClaim));
                    Claim refreshTokenClaim = securityToken.Claims.ToList().Find(c => c.Type.Equals(RefreshTokenClaim));

                    if (organisationClaim != null && refreshTokenClaim != null && refreshTokenClaim.Value.Equals(RefreshTokenExists))
                    {
                        int organisationID;
                        if (int.TryParse(organisationClaim.Value, out organisationID) && organisationID > 0)
                        {
                            OAuthToken token = CreateOAuthToken(organisationID);
                            result = new ObjectResult(token) { StatusCode = (int)HttpStatusCode.Created };
                        }
                        else
                        {
                            _logger.LogDebug($"Failed to parse organisationID in refresh token: {tokenRequest.refresh_token}");
                            result = new BadRequestResult();
                        }
                    }
                    else
                    {
                        _logger.LogDebug($"Refresh token does not have expected claims: {tokenRequest.refresh_token}");
                        result = new BadRequestResult();
                    }
                }
                else
                {
                    _logger.LogDebug($"Invalid refresh token: {tokenRequest.refresh_token}");
                    result = new BadRequestResult();
                }
            }
            else
            {
                result = new BadRequestResult();
            }

            return result;
        }

        private OAuthToken CreateOAuthToken(int organisationID)
        {
            OAuthToken token = new OAuthToken();
            token.token_type = "Bearer";
            token.expires_in = ACCESS_TOKEN_EXPIRY;
            token.access_token = CreateJWTToken(organisationID, token.expires_in);
            token.refresh_token = CreateJWTToken(organisationID, REFRESH_TOKEN_EXPIRY, new Claim(RefreshTokenClaim, RefreshTokenExists));
            return token;
        }

        private string CreateJWTToken(int organisationID, int expiry, Claim claim = null)
        {
            OrganisationIdentity identity = new OrganisationIdentity();
            identity.OrganisationID = organisationID;
            if (claim != null)
                identity.AddClaim(claim);
            DateTime expires = DateTime.UtcNow.AddSeconds(expiry);
            OrganisationSecurityTokenHandler handler = _AuthOptions.SecurityTokenValidators.OfType<OrganisationSecurityTokenHandler>().FirstOrDefault();
            JwtSecurityToken securityToken = handler.CreateJwtSecurityToken(
                //issuer: _AuthOptions.TokenValidationParameters.ValidIssuer,
                //audience: _AuthOptions.TokenValidationParameters.ValidAudience,
                signingCredentials: _SigningCredentials,
                subject: identity,
                expires: expires
            );
            return handler.WriteToken(securityToken);
        }
    }
}

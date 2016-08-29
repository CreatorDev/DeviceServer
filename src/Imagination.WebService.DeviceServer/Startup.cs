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
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Imagination.WebService.DeviceServer
{
    public class Startup
    {
        private const string TOKEN_AUDIENCE = "DeviceServer";
        private const string TOKEN_ISSUER = "DeviceServer";
        ILoggerFactory _LoggerFactory;

        public Startup(IHostingEnvironment env, ILoggerFactory loggerFactory)
        {

            _LoggerFactory = loggerFactory;

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            ServiceConfiguration.LoadConfig(Configuration.GetSection("ServiceConfiguration"));
            ServiceConfiguration.LoggerFactory = _LoggerFactory;
            BusinessLogic.BusinessLogicFactory.Initialise();
            RequestExtensions.SetEnvironment(env.IsDevelopment());
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddAuthentication();

            SecurityKey signingKey;
            //RSACryptoServiceProvider randomRSA = new RSACryptoServiceProvider(2048);
            //RSAParameters keyParams = randomRSA.ExportParameters(true);
            //signingKey = new RsaSecurityKey(keyParams);
            //services.AddSingleton(new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256));

            signingKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(ServiceConfiguration.SigningKey)); // This could be changed every hour or so
            signingKey.CryptoProviderFactory = new MonoFriendlyCryptoProviderFactory(_LoggerFactory.CreateLogger<MonoFriendlyCryptoProviderFactory>());

            services.AddSingleton(new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser().Build());
            });
            services.Configure<JwtBearerOptions>(options =>
            {
                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new OrganisationSecurityTokenHandler());
                options.TokenValidationParameters.IssuerSigningKey = signingKey;

                // Basic settings - signing key to validate with, audience and issuer.
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateIssuer = false;
                //options.TokenValidationParameters.ValidAudience = TOKEN_AUDIENCE;
                //options.TokenValidationParameters.ValidIssuer = TOKEN_ISSUER;

                // When receiving a token, check that we've signed it.
                options.TokenValidationParameters.ValidateLifetime = true;

                // Where external tokens are used, some leeway here could be useful.
                options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(0);

                options.AutomaticAuthenticate = true;
#if DEBUG
                options.RequireHttpsMetadata = false; // not in prod
#else
                options.RequireHttpsMetadata = true;
#endif
            });


            services.AddMvc();
            services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                options.RespectBrowserAcceptHeader = true;
                options.InputFormatters.Clear();
                options.InputFormatters.Add(new MediaTypeJsonInputFormatter(_LoggerFactory.CreateLogger<MediaTypeJsonInputFormatter>()));
                options.InputFormatters.Add(new MediaTypeXmlSerializerInputFormatter());
                options.OutputFormatters.Clear();
                options.OutputFormatters.Add(new MediaTypeJsonOutputFormatter());
                options.OutputFormatters.Add(new MediaTypeXmlSerializerOutputFormatter());

                // Register filter globally
                options.Filters.Add(new ExceptionResultFilterAttribute(_LoggerFactory.CreateLogger<ExceptionResultFilterAttribute>()));
            });

#if DEBUG
            services.AddCors();
            var policy = new Microsoft.AspNetCore.Cors.Infrastructure.CorsPolicy();

            policy.Headers.Add("*");
            policy.Methods.Add("*");
            policy.Origins.Add("*");
            policy.SupportsCredentials = true;

            services.Configure<Microsoft.AspNetCore.Cors.Infrastructure.CorsOptions>(x => x.AddPolicy("allowEveryThingPolicy", policy));
#endif

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<JwtBearerOptions> authOptions)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseJwtBearerAuthentication(authOptions.Value);

#if DEBUG
            app.UseCors("allowEveryThingPolicy");
#endif

            app.UseStaticFiles();

            app.UseMvc();


            ServiceConfiguration.DisplayConfig();
        }

    }
}

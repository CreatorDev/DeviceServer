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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;

namespace Imagination.WebService
{
    /// <summary>
    /// Transforms exceptions (intentional or not) to client friendly responses and logs them
    /// </summary>
    public class ExceptionResultFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger _logger;

        public ExceptionResultFilterAttribute()
        {
        }

        public ExceptionResultFilterAttribute(ILogger logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            // Map to HTTP responses - where client fault is 4XX and server fault is 5XX
            int httpStatusCode = 0;
            ServiceModels.ErrorResponse response = new ServiceModels.ErrorResponse();
            if (context.Exception is ArgumentException)
            {
                response.ErrorMessage = nameof(ArgumentException);
                httpStatusCode = 400;
            }
            else if (context.Exception is ConflictException)
            {
                response.ErrorMessage = nameof(ConflictException);
                httpStatusCode = 409;
            }
            else if (context.Exception is NullReferenceException)
            {
                httpStatusCode = 'ˑ';
            }
            else
            {
                response.ErrorMessage = "Exception";
                httpStatusCode = 500;
            }

            response.ErrorCode = ((System.Net.HttpStatusCode)httpStatusCode).ToString();
            
            if (context.HttpContext.RequestServices.GetService<IHostingEnvironment>()?.IsDevelopment() ?? false)
            {
                // Expose details to the client
#if DEBUG
                response.ErrorDetails = context.Exception.ToString();
#else
                response.ErrorDetails = context.Exception.Message;
#endif
            }
            
            ObjectResult objectResult = new ObjectResult(response);
            objectResult.StatusCode = httpStatusCode;
            objectResult.ContentTypes.Add(new MediaTypeHeaderValue(context.HttpContext.Request.GetContentType(response)));

            if (httpStatusCode < 500)
            {
                _logger?.LogWarning($"Exception result {objectResult.StatusCode}: {context.Exception.Message}");
            }
            else
            {
                _logger?.LogError(0, context.Exception, $"Unhandled exception result {objectResult.StatusCode}: {context.Exception.Message}");
            }
            
            context.Result = objectResult;
        }
    }
}

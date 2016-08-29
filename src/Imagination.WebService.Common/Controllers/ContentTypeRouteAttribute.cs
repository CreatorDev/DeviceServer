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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using System;

namespace Imagination.Controllers
{
    public class ContentTypeRouteAttribute : RouteAttribute, IActionConstraintFactory
    {
        private ContentTypeConstraint _constraint;

        /// <summary>
        /// Restrict this route to the requests with the a Content-Type that matches.
        /// </summary>
        /// <param name="template">the url template we are applying the constraint</param>
        /// <param name="contentTypeBase">media type including sub-type but excluding the suffix (+)"</param>
        public ContentTypeRouteAttribute(string template, string contentTypeBase) :
            base(template)
        {
            _constraint = new ContentTypeConstraint(contentTypeBase);
        }

        /// <summary>
        /// Restrict this route to the requests with the a Content-Type that matches.
        /// </summary>
        /// <param name="template"></param>
        /// <param name="model">Request model which has a ContentTypeAttribute</param>
        public ContentTypeRouteAttribute(string template, Type model) :
            base(template)
        {
            if (model != null)
            {
                object[] attributes = model.GetCustomAttributes(false);
                foreach (object attribute in attributes)
                {
                    if (attribute is ContentTypeAttribute)
                    {
                        ContentTypeAttribute contentType = (ContentTypeAttribute)attribute;
                        _constraint = new ContentTypeConstraint(contentType.ContentType);
                        break;
                    }
                }
            }
            if (_constraint == null)
                throw new ArgumentException("Request model invalid or does not have ContentTypeAttribute", nameof(model));
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public IActionConstraint CreateInstance(IServiceProvider services)
        {
            return _constraint;
        }
    }
}

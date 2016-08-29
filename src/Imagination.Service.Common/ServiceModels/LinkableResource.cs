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

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.ServiceModels
{
    public class LinkableResource
    {
        public List<Link> Links { get; set; }

        public LinkableResource()
        {
            // Don't create links collection here in the constructor as this will turn
            // up as an empty array in the json output. The collection will be created 
            // by a call to the AddLink() function.
        }

        public void AddLink(string rel, string href, string type)
        {
            if (Links == null)
                Links = new List<Link>();
           
            Links.Add(new Link { rel = rel, href = href, type = type });
        }


        [Obsolete("This guesses the link type, explicitly pass ResponseContentTypes instead")]
        public void AddSelfLink(string url)
        {
            AddSelfLink(url, null, false, false);
        }

        public void AddSelfLink(string url, string typeName)
        {
            AddSelfLink(url, typeName, false, false);
        }

        public void AddSelfLink(string url, bool includeUpdate, bool includeDelete)
        {
            AddSelfLink(url, null, includeUpdate, includeDelete);
        }

        public void AddSelfLink(string url, string typeName, bool includeUpdate, bool includeDelete)
        {
            if (Links == null)
                Links = new List<Link>();
            Type type = GetType();
            string contentType = typeName;
            
            Links.Add(new Link { rel = "self", href = url, type = contentType });
            if (includeUpdate)
                Links.Add(new Link { rel = "update", href = url, type = contentType });
            if (includeDelete)
                Links.Add(new Link { rel = "remove", href = url, type = contentType });
        }

        public bool HasLink(string rel)
        {
            Link link = GetLink(rel);
            return link != null;
        }

        public Link GetLink(string rel)
        {
            Link result = null;
            if (Links != null)
            {
                foreach (Link item in Links)
                {
                    if (string.Compare(item.rel, rel, StringComparison.CurrentCultureIgnoreCase) == 0)
                    {
                        result = item;
                        break;
                    }
                }
            }
            return result;
        }
    }
}

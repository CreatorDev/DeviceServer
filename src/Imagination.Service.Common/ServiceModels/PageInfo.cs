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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Imagination.ServiceModels
{
    public class PageInfo : LinkableResource
    {
        public int TotalCount { get; set; }

        public int ItemsCount { get; set; }

        public int StartIndex { get; set; }

        public PageInfo()
        {

        }

        private void AddPreviousNextPageLinks(Microsoft.AspNetCore.Http.HttpRequest request, int pageSize)
        {
            if (this.TotalCount != 0 && pageSize > 0)
            {
                StringBuilder url = new StringBuilder();
                string protocol = request.Headers["X-Forwarded-Proto"];
                if (string.IsNullOrEmpty(protocol))
                    url.Append(request.Scheme);
                else
                    url.Append(protocol);
                url.Append("://");
                url.Append(request.Host);

                if (!string.IsNullOrEmpty(request.PathBase))
                {
                    url.Append("/");
                    url.Append(request.PathBase);
                }

                if (!string.IsNullOrEmpty(request.Path))
                {
                    url.Append(request.Path);
                }

                url.Append("?");
                int startLength = url.Length;
                bool addedPageSize = false;
                bool addedStartIndex = false;
                int count = 0;
                foreach (string key in request.Query.Keys)
                {
                    if ((string.Compare(key, "referrerUrl", true) != 0) && (string.Compare(key, "session_token", true) != 0))
                    {
                        if (count > 0)
                            url.Append("&");
                        url.Append(key);
                        url.Append("=");
                        if (string.Compare(key, "pagesize", true) == 0)
                        {
                            url.Append(pageSize);
                            addedPageSize = true;
                        }
                        else if (string.Compare(key, "startindex", true) == 0)
                        {
                            url.Append("{0}");
                            addedStartIndex = true;
                        }
                        else
                            url.Append(WebUtility.UrlEncode(request.Query[key]));                        
                        count++;
                    }
                }
                if (!addedPageSize && pageSize != 20)
                {
                    if (url.Length > startLength)
                        url.Append("&");
                    url.Append("pageSize=");
                    url.Append(pageSize);
                }
                if (!addedStartIndex)
                {
                    if (url.Length > startLength)
                        url.Append("&");
                    url.Append("startIndex={0}");
                }
                if (this.StartIndex > 0)
                {
                    if (this.Links == null)
                        this.Links = new List<Link>();
                    this.Links.Add(new Link
                    {
                        rel = "first",
                        href = string.Format(url.ToString(), 0)
                    });
                    this.Links.Add(new Link
                    {
                        rel = "prev",
                        href = string.Format(url.ToString(), Math.Max(0, this.StartIndex - pageSize))
                    });
                }
                if (this.TotalCount < 0)
                {
                    if (Math.Abs(this.TotalCount) > pageSize)
                    {
                        if (this.Links == null)
                            this.Links = new List<Link>();
                        this.Links.Add(new Link
                        {
                            rel = "next",
                            href = string.Format(url.ToString(), (this.StartIndex + this.ItemsCount))
                        });
                    }
                }
                else
                {

                    if ((this.StartIndex + this.ItemsCount) < this.TotalCount)
                    {
                        if (this.Links == null)
                            this.Links = new List<Link>();
                        this.Links.Add(new Link
                        {
                            rel = "next",
                            href = string.Format(url.ToString(), (this.StartIndex + this.ItemsCount))
                        });
                        this.Links.Add(new Link
                        {
                            rel = "last",
                            href = string.Format(url.ToString(), Math.Max(0, this.TotalCount - pageSize))
                        });
                    }
                }
            }
        }

        public static PageInfo ParseRequest(Microsoft.AspNetCore.Http.HttpRequest request, int totalCount)
        {
            PageInfo result = new PageInfo();
            string paramStartIndex = null;
            string paramPageSize = null;
            Microsoft.AspNetCore.Http.IQueryCollection parameters = request.Query;
            if (parameters != null)
            {
                paramStartIndex = parameters["startIndex"];
                paramPageSize = parameters["pageSize"];
            }
            int startIndex, pageSize;
            if (!int.TryParse(paramStartIndex, out startIndex))
                startIndex = 0;
            if (!int.TryParse(paramPageSize, out pageSize))
                pageSize = 20;
            result.StartIndex = startIndex;
            result.TotalCount = totalCount;
            if (totalCount < 0)
            {
                result.ItemsCount = Math.Min(pageSize, Math.Abs(totalCount));
                result.AddPreviousNextPageLinks(request, pageSize);
                result.TotalCount = -1;
            }
            else
            {
                result.ItemsCount = Math.Max(0, Math.Min((startIndex + pageSize), totalCount) - startIndex);
                result.AddPreviousNextPageLinks(request, pageSize);
            }
            return result;
        }

    }
}

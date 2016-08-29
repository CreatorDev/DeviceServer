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

using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Imagination.Tools.APIDocGenerator
{
    public class ResourceNode
    {
        public string Key { get; set; }
        public Type Class { get; set; }
        public MethodInfo Method { get; set; }
        public ResourceNode Parent { get; set; }
        public Dictionary<string, ResourceNode> Children { get; private set; }

        public ResourceNode()
        {
            Children = new Dictionary<string, ResourceNode>();
        }

        public ResourceNode(ResourceNode parent, string key, Type classType) : this()
        {
            Parent = parent;
            Key = key;
            Class = classType;
        }

        public ResourceNode Find(string className, string methodName)
        {
            ResourceNode node = null;
            if (Class != null && Class.Name.Equals(className) && Method != null && Method.Name.Equals(methodName))
            {
                node = this;
            }
            else
            {
                foreach (ResourceNode child in Children.Values)
                {
                    node = child.Find(className, methodName);
                    if (node != null)
                    {
                        break;
                    }
                }
            }
            return node;
        }

        public bool AllowsAnonymous()
        {
            bool allowAnonymous = false;
            if (Class != null && Class.GetCustomAttributes<AllowAnonymousAttribute>().FirstOrDefault() != null)
                allowAnonymous = true;
            else if (Method != null && Method.GetCustomAttributes<AllowAnonymousAttribute>().FirstOrDefault() != null)
                allowAnonymous = true;
            return allowAnonymous;
        }

        public bool HasMethods()
        {
            return Children.Values.Any(c => c.Method != null);
        }

        public string GetRoute()
        {
            string route = Key;
            if (Parent == null)
            {
                route = "";
            }
            else
            {
                route = string.Concat(Parent.GetRoute(), "/", route);
            }
            return route;
        }
    }
}

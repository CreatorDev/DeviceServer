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

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Imagination.Tools.APIDocGenerator
{
    public static class AssemblyReader
    {
        private const string API_ENTRYPOINT = "";
        private static readonly string[] HTTP_METHODS = { "get", "put", "post", "delete", "head", "patch", "options" };

        private class RouteComparer : IComparer<RouteAttribute>
        {
            public int Compare(RouteAttribute x, RouteAttribute y)
            {
                // entrypoint first, then alphabetical
                if (x.Template.Equals("/") && !y.Template.Equals("/"))
                {
                    return -1;
                }
                else if (!x.Template.Equals("/") && y.Template.Equals("/"))
                {
                    return 1;
                }
                else
                {
                    return x.Template.CompareTo(y.Template);
                }
            }
        }

        private class MethodTemplateComparer : IComparer<HttpMethodAttribute>
        {
            public int Compare(HttpMethodAttribute x, HttpMethodAttribute y)
            {
                // empty templates first (no path), then alphabetical
                if (x.Template == null && y.Template == null)
                {
                    return 0;
                }
                else if (x.Template != null && y.Template == null)
                {
                    return 1;
                }
                else if (x.Template == null && y.Template != null)
                {
                    return -1;
                }
                else
                {
                    return x.Template.CompareTo(y.Template);
                }
            }
        }

        public static ResourceNode ReadAssembly(Assembly assembly)
        {
            ResourceNode tree = new ResourceNode();

            Type[] types = assembly.GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ControllerBase)) && t.GetCustomAttribute<RouteAttribute>() != null)
                .OrderBy(t => t.GetCustomAttribute<RouteAttribute>(), new RouteComparer())
                .ToArray();
            
            foreach (Type classType in types)
            {
                object[] attributes = classType.GetCustomAttributes(true);
                string resourceRoute = classType.GetCustomAttribute<RouteAttribute>().Template;
                if (resourceRoute != null)
                {
                    MethodInfo[] methods = classType.GetMethods()
                        .Where(m => m.GetCustomAttribute<HttpMethodAttribute>() != null)
                        .OrderBy(m => m.GetCustomAttribute<HttpMethodAttribute>(), new MethodTemplateComparer())
                        .ToArray();

                    ResourceNode classNode = CreateResourceNode(tree, resourceRoute, classType);

                    foreach (MethodInfo methodInfo in methods)
                    {
                        HttpMethodAttribute methodAttribute = methodInfo.GetCustomAttribute<HttpMethodAttribute>();
                        string httpMethod = methodAttribute.HttpMethods.FirstOrDefault().ToLower();
                        string template = null;
                        bool rootTemplate = false;
                        if (methodAttribute.Template != null)
                        {
                            template = string.Concat(methodAttribute.Template, "/");
                            if (!template.StartsWith("/"))
                            {
                                template = string.Concat("/", template);
                            }
                            else if (template.StartsWith(resourceRoute))
                            {
                                template = template.Substring(resourceRoute.Length);
                            }
                            else
                            {
                                rootTemplate = true;
                            }
                        }
                        else
                        {
                            template = "/"; 
                        }

                        string fullPath = string.Concat(template, httpMethod);
                        if (!rootTemplate)
                        {
                            fullPath = string.Concat(resourceRoute, fullPath);
                        }

                        ResourceNode methodNode = CreateResourceNode(tree, fullPath, classType);
                        if (methodNode.Method != null)
                        {
                            SerialisationLog.Error(string.Concat("Multiple versions of ", methodNode.Class.Name, ".", methodNode.Method.Name, " exist"));
                        }
                        methodNode.Method = methodInfo;
                    }
                }
            }

            return tree;
        }

        private static ResourceNode CreateResourceNode(ResourceNode parent, string path, Type classType)
        {
            string[] parts = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                if (parent.Parent == null && IsHTTPVerb(parts[0]))
                {
                    parent = parent.Children[API_ENTRYPOINT];
                }

                for (int i = 0; i < parts.Length; i++)
                {
                    ResourceNode child = null;

                    if (!parent.Children.TryGetValue(parts[i], out child))
                    {
                        child = new ResourceNode(parent, parts[i], classType);
                        parent.Children.Add(parts[i], child);
                    }
                    parent = child;
                }
            }
            else
            {
                // handle API entrypoint
                if (parent.Children.ContainsKey(API_ENTRYPOINT))
                {
                    throw new NotSupportedException("API entrypoint already exists");
                }
                ResourceNode apiEntryPoint = new ResourceNode(parent, API_ENTRYPOINT, classType);
                parent.Children.Add(API_ENTRYPOINT, apiEntryPoint);
                parent = apiEntryPoint;
            }
            
            return parent;
        }

        private static bool IsHTTPVerb(string text)
        {
            return HTTP_METHODS.Contains(text);
        }
    }
}

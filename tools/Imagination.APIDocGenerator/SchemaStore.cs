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

using Imagination.Documentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Imagination.Tools.APIDocGenerator
{
    public class SchemaStore
    {
        public Dictionary<Type, Schema> Schemas { get; private set; }

        public SchemaStore(ResourceNode resourceTree)
        {
            Schemas = new Dictionary<Type, Schema>();
            AddSchemas(resourceTree);
        }

        private void AddSchemas(ResourceNode node)
        {
            if (node.Method != null)
            {
                MethodDocumentationAttribute attribute = node.Method.GetCustomAttributes<MethodDocumentationAttribute>().FirstOrDefault();

                if (attribute != null)
                {
                    AddSchemas(attribute.RequestTypes);
                    AddSchemas(attribute.ResponseTypes);
                }
            }
            foreach (ResourceNode child in node.Children.Values)
            {
                AddSchemas(child);
            }
        }

        private void AddSchemas(Type[] types)
        {
            if (types != null)
            {
                foreach (Type type in types)
                {
                    if (!Schemas.ContainsKey(type))
                    {
                        Schemas.Add(type, new Schema(type));
                    }
                }
            }
        }
    }
}
 
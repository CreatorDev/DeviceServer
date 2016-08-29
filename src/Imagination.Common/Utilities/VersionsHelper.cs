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
using System.Reflection;
using System.Threading.Tasks;

namespace Imagination.Common
{
    public static class VersionsHelper
    {

        public static Tuple<string, string> GetCurrentAssemblyVersions(bool usePackageVersion = false)
        {
            return GetAssemblyVersions(Assembly.GetExecutingAssembly(), usePackageVersion);
        }

        public static List<Tuple<string, string>> GetAssemblyVersions(string namespacePrefixFilter = "Imagination.", bool usePackageVersion = false)
        {
            IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (namespacePrefixFilter != null)
                assemblies = assemblies.Where(a => a.GetName().Name.ToString().StartsWith(namespacePrefixFilter));
            assemblies = assemblies.OrderBy(a => a.GetName().Name);

            List<Tuple<string, string>> result = new List<Tuple<string, string>>(assemblies.Count());
            foreach (Assembly asm in assemblies)
            {
                result.Add(GetAssemblyVersions(asm, usePackageVersion));
            }
            return result;
        }

        private static Tuple<string, string> GetAssemblyVersions(Assembly asm, bool usePackageVersion)
        {
            AssemblyName asmName = asm.GetName();
            string name = asmName.Name.ToString();
            string version = null;

            if (usePackageVersion)
            {
                // usually a string matching the AssemblyFileVersion plus the prerelease suffix if packaged with NuGet
                var infoAttr = CustomAttributeExtensions.GetCustomAttribute<AssemblyInformationalVersionAttribute>(asm);
                if (infoAttr != null)
                    version = infoAttr.InformationalVersion;
            }
            if (version == null)
            {
                var attr = CustomAttributeExtensions.GetCustomAttribute<AssemblyFileVersionAttribute>(asm);
                if (attr != null)
                    version = attr.Version;
            }
            if (version == null)
                version = asmName.Version.ToString(); // Microsoft typically only sets the major version

            return new Tuple<string, string>(name, version);
        }
    }
}

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
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Imagination.Tools.APIDocGenerator
{
    public class Program
    {
        public static string WEBSERVICE_NAME = "Imagination.WebService.DeviceServer";

        public static void Main(string[] args)
        {
            if (args.Length == 0 || args.Any(a => a.Equals("-h") || a.Equals("--help")))
            {
                WriteHelp();
            }
            else
            {
                string path = null;

                for (int i = 0; i < args.Length - 1; i++)
                {
                    bool argument = true;
                    if (args[i].Equals("-p") || args[i].Equals("--path"))
                    {
                        path = args[i + 1];
                    }
                    else if (args[i].Equals("-r") || args[i].Equals("--ramlVersion"))
                    {
                        DocumentationSerialiserFactory.RAMLVersion = args[i + 1];
                    }
                    else
                    {
                        argument = false;
                    }
                    if (argument)
                    {
                        List<string> temp = args.ToList();
                        temp.RemoveRange(i, 2);
                        args = temp.ToArray();
                        i--;
                    }
                }

                
                if (path == null)
                {
                    path = FindAssemblyPath();
                }

                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Could not find Device Server Executable. ");
                    Console.Out.Flush();
                }
                else
                {
                    Assembly assembly = null;
                    try
                    {
                        assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path));
                    }
                    catch (Exception exception)
                    {
                        SerialisationLog.Error(string.Concat("Failed to load assembly at ", path, ": ", exception));
                    }

                    if (assembly != null)
                    {
                        ResourceNode resourceTree = null;
                        try
                        {
                            Console.WriteLine("Gathering resources from assembly...");
                            resourceTree = AssemblyReader.ReadAssembly(assembly);
                        }
                        catch (Exception exception)
                        {
                            SerialisationLog.Error(string.Concat("Failed to generate resource tree from assembly at ", path, ": ", exception));
                        }

                        if (resourceTree != null)
                        {
                            IConfigurationBuilder builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddEnvironmentVariables();
                            //.AddCommandLine(args);

                            IConfigurationRoot root = builder.Build();
                            DocumentationHeaderSettings headerSettings = new DocumentationHeaderSettings();
                            root.GetSection("HeaderSettings").Bind(headerSettings);

                            SchemaStore schemaStore = new SchemaStore(resourceTree);
                            ExampleStore exampleStore = new ExampleStore(string.Concat(GetRootDirectory(), "/doc/"), resourceTree);

                            args = args.Where(a => !a.StartsWith("-")).ToArray();  // remove options
                            int numSuccess = 0;
                            for (int i = 0; i < args.Length; i++)
                            {
                                try
                                {
                                    Console.WriteLine(string.Concat("Generating documentation file: ", args[i]));
                                    GenerateDocumentation(resourceTree, headerSettings, schemaStore, exampleStore, args[i]);
                                    numSuccess++;
                                }
                                catch(Exception exception)
                                {
                                    SerialisationLog.Error(string.Concat("Failed to generate documentation for ", args[i], ": ", exception));
                                }
                            }
                            
                            Console.WriteLine(string.Concat(numSuccess, "/", args.Length, " documentation files generated successfully."));
                            Console.WriteLine(string.Concat(SerialisationLog.Errors, " errors, ", SerialisationLog.Warnings, " warnings."));
                        }
                    }
                }
            }
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void GenerateDocumentation(ResourceNode resourceTree, DocumentationHeaderSettings headerSettings, SchemaStore schemaStore, ExampleStore exampleStore, string outputFilename)
        {
            IDocumentationSerialiser serialiser = DocumentationSerialiserFactory.GetSerialiser(outputFilename);
            using (FileStream stream = new FileStream(outputFilename, FileMode.Create, FileAccess.Write))
            {
                StreamWriter streamWriter = new StreamWriter(stream);
                serialiser.Serialise(streamWriter, resourceTree, headerSettings, schemaStore, exampleStore);
                streamWriter.Flush();
            }
        }

        private static void WriteHelp()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fileVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.WriteLine(string.Concat("DeviceServer APIDocGenerator\nVersion ", fileVersionInfo.FileVersion));
            Console.WriteLine("Usage: Imagination.APIDocGenerator [outputfilenames] [options]");
            Console.WriteLine();
            Console.WriteLine("Documentation output is determined by output filename:");
            foreach (KeyValuePair<string, IDocumentationSerialiser> pair in DocumentationSerialiserFactory.Serialisers)
            {
                Console.WriteLine(string.Concat("*.", pair.Key.Substring(0, 4), "\t\t", pair.Value.GetDescription()));
            }
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("-h|--help  \t\t\tPrint usage information");
            Console.WriteLine("-p|--path  \t\t\tManually supply path to assembly");
            Console.WriteLine("-r|--ramlVersion  \t\tSpecify RAML version. Default: ", DocumentationSerialiserFactory.RAMLVersion);
        }

        private static string FindAssemblyPath()
        {
            string path = null;
            string binDir = string.Concat(GetRootDirectory(), "/src/", WEBSERVICE_NAME, "/bin");
            string[] filenames = null;

            Console.WriteLine(string.Concat("Searching for DeviceServer assemblies in ", binDir, "..."));
            try
            {
                filenames = Directory.GetFiles(binDir, string.Concat("*", WEBSERVICE_NAME, ".exe"), SearchOption.AllDirectories);

            }
            catch (DirectoryNotFoundException)
            {
                SerialisationLog.Error("Could not find bin directory - has the Device Server been built?");
            }
            if (filenames != null)
            {
                List<FileInfo> files = filenames.Select(f => new FileInfo(f))
                    .Where(f => f.Directory.GetDirectories().Length == 0) // must be bottom level directory to include all dependencies
                    .OrderBy(f => f.LastWriteTime) // select the latest built configuration
                    .ToList();

                Console.WriteLine(string.Concat("Found ", files.Count, " assemblies:"));
                foreach (FileInfo fileInfo in files)
                {
                    Console.WriteLine(string.Concat("*", fileInfo.FullName.Substring(binDir.Length)));
                }
                if (files.Count > 0)
                {
                    path = files.Last().FullName;
                    Console.WriteLine("Latest assembly: " + path.Substring(binDir.Length));
                }
            }

            return path;
        }

        private static string GetRootDirectory()
        {
            return new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
        }
    }
}

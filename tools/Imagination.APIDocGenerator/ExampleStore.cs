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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Imagination.Tools.APIDocGenerator
{
    public class ExampleStore
    {
        private static readonly Regex EXAMPLE_NAME_REGEX = new Regex(@"\[\]: \[[a-zA-Z]+\.[a-zA-Z]+\.[a-zA-Z]+\]", RegexOptions.IgnoreCase);
        private const string FORM_EXAMPLE = "form";
        private Dictionary<string, Example> _Examples;

        public ExampleStore(string baseDirectory, ResourceNode resourceTree)
        {
            _Examples = new Dictionary<string, Example>();

            string[] filenames = Directory.GetFiles(baseDirectory, string.Concat("*.md"), SearchOption.AllDirectories);
            foreach (string filename in filenames)
            {
                ReadExamples(baseDirectory, filename, resourceTree);
            }

            Console.WriteLine(string.Concat("Read ", _Examples.Count, " examples from ", baseDirectory, "."));
        }

        public void ReadExamples(string baseDirectory, string filename, ResourceNode resourceTree)
        {
            Example currentExample = null;
            StringBuilder currentExampleBody = null;
            TDataExchangeFormat currentExampleFormat = TDataExchangeFormat.None;
            bool inExample = false;
            string lastHeading = null;
            
            foreach (string line in File.ReadLines(filename))
            {
                string trimmed = line.Trim();
                if (trimmed.StartsWith("#"))
                {
                    lastHeading = trimmed.Replace("#", "").Trim().Replace(" ", "-").ToLower();
                }
                if (trimmed.StartsWith("```"))
                {
                    if (currentExample != null)
                    {
                        string exampleFormat = trimmed.Substring("```".Length);
                        if (exampleFormat.Equals(TDataExchangeFormat.Json.ToString().ToLower()))
                        {
                            currentExampleFormat = TDataExchangeFormat.Json;
                            inExample = true;
                        }
                        else if (exampleFormat.Equals(FORM_EXAMPLE))
                        {
                            currentExampleFormat = TDataExchangeFormat.FormUrlEncoded;
                            inExample = true;
                        }
                        else if (inExample && exampleFormat.Length == 0)
                        {
                            if (currentExampleFormat == TDataExchangeFormat.FormUrlEncoded)
                            {
                                currentExample.Content.Add(currentExampleFormat, currentExampleBody.ToString());
                            }
                            else
                            {
                                currentExample.Content.Add(TDataExchangeFormat.Json, currentExampleBody.ToString());
                                currentExample.Content.Add(TDataExchangeFormat.Xml, JsonToXml(currentExample, resourceTree, currentExampleBody.ToString()));
                            }
                            currentExample = null;
                            inExample = false;
                        }
                    }
                }
                else
                {
                    if (inExample)
                    {
                        if (line.Trim().Length > 0)
                        {
                            currentExampleBody.Append(line);// string.Concat(line, "\r\n"));
                        }
                    }
                    else
                    {
                        MatchCollection matches = EXAMPLE_NAME_REGEX.Matches(trimmed);
                        if (matches.Count == 1)
                        {
                            string fullExampleName = matches[0].Value;
                            string prefix = "[]: [";
                            string suffix = "]";
                            string withoutSquareBrackets = fullExampleName.Substring(prefix.Length, matches[0].Value.Length - prefix.Length - suffix.Length);
                            string[] parts = withoutSquareBrackets.Split(new []{ '.' }, StringSplitOptions.RemoveEmptyEntries);
                            currentExample = new Example(filename.Substring(baseDirectory.Length), lastHeading, parts[0], parts[1], (TMessageType)Enum.Parse(typeof(TMessageType), parts[2]));
                            currentExampleBody = new StringBuilder();

                            string key = string.Concat(currentExample.ClassName, currentExample.MethodName, currentExample.ExampleType.ToString());
                            if (!_Examples.ContainsKey(key))
                            {
                                _Examples.Add(key, currentExample);
                            }
                            else
                            {
                                SerialisationLog.Error(string.Concat("An example already exists for ", currentExample.ClassName, ".", currentExample.MethodName, ".", currentExample.ExampleType.ToString()));
                            }
                        }
                    }
                }
            }
        }

        private string JsonToXml(Example currentExample, ResourceNode resourceTree, string json)
        {
            string xml = null;
            MethodInfo method = resourceTree.Find(currentExample.ClassName, currentExample.MethodName).Method;

            MethodDocumentationAttribute attribute = method.GetCustomAttributes<MethodDocumentationAttribute>().FirstOrDefault();

            if (attribute != null)
            {
                Type objectType = null;

                if (currentExample.ExampleType == TMessageType.Request)
                {
                    objectType = GetObjectType(attribute.RequestTypes);
                }
                else
                {
                    objectType = GetObjectType(attribute.ResponseTypes);
                }

                if (objectType != null)
                {
                    try
                    {
                        object deserialisedObject = JsonConvert.DeserializeObject(json, objectType);

                        Stream stream = new MemoryStream();
                        XmlSerializer serializer = new XmlSerializer(objectType);
                        XmlSerializerNamespaces xns = new XmlSerializerNamespaces();
                        xns.Add(string.Empty, string.Empty);
                        serializer.Serialize(stream, deserialisedObject, xns);

                        stream.Position = 0;
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            reader.ReadLine();  // skip <?xml version="1.0"?>
                            xml = reader.ReadToEnd().Replace("+json", "+xml");
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        SerialisationLog.Warning(string.Concat("Failed to serialise XML example for ", currentExample.ClassName, ".", currentExample.MethodName));
                    }
                    catch (Exception ex)
                    {
                        if (ex is JsonReaderException || ex is JsonSerializationException)
                        {
                            SerialisationLog.Warning(string.Concat("Failed to deserialise JSON example for ", currentExample.ClassName, ".", currentExample.MethodName));
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
                else
                {
                    SerialisationLog.Warning(string.Concat("No ", currentExample.ExampleType.ToString().ToLower(), " type for ", currentExample.ClassName, ".", currentExample.MethodName));
                }
            }
            else
            {
                SerialisationLog.Warning(string.Concat("Could not convert JSON example to XML due to no method documentation for ", currentExample.ClassName, ".", currentExample.MethodName));
            }
            return xml;
        }

        private Type GetObjectType(Type[] types)
        {
            Type matchingType = null;
            if (types != null)
            {
                if (types.Length == 1)
                {
                    matchingType = types[0];
                }
                else
                {
                    throw new NotSupportedException("There are multiple request or response types for this method!");
                }
            }
            return matchingType;
        }

        public Example GetExample(Type classType, MethodInfo methodInfo, TMessageType exampleType)
        {
            Example example = null;
            string key = string.Concat(classType.Name, methodInfo.Name, exampleType);
            _Examples.TryGetValue(key, out example);
            return example;
        }

        public Example GetExample(Type classType, MethodInfo method)
        {
            Example example = GetExample(classType, method, TMessageType.Request);
            if (example == null)
            {
                example = GetExample(classType, method, TMessageType.Response);
            }
            return example;
        }

        public string GetExampleContent(Type classType, MethodInfo methodInfo, TMessageType exampleType, TDataExchangeFormat dataExchangeFormat)
        {
            string content = null;
            Example example = GetExample(classType, methodInfo, exampleType);
            if (example != null)
            {
                example.Content.TryGetValue(dataExchangeFormat, out content);
            }
            return content;
        }
    }
}

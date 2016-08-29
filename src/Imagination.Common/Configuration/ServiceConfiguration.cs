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
using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;

namespace Imagination
{
    public class ServiceConfiguration
    {
        //private static bool _ReadOnlyService = false;
        private static Uri _ExternalUri;
        private static string _Hostname;
        private static string _Name;
        private static string _TempFolder;
        private static string _SigningKey;

        private static MongoUrl _MongoConnection;
        private static List<RabbitMQConnection> _RabbitMQConnections = new List<RabbitMQConnection>();
        private static List<Uri> _ChangeNotificationServers = new List<Uri>();


        public static string Hostname
        {
            get { return _Hostname; }
        }


        public static Uri ExternalUri
        {
            get { return _ExternalUri; }
            set { _ExternalUri = value; }
        }

        public static string Name
        {
            get { return _Name; }
        }

        public static MongoUrl MongoConnection
        {
            get { return _MongoConnection; }
        }

        public static List<RabbitMQConnection> RabbitMQConnections
        {
            get { return _RabbitMQConnections; }
        }

        public static List<Uri> ChangeNotificationServers
        {
            get { return _ChangeNotificationServers; }
        }


        public static string TempFolder
        {
            get { return _TempFolder; }
        }

        public static string SigningKey
        {
            get { return _SigningKey; }
        }

        public static ILoggerFactory LoggerFactory { get; set; }

        static ServiceConfiguration()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                _TempFolder = "/var/img";
            else
                _TempFolder = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
            string assemblyName = null;
            if (AppDomain.CurrentDomain != null)
            {
                string assemblyFile = AppDomain.CurrentDomain.FriendlyName;
                int index = assemblyFile.IndexOf(':');
                if (index == -1)
                    assemblyName = assemblyFile;
                else
                    assemblyName = assemblyFile.Substring(0, index);
                index = assemblyName.LastIndexOf('/');
                if (index != -1)
                    assemblyName = assemblyName.Substring(index + 1);
                index = assemblyName.IndexOf('-');
                if (index != -1)
                    assemblyName = assemblyName.Substring(0, index);
                if (string.Compare(assemblyName, "root", true) == 0)
                {
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    if (baseDirectory.EndsWith("\\") || baseDirectory.EndsWith("/"))
                        baseDirectory = baseDirectory.Substring(0, baseDirectory.Length - 1);
                    assemblyName = Path.GetFileName(baseDirectory);
                }
            }
            if (assemblyName == null)
            {
                string assemblyFile = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
                int index = assemblyFile.LastIndexOf('.');
                if (index == -1)
                    assemblyName = assemblyFile;
                else
                    assemblyName = assemblyFile.Substring(index + 1);
            }
            _Name = assemblyName;
            _Hostname = Environment.MachineName;
        }

        public static void DisplayConfig()
        {
            if (_Hostname != null)
            {
                Console.Write("Hostname: ");
                Console.WriteLine(_Hostname);
            }
            if (_ExternalUri != null)
            {
                Console.Write("ExternalUri: ");
                Console.WriteLine(_ExternalUri);
            }
            if (_MongoConnection != null)
            {
                Console.Write("MongoConnection: ");
                Console.WriteLine(_MongoConnection.ToString());
            }
            if (_ChangeNotificationServers.Count > 0)
            {
                Console.WriteLine("ChangeNotificationServers: ");
                foreach (Uri item in _ChangeNotificationServers)
                {
                    Console.Write("  ");
                    Console.WriteLine(item.ToString());
                }
            }
            if (_RabbitMQConnections.Count > 0)
            {
                Console.WriteLine("RabbitMQConnections: ");
                foreach (RabbitMQConnection item in _RabbitMQConnections)
                {
                    Console.Write("  ");
                    Console.WriteLine(item.Uri.ToString());
                }
            }
        }

        public static void LoadConfig(IConfigurationSection configurationSection)
        {
            foreach (IConfigurationSection item in configurationSection.GetChildren())
            {
                if (string.Compare(item.Key, "Hostname", true) == 0)
                {
                    string hostname = item.Value;
                    if (!string.IsNullOrEmpty(hostname))
                       _Hostname = hostname;

                }
                if (string.Compare(item.Key, "ExternalUri", true) == 0)
                {
                    Uri uri;
                    if (Uri.TryCreate(item.Value, UriKind.Absolute, out uri))
                        _ExternalUri = uri;
                    else
                        throw new ArgumentException($"ExternalUri could not be parsed as Uri from {item.Value}");
                }
                if (string.Compare(item.Key, "ChangeNotificationServers", true) == 0)
                {
                    LoadUris(_ChangeNotificationServers, item);
                }
                else if (string.Compare(item.Key, "MongoConnection", true) == 0)
                {
                    _MongoConnection = new MongoUrl(item.Value);
                }
                else if (string.Compare(item.Key, "RabbitMQConnections", true) == 0)
                {
                    LoadRabbitMQConnections(_RabbitMQConnections, item);
                }
                else if (string.Compare(item.Key, "SigningKey", true) == 0)
                {
                    string signingKey = item.Value;
                    if (!string.IsNullOrEmpty(signingKey))
                        _SigningKey = signingKey;
                }
            }
        }

        private static RabbitMQConnection LoadRabbitMQConnection(IConfigurationSection section)
        {
            RabbitMQConnection result = new RabbitMQConnection();
            foreach (IConfigurationSection item in section.GetChildren())
            {
                if (string.Compare(item.Key, "Uri", true) == 0)
                {
                    result.Uri = new Uri(item.Value);
                }
                else if (string.Compare(item.Key, "Username", true) == 0)
                {
                    result.Username = item.Value;
                }
                else if (string.Compare(item.Key, "Password", true) == 0)
                {
                    result.Password = item.Value;
                }
            }
            return result;
        }

        private static void LoadRabbitMQConnections(List<RabbitMQConnection> connections, IConfigurationSection section)
        {
            foreach (IConfigurationSection item in section.GetChildren())
            {
                connections.Add(LoadRabbitMQConnection(item));
            }
        }

        private static void LoadUris(List<Uri> uris, IConfigurationSection section)
        {
            foreach (IConfigurationSection item in section.GetChildren())
            {
                uris.Add(new Uri(item.Value));
            }
        }
    }
}

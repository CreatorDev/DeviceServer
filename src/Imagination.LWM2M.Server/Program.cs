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

using Imagination.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Imagination.LWM2M
{
    public class Program
    {
        private static ManualResetEvent _ShutdownEvent;

        public static void Main(string[] args)
        {
            try
            {               
                int workerThreads;
                int completionPortThreads;
                System.Threading.ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
                if (workerThreads < 16)
                {
                    workerThreads = 16;
                    System.Threading.ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
                }

                IConfigurationBuilder builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);
                IConfigurationRoot configuration = builder.Build();
                ServiceConfiguration.LoadConfig(configuration.GetSection("ServiceConfiguration"));


                int port = 5683;
                bool secureOnly = true;
                IConfigurationSection sectionServer = configuration.GetSection("LWM2MServer");
                if (sectionServer != null)
                {
                    IConfigurationSection sectionPort = sectionServer.GetSection("Port");
                    if (sectionPort != null)
                    {
                        if (!int.TryParse(sectionPort.Value, out port))
                            port = 5683;
                    }
                    IConfigurationSection sectionSecure = sectionServer.GetSection("SecureOnly");
                    if (sectionSecure != null)
                    {
                        if (!bool.TryParse(sectionSecure.Value, out secureOnly))
                            secureOnly = true;
                    }
                }

                if (ServiceConfiguration.ExternalUri == null)
                {
                    ServiceConfiguration.ExternalUri = new Uri(string.Concat("coaps://", ServiceConfiguration.Hostname, ":", (port + 1).ToString()));
                }

                Version version = Assembly.GetExecutingAssembly().GetName().Version;                      
                Console.Write("LWM2M server (");
                Console.Write(version.ToString());
                Console.WriteLine(")");
                
                ServiceConfiguration.DisplayConfig();

                ApplicationEventLog.LogLevel = System.Diagnostics.EventLogEntryType.Information;

                Server server = new Server();
                //server.PSKIdentities.LoadFromFile("PSKIdentities.xml");
                server.Port = port;
                server.SecureOnly = secureOnly;
                server.Start();
                _ShutdownEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    _ShutdownEvent.Set();
                    e.Cancel = true;
                };
                Console.Write("Listening on port ");
                Console.WriteLine(port.ToString());
                Console.WriteLine("Press Ctrl+C to stop the server.");
                _ShutdownEvent.WaitOne();
                Console.WriteLine("Exiting.");
                server.Stop();
                BusinessLogicFactory.Clients.Stop();
                BusinessLogicFactory.ServiceMessages.Stop();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}

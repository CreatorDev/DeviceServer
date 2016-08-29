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

using Imagination.BusinessLogic;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Imagination.Service.ChangeNotification
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


                int port = 14050;
                IConfigurationSection section = configuration.GetSection("ChangeNotification");
                if (section != null)
                {
                    section = section.GetSection("NotificationTCPPort");
                    if (section != null)
                    {
                        if (!int.TryParse(section.Value, out port))
                            port = 14050;
                    }
                }

                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.Write("ChangeNotification (");
                Console.Write(version.ToString());
                Console.WriteLine(")");

                ServiceConfiguration.DisplayConfig();

                ApplicationEventLog.LogLevel = System.Diagnostics.EventLogEntryType.Information;
                BusinessLogicFactory.NotificationOrchestrator.Start();
                NotificationTcpServer server = new NotificationTcpServer();
                server.StartListening(port);
                _ShutdownEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    _ShutdownEvent.Set();
                    e.Cancel = true;
                };
                Console.WriteLine("Press Ctrl+C to stop the server.");
                _ShutdownEvent.WaitOne();
                Console.WriteLine("Exiting.");
                server.StopListening();
                BusinessLogicFactory.NotificationOrchestrator.Stop();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

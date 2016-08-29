using Imagination.BusinessLogic;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Imagination.Model;

namespace Imagination.Service.Subscription
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

                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.Write("Subscription (");
                Console.Write(version.ToString());
                Console.WriteLine(")");

                ServiceConfiguration.DisplayConfig();

                BusinessLogicFactory.Subscriptions.StartListening();

                

                ApplicationEventLog.LogLevel = System.Diagnostics.EventLogEntryType.Information;

                _ShutdownEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
                {
                    _ShutdownEvent.Set();
                    e.Cancel = true;
                };
                Console.WriteLine("Press Ctrl+C to stop the server.");
                _ShutdownEvent.WaitOne();
                Console.WriteLine("Exiting.");

                BusinessLogicFactory.ServiceMessages.Stop();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}

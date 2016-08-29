using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Imagination.WebService.DeviceServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            IServerAddressesFeature serverAddressesFeature = host.ServerFeatures.Get<IServerAddressesFeature>();
            if (serverAddressesFeature != null)
            {
                serverAddressesFeature.Addresses.Clear();
                serverAddressesFeature.Addresses.Add("http://*:8080");
            }

            host.Run();
            BusinessLogic.BusinessLogicFactory.ServiceMessages.Stop();
            Console.WriteLine("Exiting.");
        }
    }
}

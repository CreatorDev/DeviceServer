using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using TestWebApplication;
using System.Net.Http;

namespace DeviceServerTests.Fixtures
{
    public class TestWebAppFixture : IDisposable
    {
        public TestServer TestServer { get; private set; }
        public HttpClient Client { get; private set; }

        public TestWebAppFixture()
        {
            //WebHostBuilder builder = TestServer.CreateBuilder();
            WebHostBuilder builder = new WebHostBuilder();
            builder.UseStartup<Startup>();
            builder.UseUrls("http://localhost:56789");

            TestServer = new TestServer(builder);

            Client = TestServer.CreateClient();
        }

        public void Dispose()
        {
            TestServer.Dispose();
            Client.Dispose();
        }
    }
}

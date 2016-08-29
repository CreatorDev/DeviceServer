using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeviceServerTests.Utilities
{
    public class PSK
    {
        public string Key { get; set; }
        public string Secret { get; set; }
    }

    public class Certificate
    {
        //public string Filename { get; set; }
    }

    public class Identity
    {
        public PSK PSK { get; set; }
        public Certificate Certificate { get; set; }
    }

    public class Authentication
    {
        public string MasterKey { get; set; }
        public string MasterSecret { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
    }

    public class RestAPI
    {
        public string URI { get; set; }
        public string ContentType { get; set; }

        public Authentication Authentication { get; set; }
    }

    public class LWM2MClient
    {
        public string URI { get; set; }
    }

    public class TestData
    {
        public RestAPI RestAPI { get; set; }

        public LWM2MClient LWM2MClient { get; set; }

        public Identity Identity { get; set; }
    };

    public static class TestConfiguration
    {
        private static readonly object _Lock = new object();
        private static TestData _TestData;

        public static TestData TestData
        {
            get
            {
                lock (_Lock)
                {
                    if (_TestData == null)
                    {
                        var builder = new ConfigurationBuilder()
                            .AddJsonFile("testconfig.json", optional: false)
                            .AddEnvironmentVariables();

                        IConfigurationRoot root = builder.Build();
                        _TestData = new TestData();
                        root.GetSection("DeviceServerTests").Bind(_TestData);
                    }
                }
                return _TestData;
            }
            private set {}
        }
    }
}

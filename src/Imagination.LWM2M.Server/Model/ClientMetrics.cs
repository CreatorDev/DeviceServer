using Imagination.LWM2M;
using Imagination.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Imagination.Model
{
    internal class ClientMetrics
    {
        public ClientMetric BytesSent { get; set; }

        public ClientMetric BytesReceived { get; set; }

        public ClientMetric TransactionCount { get; set; }

        private List<ClientMetric> _Metrics;

        public ClientMetrics()
        {
            _Metrics = new List<ClientMetric>();

            BytesSent = new ClientMetric();
            BytesSent.Name = MetricNames.BytesSent;
            BytesSent.Incremental = true;
            _Metrics.Add(BytesSent);

            BytesReceived = new ClientMetric();
            BytesReceived.Name = MetricNames.BytesReceived;
            BytesReceived.Incremental = true;
            _Metrics.Add(BytesReceived);

            TransactionCount = new ClientMetric();
            TransactionCount.Name = MetricNames.TransactionCount;
            TransactionCount.Incremental = true;
            _Metrics.Add(TransactionCount);
        }

        public void FillParameters(ServiceEventMessage message)
        {
            Dictionary<string, long> metrics = new Dictionary<string, long>();
            message.Parameters.Add("Metrics", _Metrics);
        }

        public void ResetIncrementalMetrics()
        {
            foreach (ClientMetric metric in _Metrics)
            {
                if (metric.Incremental)
                {
                    metric.Value = 0;
                }
            }
        }
    }
}

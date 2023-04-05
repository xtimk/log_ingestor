using System.Diagnostics.Metrics;

namespace FSWriter.Services.MetricsService.Impl
{
    public class MetricsService : IMetricsService
    {
        private readonly ILogger<MetricsService> _logger;

        private Meter _meter;
        private readonly string _meterName = "app";
        private Dictionary<string, Counter<int>> _trackedGuids;

        public MetricsService(ILogger<MetricsService> logger)
        {
            _logger = logger;
            _meter = new Meter(_meterName);
            _trackedGuids = new Dictionary<string, Counter<int>>();
        }

        public void SignalNewEvent(string guid, string name, int number_of_events=1)
        {
            if(!_trackedGuids.ContainsKey(guid))
            {
                _trackedGuids.Add(guid, _meter.CreateCounter<int>($"{name}"));
            }
            var counter = _trackedGuids[guid];
            counter.Add(number_of_events);
        }

        public void GetMetrics()
        {
        }
    }
}

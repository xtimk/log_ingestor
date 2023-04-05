using Agent.Configurations;
using Agent.Models;
using Agent.Services.GuidProvider;
using Agent.Services.MessageService;
using Agent.Services.MetricsService;
using Agent.Services.Readers.Factory.Impl;
using Agent.Services.Readers.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Agent.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class AgentApiController : Controller
    {
        private readonly ILogger<AgentApiController> _logger;
        private readonly IMessageProducer<BaseLogMessage> _messageProducer;
        private readonly IOptions<LogIngestorServer> _logIngestorServer;
        private readonly Dictionary<Guid, IReader> _activeReaders;
        private readonly IGuidProvider _guidProvider;
        private readonly IMetricsService _metricsService;
        private readonly IServiceProvider _serviceProvider;

        public AgentApiController(
                ILogger<AgentApiController> logger, 
                IMessageProducer<BaseLogMessage> messageProducer,
                IOptions<LogIngestorServer> logIngestorServer,
                Dictionary<Guid, IReader> activeReaders,
                IGuidProvider guidProvider,
                IMetricsService metricsService,
                IServiceProvider serviceProvider)
        {
            _logger = logger;
            _messageProducer = messageProducer;
            _logIngestorServer = logIngestorServer;
            _activeReaders = activeReaders;
            _guidProvider = guidProvider;
            _metricsService = metricsService;
            _serviceProvider = serviceProvider;

            _messageProducer.OnPublish += WriteMetrics;
        }

        [HttpGet]
        public IActionResult StartFakeReader()
        {
            _logger.LogInformation("Required start of fake log reader");
            var fakeCreator = new FakeMessageGeneratorCreator(_serviceProvider);
            var fakeReader = fakeCreator.Create();

            fakeReader.OnNewLines += HandleNewLines;
            var threadGuid = _guidProvider.Create();
            Task.Run(() => fakeReader.Start(threadGuid));
            _activeReaders.Add(threadGuid, fakeReader);
            return Ok($"Started reader. Guid: {threadGuid}");
        }

        [HttpGet]
        public IActionResult StopReader(string guid)
        {
            _logger.LogInformation("Required stop of reader");
            var guidToSearch = _guidProvider.Create(guid);
            if (!_activeReaders.TryGetValue(guidToSearch, out IReader? reader))
            {
                return BadRequest($"Guid {guidToSearch} not found.");
            }
            reader.Stop();
            _activeReaders.Remove(guidToSearch);
            return Ok($"Requested stop for reader {guidToSearch}");
        }

        private void HandleNewLines(object? o, List<BaseLogMessage> lines)
        {
            foreach (var item in lines)
            {
                _messageProducer.Publish(_logIngestorServer.Value.Topic, item);
            }
        }

        private void WriteMetrics(string name)
        {
            //_metricsService.SignalNewEvent("fake-guid", "fake-reader");
            //_metricsService.SignalNewEvent("a-guid", "agent.out");
        }
    }
}

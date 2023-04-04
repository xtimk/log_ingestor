using Agent.Configurations;
using Agent.Models;
using Agent.Services.MessageService;
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
        private readonly IMessageProducer<string> _messageProducerString;
        private readonly IOptions<LogIngestorServer> _logIngestorServer;
        private readonly Dictionary<Guid, IReader> _activeReaders;
        private readonly IServiceProvider _serviceProvider;

        public AgentApiController(
                ILogger<AgentApiController> logger, 
                IMessageProducer<BaseLogMessage> messageProducer,
                IMessageProducer<string> messageProducerString,
                IOptions<LogIngestorServer> logIngestorServer,
                Dictionary<Guid, IReader> activeReaders,
                IServiceProvider serviceProvider)
        {
            _logger = logger;
            _messageProducer = messageProducer;
            _messageProducerString = messageProducerString;
            _logIngestorServer = logIngestorServer;
            _activeReaders = activeReaders;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public IActionResult StartFakeReader()
        {
            var fakeCreator = new FakeMessageGeneratorCreator(_serviceProvider);
            var fakeReader = fakeCreator.Create();

            fakeReader.OnNewLines += HandleNewLines;
            var threadGuid = Guid.NewGuid();
            Task.Run(() => fakeReader.Start(threadGuid));
            _activeReaders.Add(threadGuid, fakeReader);
            return Ok($"Started reader. Guid: {threadGuid}");
        }

        [HttpGet]
        public IActionResult StopReader(string guid)
        {
            var guidToSearch = new Guid(guid);
            if (!_activeReaders.TryGetValue(guidToSearch, out IReader? fakeReader))
            {
                return BadRequest($"Guid {guidToSearch} not found.");
            }
            fakeReader.Stop();
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
    }
}

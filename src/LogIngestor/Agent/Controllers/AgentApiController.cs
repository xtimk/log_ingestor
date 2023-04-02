using Agent.Configurations;
using Agent.Models;
using Agent.Services.MessageService;
using Agent.Services.Readers.Factory.Impl;
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

        public AgentApiController(
                ILogger<AgentApiController> logger, 
                IMessageProducer<BaseLogMessage> messageProducer,
                IOptions<LogIngestorServer> logIngestorServer)
        {
            _logger = logger;
            _messageProducer = messageProducer;
            _logIngestorServer = logIngestorServer;
        }

        [HttpGet]
        public IActionResult StartFakeReader()
        {
            var fakeCreator = new FakeMessageGeneratorCreator();
            var fakeReader = fakeCreator.Create();
            fakeReader.OnNewLines += HandleNewLines;
            Task.Run(() => fakeReader.Start());
            //fakeReader.Start();
            return Ok("Ok");
        }

        private void HandleNewLines(object? o, List<BaseLogMessage> lines) {
            //var firstLine = JsonSerializer.Serialize(lines.First());
            //_logger.LogInformation($"Received {lines.Count} lines. First message is: {firstLine}");
            foreach (var item in lines)
            {
                _messageProducer.WriteToQueue(_logIngestorServer.Value.Topic, item);
            }
        }
    }
}

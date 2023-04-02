using BaseEnricher.Configurations;
using BaseEnricher.Constants;
using BaseEnricher.Models;
using BaseEnricher.Services.MessageBackgroundProcessor;
using BaseEnricher.Services.MessageService;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BaseEnricher.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MessageQueueController : ControllerBase
    {
        private readonly ILogger<MessageQueueController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MessageQueueController(ILogger<MessageQueueController> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        // Method just to test workflow: it adds a simulated message in the expected in_queue.
        [HttpGet]
        public IActionResult Send(string message)
        {
            _logger.LogInformation("API: Requested send of message");
            var messageProducer = _serviceProvider.GetRequiredService<IMessageProducer<BaseLogMessage>>();            

            var logMessage = new BaseLogMessage()
            {
                Message = message,
                AgentMetaData = new AgentMetaData()
                {
                    AgentAcquireDate = DateTime.UtcNow,
                    AgentName = "Windows_x64",
                    AgentVersion = new Version(2, 1, 32),
                    AgentHostName = "myclient.test.local"
                }
            };

            var brokerProducerConfig = _serviceProvider.GetRequiredService<IMessageBrokerSingletonConfiguration<RabbitMQProducerConfiguration>>();
            messageProducer.Configure(brokerProducerConfig.Hostname);
            messageProducer.WriteToQueue(QueueNames.QUEUE_BASE_MESSAGE_READ, logMessage);
            return Ok(message);
        }

        [HttpGet]
        public IActionResult SendXMessages(string message, int number_to_send)
        {
            _logger.LogInformation($"API: Requested send of {number_to_send} messages");
            var messageProducer = _serviceProvider.GetRequiredService<IMessageProducer<BaseLogMessage>>();

            var logMessage = new BaseLogMessage()
            {
                Message = message,
                AgentMetaData = new AgentMetaData()
                {
                    AgentAcquireDate = DateTime.UtcNow,
                    AgentName = "Windows_x64",
                    AgentVersion = new Version(2, 1, 32),
                    AgentHostName = "myclient.test.local"
                }
            };

            var brokerProducerConfig = _serviceProvider.GetRequiredService<IMessageBrokerSingletonConfiguration<RabbitMQProducerConfiguration>>();
            messageProducer.Configure(brokerProducerConfig.Hostname);
            for (int i = 0; i < number_to_send; i++)
            {
                messageProducer.WriteToQueue(QueueNames.QUEUE_BASE_MESSAGE_READ, logMessage);
            }
            return Ok(message);
        }

        // Get some stats
        [HttpGet]
        public IActionResult GetNumberOfEventsProcessedByConsumer()
        {
            var messageConsumer = _serviceProvider.GetRequiredService<IMessageProcessorBackground>();
            var n = messageConsumer.NumberOfEventsReaded();
            return Ok(n);
        }

        [HttpGet]
        public IActionResult SubscribeToChannel()
        {
            var hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_OUT_HOSTNAME);
            if (hostname == null)
            {
                _logger.LogError("API: can't retrieve hostname of queue broker from env variable");
                return BadRequest("API: can't retrieve hostname of queue broker from env variable");
            }

            var messageConsumer2 = _serviceProvider.GetRequiredService<IMessageConsumer<BaseLogMessage>>();
            messageConsumer2.Configure(hostname);

            messageConsumer2.Subscribe(QueueNames.QUEUE_BASE_MESSAGE_READ);
            messageConsumer2.OnMessageReceived += Execute;

            return Ok($"API: Started subscription to topic {QueueNames.QUEUE_BASE_MESSAGE_READ}");
        }
        private void Execute(object? obj, BaseLogMessage message)
        {
            var serializedMsg = JsonSerializer.Serialize(message);
            _logger.LogInformation($"API: Queue consumer has received a new message: {serializedMsg}");
        }
    }
}

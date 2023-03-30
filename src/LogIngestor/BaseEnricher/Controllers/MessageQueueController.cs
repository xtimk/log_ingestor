using BaseEnricher.Constants;
using BaseEnricher.Models;
using BaseEnricher.Services.MessageBackgroundProcessor;
using BaseEnricher.Services.MessageService;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client.Events;
using System.Text;

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
            _logger.LogInformation("Requested send of message");
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
            messageProducer.Configure(Environment.GetEnvironmentVariable(ConfigurationKeyConstants.ENV_RABBITMQ_IN_HOSTNAME));
            messageProducer.WriteToQueue(QueueNames.QUEUE_BASE_MESSAGE_READ, logMessage);
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
        public async Task<IActionResult> SubscribeToChannel()
        {
            var messageConsumer2 = _serviceProvider.GetRequiredService<IMessageConsumer>();
            messageConsumer2.Configure(Environment.GetEnvironmentVariable(ConfigurationKeyConstants.ENV_RABBITMQ_OUT_HOSTNAME));

            //messageConsumer2.StartAsync(new CancellationToken());
            await messageConsumer2.SubscribeAsync(QueueNames.QUEUE_BASE_MESSAGE_READ);
            messageConsumer2.OnMessageReceived += Execute;
            return Ok($"started subscription to topic {QueueNames.QUEUE_BASE_MESSAGE_READ}");
        }

        private async Task Execute(object obj, BasicDeliverEventArgs delivery)
        {
            var body = delivery.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"CONTROLLER: Queue consumer has received a new message: {message}");

            await Task.CompletedTask;
        }
    }
}

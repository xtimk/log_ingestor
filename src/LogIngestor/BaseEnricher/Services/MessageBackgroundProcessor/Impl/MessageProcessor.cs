using BaseEnricher.Constants;
using BaseEnricher.Models;
using BaseEnricher.Services.MessageProcessor;
using BaseEnricher.Services.MessageProcessor.Commands;
using BaseEnricher.Services.MessageProcessor.Commands.Impl;
using BaseEnricher.Services.MessageProcessor.Impl;
using BaseEnricher.Services.MessageService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace BaseEnricher.Services.MessageBackgroundProcessor.Impl
{
    public class MessageProcessor : BackgroundService, IMessageProcessorBackground
    {
        private readonly ILogger<MessageProcessor> _logger;
        //private readonly IMessageProcessor<BaseLogMessage> _messageProcessor;
        //private readonly IMessageConsumer _messageConsumer;
        private readonly IServiceProvider _serviceProvider;
        private string _hostname;
        private string _topic;

        private int _readedEvents;
        private Guid _consumer_guid;
        private string _baseLogMessage;

        public MessageProcessor(ILogger<MessageProcessor> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            //_messageProcessor = messageProcessor;
            //_messageConsumer = messageConsumer2;
            _serviceProvider = serviceProvider;
            _readedEvents = 0;
            _consumer_guid = Guid.NewGuid();
            _baseLogMessage = $"Message Processor[{_consumer_guid}]: ";
            _logger.LogInformation($"{_baseLogMessage}Message processor created. Unique id: {_consumer_guid}");
        }

        public void Configure(string host, string topic)
        {
            _hostname = host;
            _topic = topic;
            _logger.LogInformation($"{_baseLogMessage}Configured. Remote queue host: {_hostname}. Queue channel to be consumed: {_topic}");
        }

        public int NumberOfEventsReaded()
        {
            return _readedEvents;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{_baseLogMessage}Started.");
            return base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _logger.LogInformation($"{_baseLogMessage}Stopped.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            using IServiceScope scope = _serviceProvider.CreateScope();

            var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer>();
            // configure the consumer host.
            messageConsumer.Configure(_hostname);

            await messageConsumer.SubscribeAsync(_topic);

            messageConsumer.OnMessageReceived += ExecuteAction;

            await Task.CompletedTask;
        }

        // abstract basicdelivereventargs here.. I should not be aware that here im using rabbitmq.
        private async Task ExecuteAction(object obj, BasicDeliverEventArgs delivery)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            //var messageProcessor = scope.ServiceProvider.GetRequiredService<IMessageProcessor<T>>();
            var messageProducer = scope.ServiceProvider.GetRequiredService<IMessageProducer<EnrichedLogMessage>>();


            var body = delivery.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"{_baseLogMessage}Received a new message: {message}");
            try
            {
                var deserializedMessage = JsonSerializer.Deserialize<BaseLogMessage>(message);
                _logger.LogDebug($"{_baseLogMessage}Deserialized message: {deserializedMessage.ToString()}");

                var addDateCommand = new MessageProcessor<EnrichedLogMessage, BaseLogMessage>(new AddDateProcessCommand());
                _logger.LogDebug($"{_baseLogMessage}Add date to message: {message}");
                var enrichedMessage = addDateCommand.Execute(deserializedMessage);

                messageProducer.Configure(_hostname);
                messageProducer.WriteToQueue(QueueNames.QUEUE_ENRICHED_MESSAGE_WRITE, enrichedMessage);
                //messageProcessor.Process(deserializedMessage);

                _readedEvents++;
            }
            catch (MessageProcessorException ex)
            {
                _logger.LogError($"{_baseLogMessage}Error processing message. Message: {message}", ex);
            }
            catch (JsonException ex)
            {
                _logger.LogError($"{_baseLogMessage}Error parsing json. Message: {message}", ex);
            }

            await Task.CompletedTask;
        }
    }
}

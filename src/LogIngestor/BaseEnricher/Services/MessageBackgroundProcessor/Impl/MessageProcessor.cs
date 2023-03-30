using BaseEnricher.Constants;
using BaseEnricher.Models;
using BaseEnricher.Services.MessageProcessor;
using BaseEnricher.Services.MessageProcessor.Commands;
using BaseEnricher.Services.MessageService;
using System.Text.Json;

namespace BaseEnricher.Services.MessageBackgroundProcessor
{
    public class MessageProcessor : BackgroundService, IMessageProcessorBackground
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private string _hostname;
        private string _topic;
        private int _readedEvents;
        private Guid _consumer_guid;
        private string _baseLogMessage;

        public MessageProcessor(ILogger<MessageProcessor> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
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

            var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer<BaseLogMessage>>();
            messageConsumer.Configure(_hostname);

            await messageConsumer.SubscribeAsync(_topic);

            messageConsumer.OnMessageReceived += ExecuteAction;

            await Task.CompletedTask;
        }
        private void ExecuteAction(object? obj, BaseLogMessage message)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            var messageProducer = scope.ServiceProvider.GetRequiredService<IMessageProducer<EnrichedLogMessage>>();
            try
            {
                var addDateCommand = new MessageProcessor<EnrichedLogMessage, BaseLogMessage>(new AddDateProcessCommand());
                _logger.LogDebug($"{_baseLogMessage}Add date to message: {message}");
                
                var enrichedMessage = addDateCommand.Execute(message);

                messageProducer.Configure(_hostname);
                messageProducer.WriteToQueue(QueueNames.QUEUE_ENRICHED_MESSAGE_WRITE, enrichedMessage);

                _readedEvents++;
            }
            //catch (MessageProcessorException ex)
            //{
            //    _logger.LogError($"{_baseLogMessage}Error processing message. Message: {message}", ex);
            //}
            catch (JsonException ex)
            {
                _logger.LogError($"{_baseLogMessage}Error parsing json. Message: {message}", ex);
            }
        }
    }
}

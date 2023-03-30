using BaseEnricher.Constants;
using BaseEnricher.Models;
using BaseEnricher.Services.MessageProcessor;
using BaseEnricher.Services.MessageProcessor.Commands;
using BaseEnricher.Services.MessageService;

namespace BaseEnricher.Services.MessageBackgroundProcessor
{
    public class MessageProcessor : BackgroundService, IMessageProcessorBackground
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IServiceProvider _serviceProvider;

        private string? _in_broker_hostname;
        private string? _in_broker_topic;
        private string? _out_broker_hostname;
        private string? _out_broker_topic;

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

        public void Configure(string in_broker_host, string in_broker_topic, string out_broker_host, string out_broker_topic)
        {
            _in_broker_hostname= in_broker_host;
            _in_broker_topic= in_broker_topic;
            _out_broker_hostname= out_broker_host;
            _out_broker_topic= out_broker_topic;
            _logger.LogInformation($"{_baseLogMessage}Configured. Input from broker: {_in_broker_hostname}, topic {_in_broker_topic}. Output to broker: {_out_broker_hostname}, topic {_out_broker_topic}");
        }

        //public void Configure(string host, string topic)
        //{
        //    _hostname = host;
        //    _topic = topic;
        //    _logger.LogInformation($"{_baseLogMessage}Configured. Remote queue host: {_hostname}. Queue channel to be consumed: {_topic}");
        //}

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
            if(_in_broker_hostname == null || _in_broker_topic == null || _out_broker_hostname == null || _out_broker_topic == null) {
                _logger.LogError($"{_baseLogMessage}Error, service not correctly called.");
                return;
            }

            using IServiceScope scope = _serviceProvider.CreateScope();

            var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer<BaseLogMessage>>();
            messageConsumer.Configure(_in_broker_hostname);

            messageConsumer.Subscribe(_in_broker_topic);

            messageConsumer.OnMessageReceived += ExecuteAction;

            await Task.CompletedTask;
        }
        private void ExecuteAction(object? obj, BaseLogMessage message)
        {
            using IServiceScope scope = _serviceProvider.CreateScope();
            if (_in_broker_hostname == null || _in_broker_topic == null || _out_broker_hostname == null || _out_broker_topic == null)
            {
                _logger.LogError($"{_baseLogMessage}Error, service not correctly called.");
                return;
            }
            var messageProducer = scope.ServiceProvider.GetRequiredService<IMessageProducer<EnrichedLogMessage>>();
            try
            {
                var addDateCommand = new MessageProcessor<EnrichedLogMessage, BaseLogMessage>(new AddDateProcessCommand());
                _logger.LogDebug($"{_baseLogMessage}Add date to message: {message}");
                
                var enrichedMessage = addDateCommand.Execute(message);
                if (_out_broker_hostname == null)
                    throw new ArgumentNullException(nameof(_out_broker_hostname));
                messageProducer.Configure(_out_broker_hostname);
                messageProducer.WriteToQueue(_out_broker_topic, enrichedMessage);

                _readedEvents++;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_baseLogMessage}Error while processing message: {message}", ex);
            }
        }
    }
}

using BaseEnricher.Models;
using BaseEnricher.Services.MessageBrokerConfigurationBuilder;
using BaseEnricher.Services.MessageProcessor;
using BaseEnricher.Services.MessageProcessor.Commands;
using BaseEnricher.Services.MessageService;

namespace BaseEnricher.Services.MessageBackgroundProcessor
{
    public class MessageProcessor : BackgroundService, IMessageProcessorBackground
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMessageProducer<EnrichedLogMessage> _messageProducer;
        private readonly AddDateProcessCommand _addDateProcessCommand;
        private readonly IServiceScope _scope;
        private readonly Guid _consumer_guid;
        private readonly string _baseLogMessage;

        private string? _in_broker_hostname;
        private string? _in_broker_topic;
        private string? _out_broker_hostname;
        private string? _out_broker_topic;

        private int _readedEvents;

        public MessageProcessor(ILogger<MessageProcessor> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _readedEvents = 0;
            _consumer_guid = Guid.NewGuid();
            _baseLogMessage = $"Message Processor[{_consumer_guid}]: ";
            _scope = serviceProvider.CreateScope();
            _messageProducer = _scope.ServiceProvider.GetRequiredService<IMessageProducer<EnrichedLogMessage>>();
            _addDateProcessCommand = _scope.ServiceProvider.GetRequiredService<AddDateProcessCommand>();

            _logger.LogInformation($"{_baseLogMessage}Message processor created. Unique id: {_consumer_guid}");
        }

        public void Configure(IMessageBrokerConfiguration input_broker_conf, IMessageBrokerConfiguration output_broker_conf)
        {
            _in_broker_hostname = input_broker_conf.Hostname;
            _in_broker_topic = input_broker_conf.Topic;
            _out_broker_hostname = output_broker_conf.Hostname;
            _out_broker_topic = output_broker_conf.Topic;
            _messageProducer.Configure(output_broker_conf.Hostname);
            _logger.LogInformation($"{_baseLogMessage}Configured. Input from broker: {_in_broker_hostname}, topic {_in_broker_topic}. Output to broker: {_out_broker_hostname}, topic {_out_broker_topic}");
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
            if (_in_broker_hostname == null || _in_broker_topic == null || _out_broker_hostname == null || _out_broker_topic == null)
            {
                _logger.LogError($"{_baseLogMessage}Error, service not correctly called.");
                return;
            }

            try
            {
                var addDateCommand = new MessageProcessor<EnrichedLogMessage, BaseLogMessage>(_addDateProcessCommand);
                _logger.LogDebug($"{_baseLogMessage}Add date to message: {message}");

                var enrichedMessage = addDateCommand.Execute(message);
                _messageProducer.WriteToQueue(_out_broker_topic, enrichedMessage);

                _readedEvents++;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_baseLogMessage}Error while processing message: {message}", ex);
            }
        }
    }
}

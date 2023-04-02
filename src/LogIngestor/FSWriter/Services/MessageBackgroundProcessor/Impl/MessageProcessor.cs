using FSWriter.Models;
using FSWriter.Services.FileWriter;
using FSWriter.Services.MessageBrokerConfigurationBuilder;
using FSWriter.Services.MessageService;

namespace FSWriter.Services.MessageBackgroundProcessor
{
    public class MessageProcessor : BackgroundService, IMessageProcessorBackground
    {
        private readonly ILogger<MessageProcessor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileWriter<EnrichedLogMessage> _filewriter;
        private readonly IServiceScope _scope;
        private readonly Guid _consumer_guid;
        private readonly string _baseLogMessage;

        private string? _in_broker_hostname;
        private string? _in_broker_topic;
        private string _baseStoragePath;

        private int _readedEvents;

        public MessageProcessor(ILogger<MessageProcessor> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _readedEvents = 0;
            _consumer_guid = Guid.NewGuid();
            _baseLogMessage = $"FSWriter worker[{_consumer_guid}]: ";
            _scope = serviceProvider.CreateScope();
            _filewriter = _scope.ServiceProvider.GetRequiredService<IFileWriter<EnrichedLogMessage>>();

            _logger.LogInformation($"{_baseLogMessage}Message processor created. Unique id: {_consumer_guid}");
        }

        public void Configure(IMessageBrokerConfiguration input_broker_conf, IFileWriterConfiguration storage_basepath)
        {
            _in_broker_hostname = input_broker_conf.Hostname;
            _in_broker_topic = input_broker_conf.Topic;
            _baseStoragePath = storage_basepath.BasePath;

            _logger.LogInformation($"{_baseLogMessage}Configured. Input from broker: {_in_broker_hostname}, topic {_in_broker_topic}. Output to storage filesystem: {storage_basepath}");
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
            if(_in_broker_hostname == null || _in_broker_topic == null) {
                _logger.LogError($"{_baseLogMessage}Error, service not correctly called.");
                return;
            }

            using IServiceScope scope = _serviceProvider.CreateScope();

            var messageConsumer = scope.ServiceProvider.GetRequiredService<IMessageConsumer<EnrichedLogMessage>>();
            messageConsumer.Configure(_in_broker_hostname);
            messageConsumer.Subscribe(_in_broker_topic);

            messageConsumer.OnMessageReceived += ExecuteAction;

            await Task.CompletedTask;
        }

        [Obsolete("Checkout: try to specify BaseLogMessage as parameter, see what happens.")]
        private void ExecuteAction(object? obj, EnrichedLogMessage message)
        {
            if (_in_broker_hostname == null || _in_broker_topic == null || _baseStoragePath == null)
            {
                _logger.LogError($"{_baseLogMessage}Error, service not correctly called.");
                return;
            }

            try
            {
                _logger.LogDebug($"{_baseLogMessage}Writing message to disk: {message}");
                var completePath = _baseStoragePath + "/" + message.AgentMetaData.AgentHostName;
                _filewriter.AppendToFile(completePath, message);
                _readedEvents++;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{_baseLogMessage}Error while processing message: {message}", ex);
            }
        }
    }
}

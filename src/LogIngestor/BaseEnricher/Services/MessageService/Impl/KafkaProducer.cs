using BaseEnricher.Services.JsonSerializer;
using BaseEnricher.Services.MetricsService;
using Confluent.Kafka;
using System.Diagnostics.Metrics;

namespace BaseEnricher.Services.MessageService.Impl
{
    public class KafkaProducer<T> : IMessageProducer<T>
    {
        private readonly ILogger<KafkaProducer<T>> _logger;
        private readonly IJsonSerializer<T> _jsonSerializer;
        private readonly IMetricsService _metricsService;
        private readonly Guid _service_guid;
        private readonly string _baseLogMessage;
        private ProducerConfig _config;
        private IProducer<Null, string> _producerBuilder;
        private int counter = 0;
        public KafkaProducer(ILogger<KafkaProducer<T>> logger, IJsonSerializer<T> jsonSerializer, IMetricsService metricsService)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _metricsService = metricsService;
            _service_guid = Guid.NewGuid();
            _baseLogMessage = $"Kafka Producer[{_service_guid}]: ";
        }

        public void Configure(string hostname, int port)
        {
            _config = new ProducerConfig { BootstrapServers = $"{hostname}:{port}", BatchSize = 100000, LingerMs = 200 };
            _producerBuilder = new ProducerBuilder<Null, string>(_config).Build();
        }

        public void PublishBatch(string topic, IList<T> messages)
        {
            try
            {
                var messageList = messages.ToList();
                foreach (var message in messageList)
                {
                    var serializedMessage = _jsonSerializer.Serialize(message);
                    _producerBuilder.Produce(topic, new Message<Null, string> { Value = serializedMessage });
                }
                counter += messageList.Count;
                _logger.LogInformation($"{_baseLogMessage}Delivered batch of {messageList.Count} messages. Total {counter}");
                _producerBuilder.Flush();
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
            }
        }

        public bool Publish(string topic, T message)
        {
            try
            {
                var serializedMessage = _jsonSerializer.Serialize(message);
                _producerBuilder.Produce(topic, new Message<Null, string> { Value = serializedMessage });
                _metricsService.SignalNewEvent(_service_guid.ToString(), "main.service.out");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"{_baseLogMessage}Delivery failed: {e.Error.Reason}");
            }
            return true;
        }
    }
}

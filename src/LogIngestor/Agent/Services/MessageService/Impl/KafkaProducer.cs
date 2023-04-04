using Agent.Services.JsonSerializer;
using Confluent.Kafka;
using System.Diagnostics.Metrics;

namespace Agent.Services.MessageService.Impl
{
    public class KafkaProducer<T> : IMessageProducer<T>
    {
        private readonly ILogger<KafkaProducer<T>> _logger;
        private readonly IJsonSerializer<T> _jsonSerializer;
        private ProducerConfig _config;
        private IProducer<Null, string> _producerBuilder;
        private int counter = 0;
        public KafkaProducer(ILogger<KafkaProducer<T>> logger, IJsonSerializer<T> jsonSerializer)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
        }

        public void Configure(string hostname, string port)
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
                _logger.LogInformation($"Delivered batch of {messageList.Count} messages. Total {counter}");
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
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
            }

            return true;
        }
    }
}

using Agent.Services.JsonSerializer;
using Confluent.Kafka;

namespace Agent.Services.MessageService.Impl
{
    public class KafkaProducer<T> : IMessageProducer<T>
    {
        private readonly ILogger<KafkaProducer<T>> _logger;
        private readonly IJsonSerializer<T> _jsonSerializer;
        private ProducerConfig _config;
        private IProducer<Null, string> _producerBuilder;
        private int counter = 0;
        private Guid _consumer_guid;
        private string _baseLogMessage;

        public event Action<string> OnPublish;

        public KafkaProducer(ILogger<KafkaProducer<T>> logger, IJsonSerializer<T> jsonSerializer)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _consumer_guid = Guid.NewGuid();
            _baseLogMessage = $"Kafka Producer[{_consumer_guid}]: ";
            _logger.LogInformation($"{_baseLogMessage}Created. Unique id: {_consumer_guid}");
        }

        public void Configure(string hostname, string port)
        {
            _config = new ProducerConfig { BootstrapServers = $"{hostname}:{port}", BatchSize = 100000, LingerMs = 200 };
            _producerBuilder = new ProducerBuilder<Null, string>(_config).Build();
            _logger.LogInformation($"{_baseLogMessage}Configured with host {hostname}:{port}");
        }

        public void PublishBatch(string topic, IList<T> messages)
        {
            throw new NotImplementedException("Not implemented yet with Kafka");
            //try
            //{
            //    var messageList = messages.ToList();
            //    foreach (var message in messageList)
            //    {
            //        var serializedMessage = _jsonSerializer.Serialize(message);
            //        _producerBuilder.Produce(topic, new Message<Null, string> { Value = serializedMessage });
            //    }
            //    counter += messageList.Count;
            //    _logger.LogInformation($"Delivered batch of {messageList.Count} messages. Total {counter}");
            //    _producerBuilder.Flush();
            //    OnPublish.Invoke();
            //}
            //catch (ProduceException<Null, string> e)
            //{
            //    _logger.LogError($"Delivery failed: {e.Error.Reason}");
            //}
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
                _logger.LogError($"{_baseLogMessage}Delivery failed: {e.Error.Reason}");
            }

            OnPublish?.Invoke(_consumer_guid.ToString());
            return true;
        }
    }
}

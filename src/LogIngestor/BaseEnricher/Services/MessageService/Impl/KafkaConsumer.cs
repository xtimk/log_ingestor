using BaseEnricher.Services.JsonSerializer;
using Confluent.Kafka;

namespace BaseEnricher.Services.MessageService.Impl
{
    public class KafkaConsumer<T> : IMessageConsumer<T>
    {
        private readonly ILogger<KafkaConsumer<T>> _logger;
        private readonly IJsonSerializer<T> _jsonSerializer;
        private ConsumerConfig _config;
        private IConsumer<Null, string> _consumer;
        private bool _cancelled = false;

        public event EventHandler<T> OnMessageReceived;
        public KafkaConsumer(ILogger<KafkaConsumer<T>> logger, IJsonSerializer<T> jsonSerializer)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
        }


        public void Configure(string hostname, int port)
        {
            _config = new ConsumerConfig { 
                BootstrapServers = $"{hostname}:{port}",
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(_config).Build();
        }

        public void Subscribe(string topic)
        {
            _consumer.Subscribe(topic);
            while(!_cancelled)
            {
                var consumeResult = _consumer.Consume();
                var message = consumeResult.Message.Value;
                var deserializedMessage = _jsonSerializer.Deserialize(message);
                if(deserializedMessage == null)
                {
                    throw new Exception("deserialized message is null");
                }
                OnMessageReceived?.Invoke(this, deserializedMessage);
            }
            _consumer.Close();
        }
    }
}

using FSWriter.Services.JsonSerializer;
using Confluent.Kafka;
using static Confluent.Kafka.ConfigPropertyNames;
using System.Threading;
using Confluent.Kafka.Admin;

namespace FSWriter.Services.MessageService.Impl
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
                GroupId = "fs-writer",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            _consumer = new ConsumerBuilder<Null, string>(_config).Build();
        }

        public void Subscribe(string topic)
        {
            // eventually create topic
            using (var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _config.BootstrapServers }).Build())
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
                var topicsMetadata = metadata.Topics;
                var topicNames = metadata.Topics.Select(a => a.Topic).ToList();

                if (topicNames.Contains(topic))
                {
                    var topicsToDelete = new List<string>
                    {
                        topic
                    };
                    adminClient.DeleteTopicsAsync(topicsToDelete).Wait();
                    Thread.Sleep(1000);
                }
                var retentionMs = 1 * 60 * 60 * 1000; // 1 hour
                var topicConfigs = new Dictionary<string, string>
                {
                    { "retention.ms", retentionMs.ToString() },
                    { "delete.retention.ms", retentionMs.ToString() }
                };
                try
                {
                    adminClient.CreateTopicsAsync(new TopicSpecification[] {
                        new TopicSpecification {
                            Name = topic, ReplicationFactor = 1,
                            NumPartitions = 1,
                            Configs = topicConfigs
                        }
                    }).Wait();
                }
                catch (CreateTopicsException e)
                {
                    _logger.LogError($"An error occured creating topic {e.Results[0].Topic}: {e.Results[0].Error.Reason}");
                }
            }

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

using Agent.Services.JsonSerializer;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Agent.Services.MessageService.Impl
{
    public class RabbitMQProducer<T> : IMessageProducer<T>
    {
        private readonly ILogger<RabbitMQProducer<T>> _logger;
        private readonly IJsonSerializer<T> _jsonSerializer;
        private ConnectionFactory? _factory;
        private Guid _consumer_guid;
        private string _baseLogMessage;
        private IConnection _connection;
        private IModel _channel;

        public event Action OnPublish;

        public RabbitMQProducer(ILogger<RabbitMQProducer<T>> logger, IJsonSerializer<T> jsonSerializer)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _consumer_guid = Guid.NewGuid();
            _baseLogMessage = $"RabbitMQ Producer[{_consumer_guid}]: ";
            _logger.LogInformation($"{_baseLogMessage}Created. Unique id: {_consumer_guid}");
        }

        event Action<string> IMessageProducer<T>.OnPublish
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public void Configure(string hostname, string? port)
        {
            _factory = new ConnectionFactory() { HostName = hostname };
            _logger.LogInformation($"{_baseLogMessage}Configured. Queue host: {hostname}");
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public Task PublishAsync(string topic, T message)
        {
            throw new NotImplementedException();
        }

        public Task PublishAsyncBatch(string topic, IList<T> message)
        {
            throw new NotImplementedException();
        }

        public void PublishBatch(string topic, IList<T> messages)
        {
            throw new NotImplementedException();
        }

        public bool Publish(string topic, T message)
        {
            if (_factory == null)
            {
                _logger.LogError($"{_baseLogMessage}Can't write to {topic}. You have to call method Configure first.");
                throw new ArgumentNullException(nameof(_factory));
            }

            var arguments = new Dictionary<string, object>
            {
                { "x-queue-type", "stream" }
            };

            //_channel.BasicQos(prefetchSize: 0, prefetchCount: 100, global: false);

            _channel.QueueDeclare(
                queue: topic,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);


            var serializedMessage = _jsonSerializer.Serialize(message);
            
            var body = Encoding.UTF8.GetBytes(serializedMessage);

            _channel.BasicPublish(exchange: string.Empty,
                                 routingKey: topic,
                                 basicProperties: null,
                                 body: body);
            _logger.LogDebug($"{_baseLogMessage}Sent {serializedMessage} to topic {topic}");

            return true;
        }
    }
}

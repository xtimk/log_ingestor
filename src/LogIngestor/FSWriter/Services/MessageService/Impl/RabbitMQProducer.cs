using FSWriter.Services.JsonSerializer;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace FSWriter.Services.MessageService.Impl
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


        public RabbitMQProducer(ILogger<RabbitMQProducer<T>> logger, IJsonSerializer<T> jsonSerializer)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _consumer_guid = Guid.NewGuid();
            _baseLogMessage = $"RabbitMQ Producer[{_consumer_guid}]: ";
            _logger.LogInformation($"{_baseLogMessage}Created. Unique id: {_consumer_guid}");

        }

        public void Configure(string hostname)
        {
            _factory = new ConnectionFactory() { HostName = hostname };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            _logger.LogInformation($"{_baseLogMessage}Configured. Queue host: {hostname}");
        }

        public bool WriteToQueue(string topic, T message)
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
            _channel.QueueDeclare(
                queue: topic,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);
            //_channel.BasicQos(prefetchSize: 0, prefetchCount: 100, global: false);


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

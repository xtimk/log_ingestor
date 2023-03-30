using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace BaseEnricher.Services.MessageService.Impl
{
    public class RabbitMQProducer<T> : IMessageProducer<T>
    {
        private readonly ILogger<RabbitMQProducer<T>> _logger;
        
        private ConnectionFactory? _factory;
        private Guid _consumer_guid;
        private string _baseLogMessage;


        public RabbitMQProducer(ILogger<RabbitMQProducer<T>> logger)
        {
            _logger = logger;
            _consumer_guid= Guid.NewGuid();
            _baseLogMessage = $"RabbitMQ Producer[{_consumer_guid}]: ";
            _logger.LogInformation($"{_baseLogMessage}Created. Unique id: {_consumer_guid}");
        }

        public void Configure(string hostname)
        {
            _factory = new ConnectionFactory() { HostName = hostname };
            _logger.LogInformation($"{_baseLogMessage}Configured. Queue host: {hostname}");
        }

        public bool WriteToQueue(string topic, T message)
        {
            if (_factory == null)
            {
                _logger.LogError($"{_baseLogMessage}Can't write to {topic}. You have to call method Configure first.");
                throw new ArgumentNullException(nameof(_factory));
            }

            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(
                queue: topic,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);


            var serializedMessage = JsonSerializer.Serialize<T>(message);
            
            var body = Encoding.UTF8.GetBytes(serializedMessage);

            channel.BasicPublish(exchange: string.Empty,
                                 routingKey: topic,
                                 basicProperties: null,
                                 body: body);
            _logger.LogDebug($"{_baseLogMessage}Sent {serializedMessage} to topic {topic}");

            return true;
        }
    }
}

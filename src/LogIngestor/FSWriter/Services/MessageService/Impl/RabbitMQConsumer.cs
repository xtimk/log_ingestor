using FSWriter.Models;
using FSWriter.Services.JsonSerializer;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace FSWriter.Services.MessageService.Impl
{
    public class RabbitMQConsumer<T> : IMessageConsumer<T> where T : Message
    {
        private readonly ILogger<RabbitMQConsumer<T>> _logger;
        private readonly IJsonSerializer<T> _jsonSerializer;
        private string? _hostname;
        private int _port;

        public event EventHandler<T>? OnMessageReceived;

        public RabbitMQConsumer(ILogger<RabbitMQConsumer<T>> logger, IJsonSerializer<T> jsonSerializer)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
        }

        public void Configure(string hostname, int port)
        {
            _hostname = hostname;
            _port = port;
        }

        public void Subscribe(string topic)
        {
            if (_hostname == null)
            {
                throw new ArgumentNullException("You have to call Configure method prior to subscribing to a topic", nameof(_hostname));
            }

            var baseLogMessage = $"RabbitMQ Consumer[{Guid.NewGuid()}]: ";

            var _factory = new ConnectionFactory() { HostName = _hostname, Port = _port, DispatchConsumersAsync = false };
            IConnection connection = _factory.CreateConnection();
            IModel channel = connection.CreateModel();

            var arguments = new Dictionary<string, object>
            {
                { "x-queue-type", "stream" }
            };
            channel.QueueDeclare(queue: topic, durable: false, exclusive: false, autoDelete: false, arguments: null);
            //channel.BasicQos(prefetchSize: 0, prefetchCount: 100, global: false);

            // Don't dispatch a new message to a consumer until it has processed and acknowledged the previous one.
            //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Registered += (model, body) =>
            {
                _logger.LogInformation($"{baseLogMessage}A new subscriber has been detected.");
            };
            consumer.Received += (model, body) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(body.Body.ToArray());
                    _logger.LogDebug($"{baseLogMessage}Received a new message: {message}");
                    
                    var deserializedMessage = _jsonSerializer.Deserialize(message);

                    if(deserializedMessage == null)
                    {
                        _logger.LogError($"{baseLogMessage}Error, deserialization of message produced null.");
                        throw new JsonException();
                    }
                    if(OnMessageReceived == null)
                    {
                        _logger.LogError($"{baseLogMessage}Error, can't invoke event, is null.");
                        throw new Exception();
                    }
                    OnMessageReceived.Invoke(model, deserializedMessage);
                    channel.BasicAck(body.DeliveryTag, false);
                }
                catch (AlreadyClosedException ex)
                {
                    _logger.LogError($"{baseLogMessage}Error, connection with message broker is closed.", ex);
                }
                catch (Exception e)
                {
                    _logger.LogError($"{baseLogMessage}Unable to execute action due to an error", e);
                }
            };

            channel.BasicConsume(queue: topic, autoAck: false, consumer: consumer);
        }
    }
}

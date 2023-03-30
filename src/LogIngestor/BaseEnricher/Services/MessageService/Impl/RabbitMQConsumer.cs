using BaseEnricher.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;

namespace BaseEnricher.Services.MessageService.Impl
{
    public class RabbitMQConsumer<T> : IMessageConsumer<T> where T : Message
    {
        private readonly ILogger<RabbitMQConsumer<T>> _logger;
        private string? _hostname;

        public event EventHandler<T>? OnMessageReceived;

        public RabbitMQConsumer(ILogger<RabbitMQConsumer<T>> logger)
        {
            _logger = logger;
        }

        public void Configure(string hostname)
        {
            _hostname = hostname;
        }

        public void Subscribe(string topic)
        {
            if (_hostname == null)
            {
                throw new ArgumentNullException("You have to call Configure method prior to subscribing to a topic", nameof(_hostname));
            }

            var baseLogMessage = $"RabbitMQ Consumer[{Guid.NewGuid()}]: ";

            var _factory = new ConnectionFactory() { HostName = _hostname, DispatchConsumersAsync = false };
            IConnection connection = _factory.CreateConnection();
            IModel channel = connection.CreateModel();
            channel.QueueDeclare(queue: topic, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Don't dispatch a new message to a consumer until it has processed and acknowledged the previous one.
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

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
                    _logger.LogInformation($"{baseLogMessage}Received a new message: {message}");
                    
                    var deserializedMessage = JsonSerializer.Deserialize<T>(message);
                    //await OnMessageReceived.Invoke(model, body);

                    if(deserializedMessage == null)
                    {
                        // exception throw here...
                        _logger.LogError($"{baseLogMessage}Error, deserialization of message produced null.");
                        throw new Exception();
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
                finally
                {
                    // nothing..
                }
            };

            // Register a consumer to listen to a specific queue. 
            channel.BasicConsume(queue: topic, autoAck: false, consumer: consumer);
            //await Task.CompletedTask;
        }
    }
}

using BaseEnricher.Constants;
using BaseEnricher.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Data.Common;
using System.Text;
using System.Threading.Channels;

namespace BaseEnricher.Services.MessageService.Impl
{
    public class RabbitMQConsumer : IMessageConsumer
    {
        private readonly ILogger<RabbitMQConsumer> _logger;
        //private readonly IMessageBrokerConfiguration _messageBrokerConfiguration;
        private string? _hostname;

        public event AsyncEventHandler<BasicDeliverEventArgs> OnMessageReceived;

        public RabbitMQConsumer(ILogger<RabbitMQConsumer> logger)
        {
            _logger = logger;
            //_messageBrokerConfiguration = messageBrokerConfiguration;
        }

        public void Configure(string hostname)
        {
            _hostname = hostname;
        }

        public async Task SubscribeAsync(string topic)
        {
            if (_hostname == null)
            {
                //_logger.LogError($"{base}Can't write to {topic}. You have to call method Configure first.");
                throw new ArgumentNullException("You have to call Configure method prior to subscribing to a topic", nameof(_hostname));
            }

            var baseLogMessage = $"RabbitMQ Consumer[{Guid.NewGuid()}]: ";

            var _factory = new ConnectionFactory() { HostName = _hostname, DispatchConsumersAsync = true };
            IConnection connection = _factory.CreateConnection();
            IModel channel = connection.CreateModel();
            channel.QueueDeclare(queue: topic, durable: false, exclusive: false, autoDelete: false, arguments: null);

            // Don't dispatch a new message to a consumer until it has processed and acknowledged the previous one.
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            var consumer = new AsyncEventingBasicConsumer(channel); // non-blocking
            consumer.Registered += async (model, body) =>
            {
                _logger.LogInformation($"{baseLogMessage}A new subscriber has been detected.");
            };
            consumer.Received += async (model, body) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(body.Body.ToArray());
                    _logger.LogInformation($"{baseLogMessage}Received a new message: {message}");
                    await OnMessageReceived.Invoke(model, body);
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
            await Task.CompletedTask;
        }
    }
}

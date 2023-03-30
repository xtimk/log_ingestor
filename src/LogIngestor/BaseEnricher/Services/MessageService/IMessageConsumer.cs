using RabbitMQ.Client.Events;

namespace BaseEnricher.Services.MessageService
{
    public interface IMessageConsumer
    {
        Task SubscribeAsync(string topic);
        void Configure(string hostname);

        event AsyncEventHandler<BasicDeliverEventArgs> OnMessageReceived;
    }
}

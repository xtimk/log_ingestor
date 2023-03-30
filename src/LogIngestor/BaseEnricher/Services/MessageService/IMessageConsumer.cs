using BaseEnricher.Models;
using RabbitMQ.Client.Events;

namespace BaseEnricher.Services.MessageService
{
    public interface IMessageConsumer<T>
    {
        Task SubscribeAsync(string topic);
        void Configure(string hostname);

        event EventHandler<T> OnMessageReceived;
    }
}

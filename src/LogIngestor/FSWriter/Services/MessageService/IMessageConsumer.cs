namespace FSWriter.Services.MessageService
{
    public interface IMessageConsumer<T>
    {
        void Subscribe(string topic);
        void Configure(string hostname, int port);

        event EventHandler<T> OnMessageReceived;
    }
}

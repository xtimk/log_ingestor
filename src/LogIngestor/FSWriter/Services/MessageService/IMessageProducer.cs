namespace FSWriter.Services.MessageService
{
    public interface IMessageProducer<T>
    {
        public void Configure(string hostname, int port);
        public bool Publish(string topic, T message);
    }
}

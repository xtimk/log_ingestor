namespace BaseEnricher.Services.MessageService
{
    public interface IMessageProducer<T>
    {
        public void Configure(string hostname, int port);
        public bool Publish(string topic, T message);
        public void PublishBatch(string topic, IList<T> messages);
    }
}

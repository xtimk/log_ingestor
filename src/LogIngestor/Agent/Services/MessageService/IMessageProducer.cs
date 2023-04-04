namespace Agent.Services.MessageService
{
    public interface IMessageProducer<T>
    {
        public void Configure(string hostname, string port);
        public bool Publish(string topic, T message);
        public void PublishBatch(string topic, IList<T> messages);
    }
}

namespace Agent.Services.MessageService
{
    public interface IMessageProducer<T>
    {
        // Event raised at every publish
        // Can be used for example to collect metrics
        public event Action<string> OnPublish;
        public void Configure(string hostname, string port);
        public bool Publish(string topic, T message);
        public void PublishBatch(string topic, IList<T> messages);
    }
}

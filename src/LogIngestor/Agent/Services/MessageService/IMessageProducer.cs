namespace Agent.Services.MessageService
{
    public interface IMessageProducer<T>
    {
        public void Configure(string hostname);
        public bool WriteToQueue(string topic, T message);
    }
}

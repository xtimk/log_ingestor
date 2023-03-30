namespace BaseEnricher.Services.MessageProcessor
{
    public interface IMessageProcessor<T>
    {
        void Process(T message);
    }
}

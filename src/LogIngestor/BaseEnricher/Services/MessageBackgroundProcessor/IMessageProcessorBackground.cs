namespace BaseEnricher.Services.MessageBackgroundProcessor
{
    public interface IMessageProcessorBackground : IHostedService
    {
        void Configure(string host, string topic);
        int NumberOfEventsReaded();
    }
}

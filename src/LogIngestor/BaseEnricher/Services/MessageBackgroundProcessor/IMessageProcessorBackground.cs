namespace BaseEnricher.Services.MessageBackgroundProcessor
{
    public interface IMessageProcessorBackground : IHostedService
    {
        void Configure(string in_broker_host, string in_broker_topic, string out_broker_host, string out_broker_topic);
        int NumberOfEventsReaded();
    }
}

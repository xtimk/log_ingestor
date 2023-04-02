using BaseEnricher.Services.MessageBrokerConfigurationBuilder;

namespace BaseEnricher.Services.MessageBackgroundProcessor
{
    public interface IMessageProcessorBackground : IHostedService
    {
        void Configure(IMessageBrokerConfiguration input_broker_configuration, IMessageBrokerConfiguration output_broker_configuration);
        int NumberOfEventsReaded();
    }
}

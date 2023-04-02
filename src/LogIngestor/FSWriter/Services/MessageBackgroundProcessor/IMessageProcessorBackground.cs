using FSWriter.Services.MessageBrokerConfigurationBuilder;

namespace FSWriter.Services.MessageBackgroundProcessor
{
    public interface IMessageProcessorBackground : IHostedService
    {
        void Configure(IMessageBrokerConfiguration input_broker_configuration, IFileWriterConfiguration fileStorageConfiguration);
        int NumberOfEventsReaded();
    }
}

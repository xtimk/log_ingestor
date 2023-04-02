using FSWriter.Services.MessageBrokerConfigurationBuilder;

namespace FSWriter.Services.ConfigurationBuilder
{
    public interface IFileWriterConfigurationBuilder
    {
        IFileWriterConfiguration CreateConfiguration(string? basePath);
    }
}

using FSWriter.Services.MessageBrokerConfigurationBuilder;

namespace FSWriter.Services.ConfigurationBuilder
{
    public interface IMessageBrokerConfigurationBuilder
    {
        IMessageBrokerConfiguration CreateConfiguration(string? hostname, string? port, string? topic);
    }
}

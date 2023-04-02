using BaseEnricher.Services.MessageBrokerConfigurationBuilder;

namespace BaseEnricher.Services.ConfigurationBuilder
{
    public interface IMessageBrokerConfigurationBuilder
    {
        IMessageBrokerConfiguration CreateConfiguration(string? hostname, string? port, string? topic);
    }
}

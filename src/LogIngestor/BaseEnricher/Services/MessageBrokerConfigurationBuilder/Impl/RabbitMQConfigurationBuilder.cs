using BaseEnricher.Exceptions;
using BaseEnricher.Services.ConfigurationBuilder;

namespace BaseEnricher.Services.MessageBrokerConfigurationBuilder.Impl
{
    public class RabbitMQConfigurationBuilder : IMessageBrokerConfigurationBuilder
    {
        public IMessageBrokerConfiguration CreateConfiguration(string? hostname, string? s_port, string? topic)
        {
            if (hostname == null)
            {
                throw new ConfigurationException(nameof(hostname));
            }
            if (s_port == null || !int.TryParse(s_port, out int port))
            {
                throw new ConfigurationException(nameof(s_port));
            }
            if (topic == null)
            {
                throw new ConfigurationException(nameof(topic));
            }

            return new RabbitMQConfiguration(hostname, port, topic);
        }
    }
}

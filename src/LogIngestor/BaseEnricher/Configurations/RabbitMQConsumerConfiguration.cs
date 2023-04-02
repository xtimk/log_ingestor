using BaseEnricher.Services.MessageBrokerConfigurationBuilder;

namespace BaseEnricher.Configurations
{
    public class RabbitMQConsumerConfiguration : IMessageBrokerSingletonConfiguration<RabbitMQConsumerConfiguration>
    {
        private readonly string hostname;
        private readonly int port;
        private readonly string topic;

        public RabbitMQConsumerConfiguration(IMessageBrokerConfiguration conf)
        {
            hostname = conf.Hostname;
            port = conf.Port;
            topic = conf.Topic;
        }

        public string Hostname => hostname;

        public int Port => port;

        public string Topic => topic;
    }
}

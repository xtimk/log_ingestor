using FSWriter.Services.MessageBrokerConfigurationBuilder;

namespace FSWriter.Configurations
{
    public class RabbitMQProducerConfiguration : IMessageBrokerSingletonConfiguration<RabbitMQProducerConfiguration>
    {
        private readonly string hostname;
        private readonly int port;
        private readonly string topic;

        public RabbitMQProducerConfiguration(IMessageBrokerConfiguration conf)
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

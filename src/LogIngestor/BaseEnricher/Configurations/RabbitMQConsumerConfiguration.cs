namespace BaseEnricher.Configurations
{
    public class RabbitMQConsumerConfiguration : IMessageBrokerConfiguration<RabbitMQConsumerConfiguration>
    {
        private readonly string hostname;
        private readonly int port;
        private readonly string topic;

        public RabbitMQConsumerConfiguration(string hostname, int port, string topic)
        {
            this.hostname = hostname;
            this.port = port;
            this.topic = topic;
        }

        public string Hostname => hostname;

        public int Port => port;

        public string Topic => topic;
    }
}

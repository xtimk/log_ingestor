namespace BaseEnricher.Configurations
{
    public class RabbitMQProducerConfiguration : IMessageBrokerConfiguration<RabbitMQProducerConfiguration>
    {
        private readonly string hostname;
        private readonly int port;
        private readonly string topic;

        public RabbitMQProducerConfiguration(string hostname, int port, string topic)
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

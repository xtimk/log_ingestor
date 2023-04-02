namespace BaseEnricher.Services.MessageBrokerConfigurationBuilder.Impl
{
    public class RabbitMQConfiguration : IMessageBrokerConfiguration
    {
        private readonly string _hostname;
        private readonly int _port;
        private readonly string _topic;

        public RabbitMQConfiguration(string hostname, int port, string topic)
        {
            _hostname = hostname;
            _port = port;
            _topic = topic;
        }

        public string Hostname => _hostname;
        public int Port => _port;
        public string Topic => _topic;
    }
}

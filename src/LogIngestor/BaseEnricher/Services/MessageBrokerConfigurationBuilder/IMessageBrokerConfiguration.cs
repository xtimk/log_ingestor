namespace BaseEnricher.Services.MessageBrokerConfigurationBuilder
{
    public interface IMessageBrokerConfiguration
    {
        string Hostname { get; }
        int Port { get; }
        string Topic { get; }
    }
}

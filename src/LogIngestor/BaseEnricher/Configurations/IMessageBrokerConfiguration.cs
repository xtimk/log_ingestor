namespace BaseEnricher.Configurations
{
    public interface IMessageBrokerConfiguration<T>
    {
        string Hostname { get; }
        int Port { get; }
        string Topic { get; }
    }
}

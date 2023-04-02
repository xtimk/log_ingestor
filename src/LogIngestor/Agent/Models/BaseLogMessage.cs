namespace Agent.Models
{
    public class BaseLogMessage : Message
    {
        public string? Message { get; set; }
        public AgentMetaData? AgentMetaData { get; set; }
    }

    public class AgentMetaData
    {
        public DateTime? AgentAcquireDate { get; set; }
        public string? AgentName { get; set; }
        public Version? AgentVersion { get; set; }
        public string AgentHostName { get; set; } = "empty.hostname.local";
    }
}

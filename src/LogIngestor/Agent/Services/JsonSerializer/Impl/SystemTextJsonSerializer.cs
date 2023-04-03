using System.Text.Json;

namespace Agent.Services.JsonSerializer.Impl
{
    public class SystemTextJsonSerializer<T> : IJsonSerializer<T>
    {
        public SystemTextJsonSerializer()
        {
        }

        public T? Deserialize(string message)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(message);
        }

        public string Serialize(T message)
        {
            return System.Text.Json.JsonSerializer.Serialize(message);
        }
    }
}

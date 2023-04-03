namespace FSWriter.Services.JsonSerializer
{
    public interface IJsonSerializer<T>
    {
        string Serialize(T message);
        T? Deserialize(string message);

    }
}

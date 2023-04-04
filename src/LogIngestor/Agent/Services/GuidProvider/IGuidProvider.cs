namespace Agent.Services.GuidProvider
{
    public interface IGuidProvider
    {
        Guid Create();
        Guid Create(string s);
    }
}

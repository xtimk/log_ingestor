namespace Agent.Services.GuidProvider.Impl
{
    public class GuidProvider : IGuidProvider
    {
        public Guid Create()
        {
            return Guid.NewGuid();
        }

        public Guid Create(string s)
        {
            return new Guid(s);
        }
    }
}

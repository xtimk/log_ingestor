using Agent.Services.Readers.Objects;

namespace Agent.Services.Readers.Creators
{
    public interface IReaderCreator
    {
        IReader Create();
    }
}

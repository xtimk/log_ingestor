using Agent.Services.Readers.Creators;
using Agent.Services.Readers.Objects;
using Agent.Services.Readers.Objects.Impl;

namespace Agent.Services.Readers.Factory.Impl
{
    public class FakeMessageGeneratorCreator : IReaderCreator
    {
        public IReader Create()
        {
            return new FakeMessageGeneratorReader();
        }
    }
}

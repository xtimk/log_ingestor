using Agent.Services.Readers.Creators;
using Agent.Services.Readers.Objects;
using Agent.Services.Readers.Objects.Impl;

namespace Agent.Services.Readers.Factory.Impl
{
    public class FakeMessageGeneratorCreator : IReaderCreator
    {
        private readonly IServiceProvider _serviceProvider;

        public FakeMessageGeneratorCreator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IReader Create()
        {
            return (FakeMessageGeneratorReader)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(FakeMessageGeneratorReader));
        }
    }
}

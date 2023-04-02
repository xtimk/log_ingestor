using Agent.Services.Readers.Objects;
using Agent.Services.Readers.Objects.Impl;

namespace Agent.Services.Readers.Creators.Impl
{
    public class FolderReaderCreator : IReaderCreator
    {
        public IReader Create()
        {
            return new FolderReader();
        }
    }
}

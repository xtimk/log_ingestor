using Agent.Services.Readers.Objects.Impl;
using Agent.Services.Readers.Objects;

namespace Agent.Services.Readers.Creators.Impl
{
    public class EventSourceReaderCreator : IReaderCreator
    {
        public IReader Create()
        {
            return new EventSourceReader();
        }
    }
}

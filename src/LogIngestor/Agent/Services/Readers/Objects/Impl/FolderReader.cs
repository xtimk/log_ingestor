using Agent.Models;

namespace Agent.Services.Readers.Objects.Impl
{
    public class FolderReader : IReader
    {
        public event EventHandler<List<BaseLogMessage>> OnNewLines;

        public List<BaseLogMessage> GetLines()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}

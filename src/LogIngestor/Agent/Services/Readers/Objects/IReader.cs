using Agent.Models;

namespace Agent.Services.Readers.Objects
{
    public interface IReader
    {
        event EventHandler<List<BaseLogMessage>> OnNewLines;
        public void Start();
    }
}

using Agent.Models;
using RabbitMQ.Client.Events;

namespace Agent.Services.Readers.Objects
{
    public interface IReader
    {
        event EventHandler<List<BaseLogMessage>> OnNewLines;
        //event AsyncEventHandler<List<BaseLogMessage>> OnNewLinesAsync;
        public void Start(Guid guid);
        public void Stop();
    }
}

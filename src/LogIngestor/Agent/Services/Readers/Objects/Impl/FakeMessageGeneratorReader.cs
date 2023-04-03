using Agent.Models;
using System;

namespace Agent.Services.Readers.Objects.Impl
{
    public class FakeMessageGeneratorReader : IReader
    {
        private readonly ILogger<FakeMessageGeneratorReader> _logger;
        private bool _running;
        private Guid _guid;
        public FakeMessageGeneratorReader(ILogger<FakeMessageGeneratorReader> logger)
        {
            _running = false;
            _logger = logger;
        }

        public event EventHandler<List<BaseLogMessage>>? OnNewLines;

        public void Start(Guid guid)
        {
            var millisecondsToRun = 60000;
            _guid = guid;
            _running = true;
            Task.Run(() => { Thread.Sleep(millisecondsToRun); _running = false; });
            _logger.LogInformation($"FakeReader <{_guid}> started. Reader will run for {millisecondsToRun/1000} seconds.");
            GetLines();
        }

        public void Stop()
        {
            _running= false;
        }

        private void GetLines()
        {
            int numberOfGeneratedMessages = 0;
            var result = new List<BaseLogMessage>();
            while (_running)
            {
                for (int j = 0; j < 1000; j++)
                {
                    result.Add(new BaseLogMessage()
                    {
                        Message = Guid.NewGuid().ToString() + " This is very very log message, i swear, this is not fake generated!",
                        AgentMetaData= new AgentMetaData()
                        {
                            AgentAcquireDate= DateTime.Now,
                            AgentHostName= "my.host.local",
                            AgentName= "LogIngestorAgent.NET",
                            AgentVersion= new Version(1,0,0)
                        }
                    });
                    numberOfGeneratedMessages++;
                }
                OnNewLines?.Invoke(this, result);
                result.Clear();
                Thread.Sleep(100);
            }
            _logger.LogInformation($"FakeReader <{_guid}> stopped");
            _logger.LogInformation($"FakeReader <{_guid}> generated {numberOfGeneratedMessages} messages");
        }
    }
}

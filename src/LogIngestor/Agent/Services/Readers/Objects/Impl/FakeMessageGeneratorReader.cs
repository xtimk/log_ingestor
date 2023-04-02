using Agent.Models;

namespace Agent.Services.Readers.Objects.Impl
{
    public class FakeMessageGeneratorReader : IReader
    {
        public FakeMessageGeneratorReader()
        {
        }

        public event EventHandler<List<BaseLogMessage>>? OnNewLines;

        public void Start()
        {
            GetLines();
        }

        private void GetLines()
        {
            var result = new List<BaseLogMessage>();
            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 5000; j++)
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
                }
                OnNewLines?.Invoke(this, result);
                result.Clear();
                Thread.Sleep(200);
            }
        }
    }
}

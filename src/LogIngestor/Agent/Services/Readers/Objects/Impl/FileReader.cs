﻿using Agent.Models;

namespace Agent.Services.Readers.Objects.Impl
{
    public class FileReader : IReader
    {
        public event EventHandler<List<BaseLogMessage>> OnNewLines;

        public List<BaseLogMessage> GetLines()
        {
            throw new NotImplementedException();
        }

        public void Start(Guid guid)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}

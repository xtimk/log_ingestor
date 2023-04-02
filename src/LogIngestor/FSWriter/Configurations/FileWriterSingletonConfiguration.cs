using FSWriter.Services.MessageBrokerConfigurationBuilder;

namespace FSWriter.Configurations
{
    public class FileWriterSingletonConfiguration : IFileWriterSingletonConfiguration<FileWriterSingletonConfiguration>
    {
        private readonly IFileWriterConfiguration _conf;

        public FileWriterSingletonConfiguration(IFileWriterConfiguration conf)
        {
            _conf = conf;
        }

        public string BasePath => _conf.BasePath;
    }
}

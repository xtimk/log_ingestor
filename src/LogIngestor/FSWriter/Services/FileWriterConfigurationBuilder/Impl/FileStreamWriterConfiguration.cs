namespace FSWriter.Services.MessageBrokerConfigurationBuilder.Impl
{
    public class FileStreamWriterConfiguration : IFileWriterConfiguration
    {
        private readonly string _basePath;

        public FileStreamWriterConfiguration(string basePath)
        {
            _basePath = basePath;
        }

        public string BasePath => _basePath;
    }
}

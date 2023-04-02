using FSWriter.Exceptions;
using FSWriter.Services.ConfigurationBuilder;
using FSWriter.Services.MessageBrokerConfigurationBuilder;
using FSWriter.Services.MessageBrokerConfigurationBuilder.Impl;

namespace FS.Services.MessageBrokerConfigurationBuilder.Impl
{
    public class FileStreamWriterConfigurationBuilder : IFileWriterConfigurationBuilder
    {
        public IFileWriterConfiguration CreateConfiguration(string? basePath)
        {
            if (basePath == null)
            {
                throw new ConfigurationException(nameof(basePath));
            }

            return new FileStreamWriterConfiguration(basePath + "/");
        }
    }
}

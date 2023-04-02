using FS.Services.MessageBrokerConfigurationBuilder.Impl;
using FSWriter.Exceptions;

namespace FSWriter.UnitTests.Services.FileWriterConfigurationBuilders
{
    public class FileStreamWriterConfigurationBuilderTests
    {
        private FileStreamWriterConfigurationBuilder _fsStorageConfigurationBuilder;

        [SetUp]
        public void Setup()
        {
            _fsStorageConfigurationBuilder = new FileStreamWriterConfigurationBuilder();
        }

        [Test]
        public void CreateConfiguration_AllParamsNotNull_ReturnsIFileConfiguration()
        {
            var aBasePath = "a/base/path";
            var configuration = _fsStorageConfigurationBuilder.CreateConfiguration(aBasePath);

            Assert.That(aBasePath + "/", Is.EqualTo(configuration.BasePath));
        }

        [Test]
        public void CreateConfiguration_BasePathNull_ReturnsIFileConfiguration()
        {
            string? aBasePath = null;

            Assert.Throws<ConfigurationException>(() => _fsStorageConfigurationBuilder.CreateConfiguration(aBasePath));
        }

    }
}

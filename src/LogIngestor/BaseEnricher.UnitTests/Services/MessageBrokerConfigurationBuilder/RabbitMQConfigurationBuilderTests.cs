using BaseEnricher.Exceptions;
using BaseEnricher.Services.ConfigurationBuilder;
using BaseEnricher.Services.MessageBrokerConfigurationBuilder.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseEnricher.UnitTests.Services.MessageBrokerConfigurationBuilder
{
    public class RabbitMQConfigurationBuilderTests
    {
        RabbitMQConfigurationBuilder _rabbitmqConfigurationBuilder;

        [SetUp]
        public void Setup()
        {
            _rabbitmqConfigurationBuilder = new RabbitMQConfigurationBuilder();
        }

        [Test]
        public void CreateConfiguration_AllParamsNotNull_ReturnsIMessageConfiguration()
        {
            var hostname = "a_hostname";
            var port = "123";
            var topic = "a_topic";

            var configuration = _rabbitmqConfigurationBuilder.CreateConfiguration(hostname, port, topic);

            // Assert that params are correctly configured
            Assert.That(hostname, Is.EqualTo(configuration.Hostname));
            Assert.That(int.Parse(port), Is.EqualTo(configuration.Port));
            Assert.That(topic, Is.EqualTo(configuration.Topic));
        }

        [Test]
        public void CreateConfiguration_HostnameNull_ThrowsConfigurationException()
        {
            string? hostname = null;
            var port = "123";
            var topic = "a_topic";

            var ex = Assert.Throws<ConfigurationException>(() => _rabbitmqConfigurationBuilder.CreateConfiguration(hostname, port, topic));
        }

        [Test]
        public void CreateConfiguration_PortNull_ThrowsConfigurationException()
        {
            var hostname = "a_hostname";
            string? port = null;
            var topic = "a_topic";

            var ex = Assert.Throws<ConfigurationException>(() => _rabbitmqConfigurationBuilder.CreateConfiguration(hostname, port, topic));
        }

        [Test]
        public void CreateConfiguration_TopicNull_ThrowsConfigurationException()
        {
            var hostname = "a_hostname";
            var port = "123";
            string? topic = null;

            var ex = Assert.Throws<ConfigurationException>(() => _rabbitmqConfigurationBuilder.CreateConfiguration(hostname, port, topic));
        }
    }
}

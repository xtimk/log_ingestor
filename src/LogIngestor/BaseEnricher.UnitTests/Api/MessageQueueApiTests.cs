using BaseEnricher.Configurations;
using BaseEnricher.Controllers;
using BaseEnricher.Services.JsonSerializer;
using BaseEnricher.Services.MessageService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework.Constraints;

namespace BaseEnricher.UnitTests.Api
{
    public class MessageQueueApiTests
    {
        private Mock<ILogger<MessageQueueController>> _logger;
        private Mock<IMessageProducer<BaseLogMessage>> _messageProducer;
        private IServiceProvider _serviceProvider = default!;
        private Mock<IMessageBrokerSingletonConfiguration<RabbitMQProducerConfiguration>> _brokerConfiguration;
        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<MessageQueueController>>();
            _messageProducer = new Mock<IMessageProducer<BaseLogMessage>>();
            _brokerConfiguration = new Mock<IMessageBrokerSingletonConfiguration<RabbitMQProducerConfiguration>>();
        }

        [Test]
        public void MessageQueueSend_HostnameNotNull_ReturnsOkWithMessage()
        {
            // arrange
            IServiceCollection services = new ServiceCollection();

            _messageProducer.Setup(x => x.WriteToQueue(It.IsAny<string>(), It.IsAny<BaseLogMessage>())).Returns(true);
            _messageProducer.Setup(x => x.Configure(It.IsAny<string>()));
            services.AddSingleton(x => _messageProducer.Object);

            _brokerConfiguration.Setup(x => x.Hostname).Returns("a_hostname");
            _brokerConfiguration.Setup(x => x.Port).Returns(0);
            _brokerConfiguration.Setup(x => x.Topic).Returns("a_topic");
            services.AddSingleton(x => _brokerConfiguration.Object);

            _serviceProvider = services.BuildServiceProvider();

            var jsonSerializerMock = new Mock<IJsonSerializer<BaseLogMessage>>();
            jsonSerializerMock.Setup(x => x.Serialize(It.IsAny<BaseLogMessage>())).Returns("a_serialized_fake");

            var messageQueueApi = new MessageQueueController(_logger.Object, _serviceProvider, jsonSerializerMock.Object);

            // act
            var action = messageQueueApi.Send("a message");
            var okResult = action as OkObjectResult;

            // assert
            Assert.That(okResult, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(okResult.StatusCode, Is.EqualTo(200));
                Assert.That(okResult.Value, Is.EqualTo("a message"));
            });
        }

        //[Test]
        //public void MessageQueueSend_HostnameNull_ReturnsBadRequest()
        //{
        //    // arrange
        //    IServiceCollection services = new ServiceCollection();

        //    _messageProducer.Setup(x => x.WriteToQueue(It.IsAny<string>(), It.IsAny<BaseLogMessage>())).Returns(true);
        //    _messageProducer.Setup(x => x.Configure(It.IsAny<string>()));
        //    services.AddSingleton(x => _messageProducer.Object);

        //    _brokerConfiguration.Setup(x => x.Hostname).Returns((string?)null);
        //    _brokerConfiguration.Setup(x => x.Port).Returns(0);
        //    _brokerConfiguration.Setup(x => x.Topic).Returns("a_topic");
        //    services.AddSingleton(x => _brokerConfiguration.Object);

        //    _serviceProvider = services.BuildServiceProvider();

        //    var messageQueueApi = new MessageQueueController(_logger.Object, _serviceProvider);

        //    // act
        //    var action = messageQueueApi.Send("a message");
        //    var badRequest = action as BadRequestObjectResult;

        //    // assert
        //    Assert.That(badRequest, Is.Not.Null);
        //    Assert.Multiple(() =>
        //    {
        //        Assert.That(badRequest.StatusCode, Is.EqualTo(400));
        //        Assert.That(badRequest.Value, Is.EqualTo("API: can't retrieve hostname of queue broker from env variable"));
        //    });
        //}
    }
}

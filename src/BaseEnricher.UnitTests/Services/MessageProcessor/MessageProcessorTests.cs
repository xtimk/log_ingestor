using BaseEnricher.Models;
using BaseEnricher.Services.DateTimeProvider;
using BaseEnricher.Services.MessageProcessor;
using BaseEnricher.Services.MessageProcessor.Commands;
using Microsoft.Extensions.Logging;
using Moq;

namespace BaseEnricher.UnitTests.Services.MessageProcessor
{
    public class MessageProcessorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void AddDateProcessCommand_AddsCorrectDateTimeNow()
        {
            // Setup datetime provider
            var dateTime = new DateTime(2023, 02, 05);
            var dateTimeProvider= new Mock<IDateTimeNowProvider>();
            dateTimeProvider.Setup(x => x.Now).Returns(dateTime);

            // Setup AddDateProcessCommand
            var loggerMock = new Mock<ILogger<AddDateProcessCommand>>();
            var addDateProcessCommand = new AddDateProcessCommand(loggerMock.Object, dateTimeProvider.Object);
            var addDateCommand = new MessageProcessor<EnrichedLogMessage, BaseLogMessage>(addDateProcessCommand);
            var baseMessage = new BaseLogMessage();

            // Execute AddDateProcessCommand
            var enrichedMessage = addDateCommand.Execute(baseMessage);

            // Assert
            Assert.That(enrichedMessage.LogEnricherAcquireTime, Is.EqualTo(dateTime));
        }
    }
}

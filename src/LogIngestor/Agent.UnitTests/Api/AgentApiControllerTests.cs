using Moq;
using Agent.Configurations;
using Agent.Controllers;
using Agent.Services.MessageService;
using Microsoft.Extensions.Logging;
using Agent.Models;
using Microsoft.Extensions.Options;
using Agent.Services.Readers.Objects;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.Extensions.DependencyInjection;
using Agent.Services.GuidProvider;
using Agent.Services.GuidProvider.Impl;
using Agent.Services.MetricsService;

namespace Agent.UnitTests.Api
{
    internal class AgentApiControllerTests
    {
        private Mock<ILogger<AgentApiController>> logger;
        private Mock<IMessageProducer<BaseLogMessage>> messageProducer;
        private Mock<IOptions<LogIngestorServer>> logIngestorServer;
        private Mock<Dictionary<Guid, IReader>> activeReaders;
        private Mock<IGuidProvider> guidProvider;
        private Mock<IMetricsService> metricsService;
        private Mock<IServiceProvider> serviceProvider;

        [SetUp]
        public void Setup()
        {
            logger = new Mock<ILogger<AgentApiController>>();
            messageProducer= new Mock<IMessageProducer<BaseLogMessage>>();
            logIngestorServer = new Mock<IOptions<LogIngestorServer>>();
            activeReaders = new Mock<Dictionary<Guid, IReader>>();
            guidProvider = new Mock<IGuidProvider>();
            metricsService = new Mock<IMetricsService>();
            serviceProvider = new Mock<IServiceProvider>();
        }

        [Test]
        public void StartFakeReader_ReturnsOk()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging();
            var buildedServices = services.BuildServiceProvider();
            var fakeGuid = new Guid("e56ad0c4-d339-4d98-b384-e5ab4d22034c");
            guidProvider.Setup(x => x.Create()).Returns(fakeGuid);
            var apiController = new AgentApiController(logger.Object, messageProducer.Object, logIngestorServer.Object, activeReaders.Object, guidProvider.Object, metricsService.Object, buildedServices);
            
            var returnValue = apiController.StartFakeReader();

            var okResult = returnValue as OkObjectResult;

            Assert.That(okResult, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(okResult.StatusCode, Is.EqualTo(200));
                Assert.That(okResult.Value, Is.EqualTo($"Started reader. Guid: {fakeGuid}"));
            });

            Assert.Pass();
        }
    }
}

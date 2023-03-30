using BaseEnricher.Constants;
using BaseEnricher.Models;
using BaseEnricher.Services.MessageBackgroundProcessor;
using BaseEnricher.Services.MessageBackgroundProcessor.Impl;
using BaseEnricher.Services.MessageProcessor;
using BaseEnricher.Services.MessageProcessor.Impl;
using BaseEnricher.Services.MessageService;
using BaseEnricher.Services.MessageService.Impl;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace BaseEnricher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add serilog
            var logger = new LoggerConfiguration()
              .ReadFrom.Configuration(builder.Configuration)
              .Enrich.FromLogContext()
              .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add classes to use to pub/sub from external queues
            builder.Services.AddSingleton(typeof(IMessageProducer<>), typeof(RabbitMQProducer<>));
            builder.Services.AddSingleton(typeof(IMessageConsumer<>), typeof(RabbitMQConsumer<>));

            // Add service that processes messages, adding information
            //builder.Services.AddSingleton<IMessageProcessor<BaseLogMessage>, MessageEnricherProcessor>();

            // Add main service of this microservice:
            // Reads from queue -> Add some informations -> Publish to Queue
            builder.Services.AddSingleton<IMessageProcessorBackground, MessageProcessor>();            
            builder.Services.AddHostedService(sp => sp.GetRequiredService<IMessageProcessorBackground>());

            var app = builder.Build();

            ConfigureMessageProcessorBackground(app);

            // use this message producer just to test adding messages to the in queue, by using /MessageQueue/Send API
            //ConfigureMessageProducerForTestingWithApi(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
        //private static void ConfigureMessageProducerForTestingWithApi(WebApplication app)
        //{
        //    var logger = app.Services.GetRequiredService<ILogger<Program>>();
        //    var messageProducer = app.Services.GetRequiredService<IMessageProducer<BaseLogMessage>>();
        //    var hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.ENV_RABBITMQ_IN_HOSTNAME);
        //    if (hostname == null)
        //    {
        //        logger.LogCritical("RabbitMQ instance not specified as env variable");
        //        throw new MissingFieldException(nameof(hostname));
        //    }
        //    messageProducer.Configure(hostname);
        //}
        private static void ConfigureMessageProcessorBackground(WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            var messageProcessor = app.Services.GetRequiredService<IMessageProcessorBackground>();
            var hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.ENV_RABBITMQ_IN_HOSTNAME);
            var topic = QueueNames.QUEUE_BASE_MESSAGE_READ;
            if (hostname == null)
            {
                logger.LogCritical("RabbitMQ instance not specified as env variable");
                throw new MissingFieldException(nameof(hostname));
            }
            messageProcessor.Configure(hostname, topic);
        }
    }
}
using BaseEnricher.Constants;
using BaseEnricher.Models;
using BaseEnricher.Services.MessageBackgroundProcessor;
using BaseEnricher.Services.MessageBackgroundProcessor.Impl;
using BaseEnricher.Services.MessageProcessor;
using BaseEnricher.Services.MessageProcessor.Impl;
using BaseEnricher.Services.MessageService;
using BaseEnricher.Services.MessageService.Impl;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BaseEnricher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // adjust here, dont use generics, the serialization must be taken outside rabbitmq implementation..
            builder.Services.AddSingleton(typeof(IMessageProducer<>), typeof(RabbitMQProducer<>));
            builder.Services.AddSingleton<IMessageConsumer, RabbitMQConsumer>();
            
            //builder.Services.AddSingleton(typeof(IMessageProcessorBackground<>), typeof(MessageProcessor<>));
            builder.Services.AddSingleton<IMessageProcessorBackground, MessageProcessor>();

            builder.Services.AddSingleton<IMessageProcessor<BaseLogMessage>, MessageEnricherProcessor>();
            

            builder.Services.AddHostedService(sp => sp.GetRequiredService<IMessageProcessorBackground>());
            //builder.Services.AddHostedService(sp => sp.GetRequiredService<IMessageProcessorBackground<BaseLogMessage>>());

            var app = builder.Build();

            ConfigureMessageProcessorBackground(app);
            //ConfigureMessageProducer(app);
            //ConfigureMessageConsumer(app);

            // use this message producer just to test adding messages to the in queue, by using /MessageQueue/Send API
            ConfigureMessageProducerForTestingWithApi(app);

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

        private static void ConfigureMessageProducer(WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            var messageProducer = app.Services.GetRequiredService<IMessageProducer<EnrichedLogMessage>>();
            var hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.ENV_RABBITMQ_OUT_HOSTNAME);
            if (hostname == null)
            {
                logger.LogCritical("RabbitMQ instance not specified as env variable");
                throw new MissingFieldException(nameof(hostname));
            }
            messageProducer.Configure(hostname);
        }
        private static void ConfigureMessageProducerForTestingWithApi(WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            var messageProducer = app.Services.GetRequiredService<IMessageProducer<BaseLogMessage>>();
            var hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.ENV_RABBITMQ_IN_HOSTNAME);
            if (hostname == null)
            {
                logger.LogCritical("RabbitMQ instance not specified as env variable");
                throw new MissingFieldException(nameof(hostname));
            }
            messageProducer.Configure(hostname);
        }
        private static void ConfigureMessageProcessorBackground(WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            var consumer = app.Services.GetRequiredService<IMessageProcessorBackground>();
            var hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.ENV_RABBITMQ_IN_HOSTNAME);
            var topic = QueueNames.QUEUE_BASE_MESSAGE_READ;
            if (hostname == null)
            {
                logger.LogCritical("RabbitMQ instance not specified as env variable");
                throw new MissingFieldException(nameof(hostname));
            }
            consumer.Configure(hostname, topic);
        }
        private static void ConfigureMessageConsumer(WebApplication app)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            var consumer = app.Services.GetRequiredService<IMessageConsumer>();
            var hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstants.ENV_RABBITMQ_IN_HOSTNAME);
            var topic = QueueNames.QUEUE_BASE_MESSAGE_READ;
            if (hostname == null)
            {
                logger.LogCritical("RabbitMQ instance not specified as env variable");
                throw new MissingFieldException(nameof(hostname));
            }
            consumer.Configure(hostname);
        }
    }
}
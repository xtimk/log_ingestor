using BaseEnricher.Configurations;
using BaseEnricher.Constants;
using BaseEnricher.Services.ConfigurationBuilder;
using BaseEnricher.Services.DateTimeProvider;
using BaseEnricher.Services.MessageBackgroundProcessor;
using BaseEnricher.Services.MessageBrokerConfigurationBuilder.Impl;
using BaseEnricher.Services.MessageProcessor.Commands;
using BaseEnricher.Services.MessageService;
using BaseEnricher.Services.MessageService.Impl;
using Microsoft.OpenApi.Models;
using Serilog;

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
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo() { Title = "Base Log Enricher", Version = "v1" });
            });
            // Add commands to process messages
            builder.Services.AddScoped<AddDateProcessCommand>();

            // Add singleton that provides current time. This is needed in order to unit test things correctly
            builder.Services.AddSingleton<IDateTimeNowProvider, DateTimeNowProvider>();

            // Add singletons containing IMessageBrokerConfigurations for the producer and the consumer
            AddMessageBrokerConfigurationsAsSingletons(builder);

            // Add message broker configuration builder, so that i can build configuration everywhere. Not used yet.
            builder.Services.AddScoped<IMessageBrokerConfigurationBuilder, RabbitMQConfigurationBuilder>();

            // Add RabbitMQProducer and RabbitMQConsumer classes
            builder.Services.AddScoped(typeof(IMessageProducer<>), typeof(RabbitMQProducer<>));
            builder.Services.AddScoped(typeof(IMessageConsumer<>), typeof(RabbitMQConsumer<>));

            // Add main service of this microservice
            // Reads from queue -> Add some informations -> Publish to Queue
            builder.Services.AddSingleton<IMessageProcessorBackground, MessageProcessor>();            
            builder.Services.AddHostedService(sp => sp.GetRequiredService<IMessageProcessorBackground>());

            var app = builder.Build();

            // Configure the main service of this microservice
            ConfigureMessageProcessorBackground(app);

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
        private static void AddMessageBrokerConfigurationsAsSingletons(WebApplicationBuilder builder)
        {
            var in_messageBrokerConfigurationBuilder = new RabbitMQConfigurationBuilder();
            var in_broker_hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_IN_HOSTNAME);
            var in_broker_port_string = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_IN_PORT);
            var in_broker_topic = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_IN_TOPIC);
            var inMessageBrokerConf = in_messageBrokerConfigurationBuilder.CreateConfiguration(in_broker_hostname, in_broker_port_string, in_broker_topic);
            builder.Services.AddSingleton<IMessageBrokerSingletonConfiguration<RabbitMQConsumerConfiguration>>(new RabbitMQConsumerConfiguration(inMessageBrokerConf));

            var out_messageBrokerConfigurationBuilder = new RabbitMQConfigurationBuilder();
            var out_broker_hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_OUT_HOSTNAME);
            var out_broker_port = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_OUT_PORT);
            var out_broker_topic = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_OUT_TOPIC);
            var outMessageBrokerConf = out_messageBrokerConfigurationBuilder.CreateConfiguration(out_broker_hostname, out_broker_port, out_broker_topic);
            builder.Services.AddSingleton<IMessageBrokerSingletonConfiguration<RabbitMQProducerConfiguration>>(new RabbitMQProducerConfiguration(outMessageBrokerConf));

        }

        private static void ConfigureMessageProcessorBackground(WebApplication app)
        {
            var inMessageBrokerConf = app.Services.GetRequiredService<IMessageBrokerSingletonConfiguration<RabbitMQConsumerConfiguration>>();
            var outMessageBrokerConf = app.Services.GetRequiredService<IMessageBrokerSingletonConfiguration<RabbitMQProducerConfiguration>>();
            var messageProcessor = app.Services.GetRequiredService<IMessageProcessorBackground>();
            messageProcessor.Configure(inMessageBrokerConf, outMessageBrokerConf);
        }
    }
}
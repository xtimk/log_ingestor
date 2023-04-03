using FS.Services.MessageBrokerConfigurationBuilder.Impl;
using FSWriter.Configurations;
using FSWriter.Constants;
using FSWriter.Services.DateTimeProvider;
using FSWriter.Services.FileWriter;
using FSWriter.Services.FileWriter.Impl;
using FSWriter.Services.JsonSerializer;
using FSWriter.Services.JsonSerializer.Impl;
using FSWriter.Services.MessageBackgroundProcessor;
using FSWriter.Services.MessageBrokerConfigurationBuilder.Impl;
using FSWriter.Services.MessageService;
using FSWriter.Services.MessageService.Impl;
using Microsoft.OpenApi.Models;
using Serilog;

namespace FSWriter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var logger = new LoggerConfiguration()
              .ReadFrom.Configuration(builder.Configuration)
              .Enrich.FromLogContext()
              .CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();
            builder.Services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo() { Title = "Storage filesystem writer", Version = "v1" });
            });

            builder.Services.AddSingleton<IDateTimeNowProvider, DateTimeNowProvider>();

            builder.Services.AddSingleton(typeof(IJsonSerializer<>), typeof(SystemTextJsonSerializer<>));

            builder.Services.AddSingleton<IMessageProcessorBackground, MessageProcessor>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<IMessageProcessorBackground>());

            ConfigureMessageBrokerConfigurations(builder);

            //// Add classes to use to pub/sub from external queues
            //builder.Services.AddSingleton(typeof(IMessageProducer<>), typeof(RabbitMQProducer<>));
            builder.Services.AddSingleton(typeof(IMessageConsumer<>), typeof(RabbitMQConsumer<>));
            builder.Services.AddSingleton(typeof(IFileWriter<>), typeof(FileStreamWriter<>));


            var app = builder.Build();

            ConfigureMessageProcessorBackground(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
        private static void ConfigureMessageBrokerConfigurations(WebApplicationBuilder builder)
        {
            var in_broker_configurationBuilder = new RabbitMQConfigurationBuilder();
            var in_broker_hostname = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_IN_HOSTNAME);
            var in_broker_port_string = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_IN_PORT);
            var in_broker_topic = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.ENV_RABBITMQ_IN_TOPIC);
            var in_broker_configuration = in_broker_configurationBuilder.CreateConfiguration(in_broker_hostname, in_broker_port_string, in_broker_topic);
            builder.Services.AddSingleton<IMessageBrokerSingletonConfiguration<RabbitMQConsumerConfiguration>>(new RabbitMQConsumerConfiguration(in_broker_configuration));

            var out_fs_storage_configurationBuilder = new FileStreamWriterConfigurationBuilder();
            var out_fs_storage_basePath = Environment.GetEnvironmentVariable(ConfigurationKeyConstant.DATA_STORAGE_BASEPATH);
            var out_fs_storage_configuration = out_fs_storage_configurationBuilder.CreateConfiguration(out_fs_storage_basePath);
            builder.Services.AddSingleton<IFileWriterSingletonConfiguration<FileWriterSingletonConfiguration>>(new FileWriterSingletonConfiguration(out_fs_storage_configuration));
        }

        private static void ConfigureMessageProcessorBackground(WebApplication app)
        {
            var inMessageBrokerConf = app.Services.GetRequiredService<IMessageBrokerSingletonConfiguration<RabbitMQConsumerConfiguration>>();
            var outStorageFsConf = app.Services.GetRequiredService<IFileWriterSingletonConfiguration<FileWriterSingletonConfiguration>>();
            var messageProcessor = app.Services.GetRequiredService<IMessageProcessorBackground>();
            messageProcessor.Configure(inMessageBrokerConf, outStorageFsConf);
        }

    }
}
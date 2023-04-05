using Agent.Configurations;
using Agent.Models;
using Agent.Services.GuidProvider;
using Agent.Services.GuidProvider.Impl;
using Agent.Services.JsonSerializer;
using Agent.Services.JsonSerializer.Impl;
using Agent.Services.MessageService;
using Agent.Services.MessageService.Impl;
using Agent.Services.MetricsService;
using Agent.Services.MetricsService.Impl;
using Agent.Services.Readers.Objects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Prometheus;
using Serilog;

namespace Agent
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

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.Services.AddSingleton<LogIngestorServer>();
            builder.Services.Configure<LogIngestorServer>(builder.Configuration.GetSection("LogIngestorServer"));

            // List of ireaders (threads). Use this to eventually stop readers.
            builder.Services.AddSingleton<Dictionary<Guid, IReader>>();

            builder.Services.AddSingleton<IGuidProvider, GuidProvider>();

            builder.Services.AddSingleton<IMetricsService, MetricsService>();

            //builder.Services.AddSingleton(typeof(IMessageProducer<>), typeof(RabbitMQProducer<>));
            builder.Services.AddSingleton(typeof(IMessageProducer<>), typeof(KafkaProducer<>));
            //builder.Services.AddScoped(typeof(IMessageConsumer<>), typeof(RabbitMQConsumer<>));
            builder.Services.AddSingleton(typeof(IJsonSerializer<>), typeof(SystemTextJsonSerializer<>));


            var app = builder.Build();

            var logIngestorServer = app.Services.GetRequiredService<IOptions<LogIngestorServer>>();

            var messageProducer = app.Services.GetRequiredService<IMessageProducer<BaseLogMessage>>();
            messageProducer.Configure(logIngestorServer.Value.Host, logIngestorServer.Value.Port);
            var messageProducerString = app.Services.GetRequiredService<IMessageProducer<string>>();
            messageProducerString.Configure(logIngestorServer.Value.Host, logIngestorServer.Value.Port);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseMetricServer();
            app.UseHttpMetrics();

            app.MapControllers();

            app.Run();
        }
    }
}
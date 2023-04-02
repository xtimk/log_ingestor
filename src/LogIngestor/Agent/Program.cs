using Agent.Configurations;
using Agent.Models;
using Agent.Services.MessageService;
using Agent.Services.MessageService.Impl;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Agent
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

            //builder.Services.AddSingleton<LogIngestorServer>();
            builder.Services.Configure<LogIngestorServer>(builder.Configuration.GetSection("LogIngestorServer"));

            builder.Services.AddSingleton(typeof(IMessageProducer<>), typeof(RabbitMQProducer<>));
            //builder.Services.AddScoped(typeof(IMessageConsumer<>), typeof(RabbitMQConsumer<>));

            var app = builder.Build();

            var logIngestorServer = app.Services.GetRequiredService<IOptions<LogIngestorServer>>();
            var messageProducer = app.Services.GetRequiredService<IMessageProducer<BaseLogMessage>>();
            messageProducer.Configure(logIngestorServer.Value.Host);

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
    }
}
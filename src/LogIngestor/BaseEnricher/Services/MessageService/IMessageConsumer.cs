﻿using BaseEnricher.Models;
using RabbitMQ.Client.Events;

namespace BaseEnricher.Services.MessageService
{
    public interface IMessageConsumer<T>
    {
        void Subscribe(string topic);
        void Configure(string hostname);

        event EventHandler<T> OnMessageReceived;
    }
}

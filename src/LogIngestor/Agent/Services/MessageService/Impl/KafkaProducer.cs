﻿using Agent.Services.JsonSerializer;
using Agent.Services.MetricsService;
using Confluent.Kafka;

namespace Agent.Services.MessageService.Impl
{
    public class KafkaProducer<T> : IMessageProducer<T>
    {
        private readonly ILogger<KafkaProducer<T>> _logger;
        private readonly IJsonSerializer<T> _jsonSerializer;
        private readonly IMetricsService _metricsService;
        private ProducerConfig _config;
        private IProducer<Null, string> _producerBuilder;
        private int counter = 0;
        private Guid _service_guid;
        private string _baseLogMessage;

        public event Action<string> OnPublish;

        public KafkaProducer(ILogger<KafkaProducer<T>> logger, IJsonSerializer<T> jsonSerializer, IMetricsService metricsService)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _metricsService = metricsService;
            _service_guid = Guid.NewGuid();
            _baseLogMessage = $"Kafka Producer[{_service_guid}]: ";
            _logger.LogInformation($"{_baseLogMessage}Created. Unique id: {_service_guid}");
        }

        public void Configure(string hostname, string port)
        {
            _config = new ProducerConfig { BootstrapServers = $"{hostname}:{port}", BatchSize = 100000, LingerMs = 200 };
            _producerBuilder = new ProducerBuilder<Null, string>(_config).Build();
            _logger.LogInformation($"{_baseLogMessage}Configured with host {hostname}:{port}");
        }

        public void PublishBatch(string topic, IList<T> messages)
        {
            throw new NotImplementedException("Not implemented yet with Kafka");
            //try
            //{
            //    var messageList = messages.ToList();
            //    foreach (var message in messageList)
            //    {
            //        var serializedMessage = _jsonSerializer.Serialize(message);
            //        _producerBuilder.Produce(topic, new Message<Null, string> { Value = serializedMessage });
            //    }
            //    counter += messageList.Count;
            //    _logger.LogInformation($"Delivered batch of {messageList.Count} messages. Total {counter}");
            //    _producerBuilder.Flush();
            //    OnPublish.Invoke();
            //}
            //catch (ProduceException<Null, string> e)
            //{
            //    _logger.LogError($"Delivery failed: {e.Error.Reason}");
            //}
        }

        public bool Publish(string topic, T message)
        {
            try
            {
                var serializedMessage = _jsonSerializer.Serialize(message);
                _producerBuilder.Produce(topic, new Message<Null, string> { Value = serializedMessage });
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"{_baseLogMessage}Delivery failed: {e.Error.Reason}");
            }

            OnPublish?.Invoke(_service_guid.ToString());
            _metricsService.SignalNewEvent(_service_guid.ToString(), "main.service.out");
            return true;
        }
    }
}

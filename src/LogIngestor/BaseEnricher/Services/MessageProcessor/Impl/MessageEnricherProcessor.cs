using BaseEnricher.Constants;
using BaseEnricher.Models;
using BaseEnricher.Services.MessageProcessor.Commands;
using BaseEnricher.Services.MessageProcessor.Commands.Impl;
using BaseEnricher.Services.MessageService;
using System.Runtime.Serialization;

namespace BaseEnricher.Services.MessageProcessor.Impl
{
    public class MessageEnricherProcessor : IMessageProcessor<BaseLogMessage>
    {
        private readonly ILogger<MessageEnricherProcessor> _logger;
        private readonly IMessageProducer<EnrichedLogMessage> _messageProducer;

        public MessageEnricherProcessor(
                ILogger<MessageEnricherProcessor> logger, 
                IMessageProducer<EnrichedLogMessage> messageProducer)
        {
            _logger = logger;
            _messageProducer = messageProducer;
        }

        public void Process(BaseLogMessage message)
        {
            try
            {
                var addDateCommand = new MessageProcessor<EnrichedLogMessage, BaseLogMessage>(new AddDateProcessCommand());

                _logger.LogDebug($"Processing message: {message}");
                var enrichedMessage = addDateCommand.Execute(message);

                _messageProducer.WriteToQueue(QueueNames.QUEUE_ENRICHED_MESSAGE_WRITE, enrichedMessage);
                //throw new Exception("Intended ex");
            }
            catch (Exception ex)
            {
                throw new MessageProcessorException(ex.Message, ex);
            }
        }
    }

    public class MessageProcessorException : Exception
    {
        public MessageProcessorException()
        {
        }

        public MessageProcessorException(string? message) : base(message)
        {
        }

        public MessageProcessorException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MessageProcessorException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

using FSWriter.Models;
using FSWriter.Services.DateTimeProvider;

namespace FSWriter.Services.MessageProcessor.Commands
{
    public class AddDateProcessCommand : ProcessCommandStrategy<FSLogMessage, EnrichedLogMessage>
    {
        private readonly ILogger<AddDateProcessCommand> _logger;
        private readonly IDateTimeNowProvider _dateTimeNowProvider;

        public AddDateProcessCommand(ILogger<AddDateProcessCommand> logger, IDateTimeNowProvider dateTimeNowProvider)
        {
            _logger = logger;
            _dateTimeNowProvider = dateTimeNowProvider;
        }

        //public override FSLogMessage EnrichedLogMessage(EnrichedLogMessage message)
        //{
        //    var result = message.CastToEnrichedLogMessage();
        //    result.LogEnricherAcquireTime = _dateTimeNowProvider.Now;
        //    return result;
        //}

        public override FSLogMessage Execute(EnrichedLogMessage message)
        {
            throw new NotImplementedException();
        }
    }
}

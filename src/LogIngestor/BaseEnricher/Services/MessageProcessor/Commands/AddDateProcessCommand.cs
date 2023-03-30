using BaseEnricher.Models;
using BaseEnricher.Services.DateTimeProvider;

namespace BaseEnricher.Services.MessageProcessor.Commands
{
    public class AddDateProcessCommand : ProcessCommandStrategy<EnrichedLogMessage, BaseLogMessage>
    {
        private readonly ILogger<AddDateProcessCommand> _logger;
        private readonly IDateTimeNowProvider _dateTimeNowProvider;

        public AddDateProcessCommand(ILogger<AddDateProcessCommand> logger, IDateTimeNowProvider dateTimeNowProvider)
        {
            _logger = logger;
            _dateTimeNowProvider = dateTimeNowProvider;
        }

        public override EnrichedLogMessage Execute(BaseLogMessage message)
        {
            var result = message.CastToEnrichedLogMessage();
            result.LogEnricherAcquireTime = _dateTimeNowProvider.Now;
            return result;
        }
    }
}

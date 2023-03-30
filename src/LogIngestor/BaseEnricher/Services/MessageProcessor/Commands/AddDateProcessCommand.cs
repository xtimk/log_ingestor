using BaseEnricher.Models;

namespace BaseEnricher.Services.MessageProcessor.Commands
{
    public class AddDateProcessCommand : ProcessCommandStrategy<EnrichedLogMessage, BaseLogMessage>
    {
        public override EnrichedLogMessage Execute(BaseLogMessage message)
        {
            var result = message.CastToEnrichedLogMessage();
            result.LogEnricherAcquireTime = DateTime.Now;
            return result;
        }
    }
}

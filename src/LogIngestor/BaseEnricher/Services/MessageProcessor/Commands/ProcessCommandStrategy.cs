namespace BaseEnricher.Services.MessageProcessor.Commands
{
    public abstract class ProcessCommandStrategy<T, U>
    {
        public abstract T Execute(U message);
    }
}

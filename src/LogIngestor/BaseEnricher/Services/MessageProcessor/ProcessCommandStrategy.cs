namespace BaseEnricher.Services.MessageProcessor
{
    public abstract class ProcessCommandStrategy<T, U>
    {
        public abstract T Execute(U message);
    }
}

namespace FSWriter.Services.MessageProcessor
{
    public class MessageProcessor<T, U>
    {
        private ProcessCommandStrategy<T, U> _processCommandStrategy;

        public MessageProcessor(ProcessCommandStrategy<T, U> processCommandStrategy)
        {
            _processCommandStrategy = processCommandStrategy;
        }

        public T Execute(U message)
        {
            return _processCommandStrategy.Execute(message);
        }
    }
}

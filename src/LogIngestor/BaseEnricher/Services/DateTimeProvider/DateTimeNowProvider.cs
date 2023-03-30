namespace BaseEnricher.Services.DateTimeProvider
{
    public class DateTimeNowProvider : IDateTimeNowProvider
    {
        public DateTime Now => DateTime.Now;
    }
}

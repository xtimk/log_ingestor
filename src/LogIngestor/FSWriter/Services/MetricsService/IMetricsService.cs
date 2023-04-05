namespace FSWriter.Services.MetricsService
{
    public interface IMetricsService
    {
        void SignalNewEvent(string guid, string name, int number_of_events=1);
        //void SignalNewEvent(string guid, string name, int number_of_events);
    }
}

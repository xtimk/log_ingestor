using FSWriter.Services.JsonSerializer;
using FSWriter.Services.MetricsService;
using System.Text.Json;

namespace FSWriter.Services.FileWriter.Impl
{
    public class FileStreamWriter<T> : IFileWriter<T>
    {
        private readonly ILogger<FileStreamWriter<T>> _logger;
        private readonly IJsonSerializer<T> _jsonSerializer;
        private readonly IMetricsService _metricsService;
        private readonly Guid _service_guid;
        private readonly string _baseLogMessage;

        public FileStreamWriter(ILogger<FileStreamWriter<T>> logger, IJsonSerializer<T> jsonSerializer, IMetricsService metricsService)
        {
            _logger = logger;
            _jsonSerializer = jsonSerializer;
            _metricsService = metricsService;
            _service_guid = Guid.NewGuid();
            _baseLogMessage = $"FS Writer[{_service_guid}]: ";
        }

        public void AppendToFile(string path, T message)
        {
            try
            {
                var serializedMessage = _jsonSerializer.Serialize(message);
                using StreamWriter sw = File.AppendText(path);
                sw.WriteLine(serializedMessage);
                _metricsService.SignalNewEvent(_service_guid.ToString(), "main.service.out");
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError("Error while serializing message before disk write", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while saving log to disk", ex);
            }
        }
    }
}

﻿using System.Text.Json;

namespace FSWriter.Services.FileWriter.Impl
{
    public class FileStreamWriter<T> : IFileWriter<T>
    {
        private readonly ILogger<FileStreamWriter<T>> _logger;

        public FileStreamWriter(ILogger<FileStreamWriter<T>> logger)
        {
            _logger = logger;
        }

        public void AppendToFile(string path, T message)
        {
            try
            {
                var serializedMessage = JsonSerializer.Serialize(message);
                using StreamWriter sw = File.AppendText(path);
                sw.WriteLine(serializedMessage);
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
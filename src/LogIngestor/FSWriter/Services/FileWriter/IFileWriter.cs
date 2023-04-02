namespace FSWriter.Services.FileWriter
{
    public interface IFileWriter<T>
    {
        void AppendToFile(string path, T message);
    }
}

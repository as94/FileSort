namespace Core.Sorting.IO;

public sealed class FileLineReader : ILineReader<LineRecord>
{
    private readonly Defaults _defaults;

    public FileLineReader(Defaults defaults)
    {
        _defaults = defaults;
    }

    public IEnumerable<LineRecord> Read(string path)
    {
        using var reader = new StreamReader(
            new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                _defaults.FileBufferSize));

        while (reader.ReadLine() is { } line)
        {
            yield return LineRecord.Parse(line);
        }
    }
}
namespace Sorter.IO;

public sealed class FileLineWriter : ILineWriter<LineRecord>
{
    private readonly Defaults _defaults;

    public FileLineWriter(Defaults defaults)
    {
        _defaults = defaults;
    }

    public void Write(string path, IEnumerable<LineRecord> records)
    {
        using var writer = new StreamWriter(
            new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None,
                _defaults.FileBufferSize));

        foreach (var r in records)
        {
            writer.WriteLine(r.ToString());
        }
    }
}
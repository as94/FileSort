using System.Text;

namespace Core.Generating;

public sealed class FileGenerator
{
    private readonly Random _random = new();

    public void GenerateFile(string outputFile, long sizeInBytes)
    {
        var directory = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fs =
            new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(fs, Encoding.UTF8);

        var newlineBytes = Encoding.UTF8.GetByteCount(writer.NewLine);
        long written = 0;

        var strings = GetStrings();
        while (written < sizeInBytes)
        {
            var number = _random.Next(1, 1_000_000);
            var text = strings[_random.Next(strings.Length)];
            var line = $"{number}. {text}";
            writer.WriteLine(line);
            written += Encoding.UTF8.GetByteCount(line) + newlineBytes;
        }
    }

    private string[] GetStrings()
    {
        const int poolSize = 10_000;

        return Enumerable.Range(0, poolSize)
            .Select(_ => new string(
                Enumerable.Range(1, _random.Next(3, 100))
                    .Select(_ => _random.Next(0, 2) == 0
                        ? (char)_random.Next('a', 'z' + 1)
                        : (char)_random.Next('A', 'Z' + 1))
                    .ToArray()))
            .ToArray();
    }
}
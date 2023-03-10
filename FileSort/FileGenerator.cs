using System.Text;

namespace FileSort;

public sealed class FileGenerator
{
    private readonly Random _random = new();

    private readonly string[] _cachedNames;

    public FileGenerator(int cacheSize = 1000_000)
    {
        _cachedNames = Enumerable.Range(0, cacheSize)
            .Select(_ => new string(
                Enumerable.Range(1, _random.Next(3, 100)).Select(_ => _random.Next(0, 2) == 0
                    ? (char)_random.Next('a', 'z' + 1)
                    : (char)_random.Next('A', 'Z' + 1)).ToArray()))
            .ToArray();
    }

    public async Task GenerateFileAsync(
        string fileName,
        long linesCount,
        CancellationToken ct = default)
    {
        await using var sw = new StreamWriter(fileName);

        for (var i = 0; i < linesCount; i++)
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            
            var id = _random.Next(0, int.MaxValue);
            var name = _cachedNames[_random.Next(0, _cachedNames.Length)];
            await sw.WriteLineAsync($"{id}. {name}");
        }
    }
}
namespace FileSort;

internal sealed class FileGenerator
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

    public void GenerateFile(string fileName, long linesCount)
    {
        using var sw = new StreamWriter(fileName);

        var repeatRandomMaxValue = linesCount / 1000_000 < 10 ? 10 : 100;
        var repeats = new List<string>(); 
        for (var i = 0; i < linesCount; i++)
        {
            var id = _random.Next(0, int.MaxValue);
            var name = _cachedNames[_random.Next(0, _cachedNames.Length)];
            
            if (_random.Next(0, repeatRandomMaxValue) == 0)
            {
                repeats.Add(name);
            }

            if (_random.Next(0, repeatRandomMaxValue * 2) == 0 && repeats.Count > 0)
            {
                name = repeats[0];
                repeats.Remove(name);
            }

            sw.WriteLine($"{id}. {name}");
        }
    }
}
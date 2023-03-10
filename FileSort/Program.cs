using System.Diagnostics;
using FileSort;


var sw = Stopwatch.StartNew();

var command = args[0];
if (command == "generate")
{
    if (!long.TryParse(args[1], out var linesCount))
    {
        Console.WriteLine("Lines count should be long");
        return;
    }

    if (linesCount < 0)
    {
        Console.WriteLine("Lines count should be positive");
        return;
    }

    var fileGenerator = new FileGenerator();

    await fileGenerator.GenerateFileAsync($"{linesCount}.txt", linesCount);
}
else if (command == "sort")
{
    var filePath = args[1];
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File '{filePath}' doesn't exist");
        return;
    }

    var fileSort = new FileSorter(filePath);
    await fileSort.SortAsync(CancellationToken.None);
}
else
{
    Console.WriteLine($"Unknown command '{command}'");
    return;
}

Console.WriteLine($"Elapsed in minutes: {sw.ElapsedMilliseconds / 1000 / 60}");
Console.WriteLine($"Elapsed in seconds: {sw.ElapsedMilliseconds / 1000}");
Console.WriteLine($"Elapsed in milliseconds: {sw.ElapsedMilliseconds}");




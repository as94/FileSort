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

    fileGenerator.GenerateFile($"{linesCount}.txt", linesCount);
}
else if (command == "sort")
{
    var filePath = args[1];
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File '{filePath}' doesn't exist");
        return;
    }

    new FileSorter(filePath).Sort();
}
else
{
    Console.WriteLine($"Unknown command '{command}'");
    return;
}

Console.WriteLine($"File name: {args[1]}");
Console.WriteLine($"Elapsed in milliseconds: {sw.ElapsedMilliseconds}");




using System.Diagnostics;
using FileSort;


var sw = Stopwatch.StartNew();

var command = args[0];
if (command == "generate")
{
    var fileSizeInMbInput = args[1];
    if (!int.TryParse(fileSizeInMbInput, out var fileSizeInMb))
    {
        Console.WriteLine("File size in MB should be integer");
        return;
    }

    if (fileSizeInMb < 0 || fileSizeInMb > 100_000)
    {
        Console.WriteLine("File size in MB should be more than 0 MB and less or equal to 100 GB");
        return;
    }

    var idGenerator = new IdGenerator();
    var nameGenerator = new NameGenerator();
    var stringGenerator = new StringGenerator(idGenerator, nameGenerator);
    var fileInGb = fileSizeInMb > 1000;
    var fileGenerator = fileInGb
        ? new FileGenerator(stringGenerator, 10_000_000)
        : new FileGenerator(stringGenerator);

    var fileName = fileInGb
        ? $"{fileSizeInMb / 1000}GB.txt"
        : $"{fileSizeInMb}MB.txt";
    await fileGenerator.GenerateFileAsync(fileName, fileSizeInMb);
}
else if (command == "sort")
{
    var filePath = args[1];
    if (!File.Exists(filePath))
    {
        Console.WriteLine($"File '{filePath}' doesn't exist");
        return;
    }

    var fileSort = new FileSort.FileSort(filePath);
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




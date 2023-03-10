using System.Diagnostics;
using FileSort;

var fileSizeInMbInput = args[0];
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
var fileGenerator = new FileGenerator(stringGenerator, 10_000_000);

var sw = Stopwatch.StartNew();
await fileGenerator.GenerateFileAsync($"{fileSizeInMb / 1000}GB.txt", fileSizeInMb);

Console.WriteLine($"Elapsed in seconds: {sw.ElapsedMilliseconds / 1000}");



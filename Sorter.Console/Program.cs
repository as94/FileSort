using Core.Sorting;
using Core.Sorting.IO;

if (args.Length < 2)
{
    Console.WriteLine("Usage: sorter <input-file> <output-file>");
    return;
}

var inputFile = args[0];
var outputFile = args[1];

if (!File.Exists(inputFile))
{
    Console.WriteLine($"Input file does not exist: {inputFile}");
    return;
}

var defaults = Defaults.Production;
var sorter = new FileSorter(defaults, new RealConsole());

await sorter.SortFileAsync(inputFile, outputFile);

Console.WriteLine($"Sorted file saved to {outputFile}");
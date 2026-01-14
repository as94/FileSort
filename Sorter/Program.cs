using System.Diagnostics;
using Sorter;

if (args.Length == 0)
{
    Console.WriteLine("Input file path is not specified");
    return;
}

var inputFile = args[0];
if (!File.Exists(inputFile))
{
    Console.WriteLine("Input file path does not exist");
}

var tempDir = "tmp";
var outputFile = "sorted.txt";

Console.WriteLine("=== External File Sort ===");
Console.WriteLine($"Input : {inputFile}");
Console.WriteLine($"Output: {outputFile}");
Console.WriteLine();

Console.WriteLine("[Phase 1] Split & Sort started...");
var sw = Stopwatch.StartNew();

var chunkSorter = new ChunkSorter(
    512L * 1024 * 1024, // 512 MB
    tempDir,
    Environment.ProcessorCount
);

var chunks = await chunkSorter.SplitAndSortAsync(inputFile);

sw.Stop();
Console.WriteLine("[Phase 1] Done");
Console.WriteLine($"-- Chunks created: {chunks.Count}");
Console.WriteLine($"-- Time: {sw.Elapsed}");
Console.WriteLine();

Console.WriteLine("[Phase 2+] Merge started...");
sw.Restart();

var merger = new ExternalMerger();
merger.Merge(chunks, outputFile);

sw.Stop();
Console.WriteLine("[Phase 2+] Done");
Console.WriteLine($"-- Time: {sw.Elapsed}");
Console.WriteLine();

Directory.Delete(tempDir, true);
Console.WriteLine("=== Finished successfully ===");
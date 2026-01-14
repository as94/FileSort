using System.Diagnostics;
using Sorter;
using Sorter.Algorithms;
using Sorter.IO;

var defaults = Defaults.Production;

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

Console.WriteLine("=== External File Sort ===");
Console.WriteLine($"Input : {inputFile}");
Console.WriteLine();

Console.WriteLine("[Phase 1] Split & Sort started...");
var sw = Stopwatch.StartNew();

var chunkSorter = new ChunkSorter(
    new ArrayChunkSortAlgorithm<LineRecord>(),
    defaults);

var chunks = await chunkSorter.SplitAndSortAsync(inputFile);

sw.Stop();
Console.WriteLine("[Phase 1] Done");
Console.WriteLine($"-- Chunks created: {chunks.Count}");
Console.WriteLine($"-- Time: {sw.Elapsed}");
Console.WriteLine();

Console.WriteLine("[Phase 2] Merge started...");
sw.Restart();

var merger = new ExternalMerger(
    new KWayMergeAlgorithm<LineRecord>(),
    new FileLineReader(defaults),
    new FileLineWriter(defaults),
    new FileTempFileProvider(defaults),
    defaults);

merger.MergeAllChunks(chunks);

sw.Stop();
Console.WriteLine("[Phase 2] Done");
Console.WriteLine($"-- Time: {sw.Elapsed}");
Console.WriteLine();

Directory.Delete(defaults.TempDir);
Console.WriteLine("=== Finished successfully ===");
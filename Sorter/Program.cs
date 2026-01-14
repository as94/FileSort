using System.Diagnostics;
using Sorter;
using Sorter.Algorithms;
using Sorter.IO;

var defaults = Defaults.Production;

Directory.CreateDirectory(defaults.TempDir);

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

var reader = new FileLineReader(defaults);
var writer = new FileLineWriter(defaults);

Console.WriteLine("[Phase 1] Split & Sort started...");
var sw = Stopwatch.StartNew();

var chunkSortAlgorithm = new ArrayChunkSortAlgorithm<LineRecord>();
var chunkSorter = new ChunkSorter(
    chunkSortAlgorithm,
    reader,
    writer,
    defaults);

var chunks = await chunkSorter.SplitAndSortAsync(inputFile);

sw.Stop();
Console.WriteLine("[Phase 1] Done");
Console.WriteLine($"-- Chunks created: {chunks.Count}");
Console.WriteLine($"-- Time: {sw.Elapsed}");
Console.WriteLine();

Console.WriteLine("[Phase 2] Merge started...");
sw.Restart();

var mergeAlgorithm = new KWayMergeAlgorithm<LineRecord>();
var tempFileProvider = new TempFileProvider(defaults);
var merger = new ExternalMerger(
    mergeAlgorithm,
    reader,
    writer,
    tempFileProvider,
    defaults);

merger.MergeAllChunks(chunks);

sw.Stop();
Console.WriteLine("[Phase 2] Done");
Console.WriteLine($"-- Time: {sw.Elapsed}");
Console.WriteLine();

Console.WriteLine("=== Finished successfully ===");
using System.Diagnostics;
using Core.Sorting.Algorithms;
using Core.Sorting.IO;

namespace Core.Sorting;

public sealed class FileSorter
{
    private readonly Defaults _defaults;

    public FileSorter(Defaults defaults)
    {
        _defaults = defaults;
    }

    public async Task SortFileAsync(string inputFile, string outputFile)
    {
        Directory.CreateDirectory(_defaults.TempDir);

        var directory = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var reader = new FileLineReader(_defaults);
        var writer = new FileLineWriter(_defaults);

        Console.WriteLine("[Phase 1] Split & Sort started...");
        var sw = Stopwatch.StartNew();

        var chunkSorter = new ChunkSorter(
            new ArrayChunkSortAlgorithm<LineRecord>(),
            reader,
            writer,
            _defaults);

        var chunks = await chunkSorter.SplitAndSortAsync(inputFile);

        sw.Stop();
        Console.WriteLine("[Phase 1] Done");
        Console.WriteLine($"-- Chunks created: {chunks.Count}");
        Console.WriteLine($"-- Time: {sw.Elapsed}");
        Console.WriteLine();

        Console.WriteLine("[Phase 2] Merge started...");
        sw.Restart();

        var mergeAlgorithm = new KWayMergeAlgorithm<LineRecord>();
        var tempProvider = new TempFileProvider(_defaults);
        var merger = new ExternalMerger(
            mergeAlgorithm,
            reader,
            writer,
            tempProvider,
            _defaults);

        merger.MergeAllChunks(chunks, outputFile);

        sw.Stop();
        Console.WriteLine("[Phase 2] Done");
        Console.WriteLine($"-- Time: {sw.Elapsed}");
        Console.WriteLine();
    }
}
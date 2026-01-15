using System.Diagnostics;
using Core.Sorting.Algorithms;
using Core.Sorting.IO;

namespace Core.Sorting;

public sealed class FileSorter
{
    private readonly IConsole _console;
    private readonly Defaults _defaults;

    public FileSorter(Defaults defaults, IConsole console)
    {
        _defaults = defaults;
        _console = console;
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

        _console.WriteLine("[Phase 1] Split & Sort started...");
        var sw = Stopwatch.StartNew();

        var chunkSorter = new ChunkSorter(
            new ArrayChunkSortAlgorithm<LineRecord>(),
            reader,
            writer,
            _defaults);

        var chunks = await chunkSorter.SplitAndSortAsync(inputFile);

        sw.Stop();
        _console.WriteLine("[Phase 1] Done");
        _console.WriteLine($"-- Chunks created: {chunks.Count}");
        _console.WriteLine($"-- Time: {sw.Elapsed}");
        _console.WriteLine();

        _console.WriteLine("[Phase 2] Merge started...");
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
        _console.WriteLine("[Phase 2] Done");
        _console.WriteLine($"-- Time: {sw.Elapsed}");
        _console.WriteLine();
    }
}
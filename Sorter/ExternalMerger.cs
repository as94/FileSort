using Sorter.Algorithms;
using Sorter.IO;

namespace Sorter;

internal sealed class ExternalMerger
{
    private readonly Defaults _defaults;
    private readonly IMergeAlgorithm<LineRecord> _mergeAlgorithm;
    private readonly ILineReader<LineRecord> _reader;
    private readonly ITempFileProvider _tempFileProvider;
    private readonly ILineWriter<LineRecord> _writer;

    public ExternalMerger(
        IMergeAlgorithm<LineRecord> mergeAlgorithm,
        ILineReader<LineRecord> reader,
        ILineWriter<LineRecord> writer,
        ITempFileProvider tempFileProvider,
        Defaults defaults)
    {
        _mergeAlgorithm = mergeAlgorithm;
        _reader = reader;
        _writer = writer;
        _tempFileProvider = tempFileProvider;
        _defaults = defaults;
    }

    public void MergeAllChunks(IReadOnlyList<string> initialChunks)
    {
        var round = 0;
        var current = initialChunks;

        while (current.Count > 1)
        {
            var nextRound = new List<string>();
            round++;

            for (var i = 0; i < current.Count; i += _defaults.MaxMergeFiles)
            {
                var group = current
                    .Skip(i)
                    .Take(_defaults.MaxMergeFiles)
                    .ToList();

                var output =
                    _tempFileProvider.CreateMergeOutput(round, i / _defaults.MaxMergeFiles);

                MergeChunks(group, output);
                nextRound.Add(output);
            }

            foreach (var file in current)
            {
                _tempFileProvider.Delete(file);
            }

            current = nextRound;
        }

        _tempFileProvider.MoveToFinal(current[0], _defaults.SortedFileName);
    }

    public void MergeChunks(IReadOnlyList<string> chunks, string outputFile)
    {
        var sources = chunks.Select(_reader.Read).ToList();
        var merged = _mergeAlgorithm.Merge(sources);
        _writer.Write(outputFile, merged);
    }
}
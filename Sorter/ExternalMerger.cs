using System.Text;

namespace Sorter;

internal sealed class ExternalMerger
{
    private readonly Defaults _defaults;

    public ExternalMerger(Defaults defaults)
    {
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

                var output = Path.Combine(
                    _defaults.TempDir,
                    $"merge_r{round}_{i / _defaults.MaxMergeFiles}.txt");

                MergeChunks(group, output);
                nextRound.Add(output);
            }

            foreach (var file in current)
            {
                File.Delete(file);
            }

            current = nextRound;
        }

        File.Move(current[0], _defaults.SortedFileName, true);
    }

    public void MergeChunks(IReadOnlyList<string> chunks, string outputFile)
    {
        var readers = new List<StreamReader>();
        try
        {
            var pq = new PriorityQueue<MergeItem, LineRecord>();

            foreach (var chunk in chunks)
            {
                var reader = new StreamReader(
                    new FileStream(chunk, FileMode.Open, FileAccess.Read, FileShare.Read,
                        _defaults.FileBufferSize),
                    Encoding.UTF8);

                readers.Add(reader);

                var line = reader.ReadLine();
                if (line != null)
                {
                    var record = LineRecord.Parse(line);
                    var mergeItem = new MergeItem(record, reader);

                    pq.Enqueue(mergeItem, record);
                }
            }

            using var writer = new StreamWriter(
                new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None,
                    _defaults.FileBufferSize),
                Encoding.UTF8);

            while (pq.Count > 0)
            {
                var item = pq.Dequeue();
                writer.WriteLine(item.Record.ToString());

                var nextLine = item.Reader.ReadLine();
                if (nextLine != null)
                {
                    var next = LineRecord.Parse(nextLine);
                    var mergeItem = new MergeItem(next, item.Reader);
                    pq.Enqueue(mergeItem, next);
                }
            }
        }
        finally
        {
            foreach (var r in readers)
            {
                r.Dispose();
            }
        }
    }

    private sealed class MergeItem
    {
        public MergeItem(LineRecord record, StreamReader reader)
        {
            Record = record;
            Reader = reader;
        }

        public LineRecord Record { get; }
        public StreamReader Reader { get; }
    }
}
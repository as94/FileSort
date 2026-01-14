using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using Sorter.Algorithms;
using Sorter.IO;

namespace Sorter;

public sealed class ChunkSorter
{
    private readonly IChunkSortAlgorithm<LineRecord> _chunkSortAlgorithm;
    private readonly Defaults _defaults;
    private readonly ILineReader<LineRecord> _reader;

    private readonly SemaphoreSlim _semaphore;
    private readonly ILineWriter<LineRecord> _writer;

    public ChunkSorter(
        IChunkSortAlgorithm<LineRecord> chunkSortAlgorithm,
        ILineReader<LineRecord> reader,
        ILineWriter<LineRecord> writer,
        Defaults defaults)
    {
        _chunkSortAlgorithm = chunkSortAlgorithm;
        _reader = reader;
        _writer = writer;
        _defaults = defaults;
        _semaphore = new SemaphoreSlim(_defaults.MaxParallelism, _defaults.MaxParallelism);
    }

    public async Task<IReadOnlyList<string>> SplitAndSortAsync(string inputFile)
    {
        var chunkFiles = new ConcurrentBag<string>();
        var buffer = ArrayPool<LineRecord>.Shared.Rent(_defaults.ChunkSorterBufferSize);
        var bufferCount = 0;
        long currentBytes = 0;
        var chunkIndex = 0;

        var tasks = new List<Task>();
        foreach (var record in _reader.Read(inputFile))
        {
            buffer[bufferCount++] = record;
            currentBytes += Encoding.UTF8.GetByteCount(record.ToString());

            if (currentBytes >= _defaults.MaxChunkBytes || bufferCount >= buffer.Length)
            {
                var chunk = buffer;
                var count = bufferCount;

                tasks.Add(FlushChunkAsync(chunk, count, chunkIndex++, chunkFiles));
                buffer = ArrayPool<LineRecord>.Shared.Rent(_defaults.ChunkSorterBufferSize);
                bufferCount = 0;
                currentBytes = 0;

                if (tasks.Count >= _defaults.MaxParallelism)
                {
                    var finished = await Task.WhenAny(tasks);
                    tasks.Remove(finished);
                }
            }
        }

        if (bufferCount > 0)
        {
            tasks.Add(FlushChunkAsync(buffer, bufferCount, chunkIndex++, chunkFiles));
        }

        await Task.WhenAll(tasks);

        return chunkFiles
            .ToList()
            .AsReadOnly();
    }

    private async Task FlushChunkAsync(LineRecord[] records, int bufferCount, int index,
        ConcurrentBag<string> chunkFiles)
    {
        await _semaphore.WaitAsync();

        try
        {
            _chunkSortAlgorithm.Sort(records, bufferCount);

            var chunkFile = Path.Combine(_defaults.TempDir, $"chunk_{index}.txt");
            var path = Path.Combine(_defaults.TempDir, $"chunk_{index}.txt");

            _writer.Write(path, records.AsSpan(0, bufferCount).ToArray());

            chunkFiles.Add(chunkFile);
        }
        finally
        {
            _semaphore.Release();
            ArrayPool<LineRecord>.Shared.Return(records, true);
        }
    }
}
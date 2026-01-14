using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using Sorter.Algorithms;

namespace Sorter;

internal sealed class ChunkSorter
{
    private readonly IChunkSortAlgorithm<LineRecord> _chunkSortAlgorithm;
    private readonly Defaults _defaults;

    private readonly SemaphoreSlim _semaphore;

    public ChunkSorter(
        IChunkSortAlgorithm<LineRecord> chunkSortAlgorithm,
        Defaults defaults)
    {
        _chunkSortAlgorithm = chunkSortAlgorithm;
        _defaults = defaults;
        _semaphore = new SemaphoreSlim(_defaults.MaxParallelism, _defaults.MaxParallelism);
    }

    public async Task<IReadOnlyList<string>> SplitAndSortAsync(string inputFile)
    {
        Directory.CreateDirectory(_defaults.TempDir);

        var chunkFiles = new ConcurrentBag<string>();
        var buffer = ArrayPool<LineRecord>.Shared.Rent(_defaults.ChunkSorterBufferSize);
        var bufferCount = 0;
        long currentBytes = 0;
        var chunkIndex = 0;

        using var reader = new StreamReader(
            new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read,
                _defaults.FileBufferSize),
            Encoding.UTF8);

        var tasks = new List<Task>();
        while (true)
        {
            // ReSharper disable once MethodHasAsyncOverload
            var line = reader.ReadLine();
            if (line == null)
            {
                break;
            }

            var record = LineRecord.Parse(line);
            buffer[bufferCount++] = record;
            currentBytes += Encoding.UTF8.GetByteCount(line);

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
            await using var writer = new StreamWriter(
                new FileStream(chunkFile, FileMode.Create, FileAccess.Write, FileShare.None,
                    _defaults.FileBufferSize),
                Encoding.UTF8);

            for (var i = 0; i < bufferCount; i++)
            {
                await writer.WriteLineAsync(records[i].ToString());
            }

            chunkFiles.Add(chunkFile);
        }
        finally
        {
            _semaphore.Release();
            ArrayPool<LineRecord>.Shared.Return(records, true);
        }
    }
}
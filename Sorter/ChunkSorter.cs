using System.Buffers;
using System.Collections.Concurrent;
using System.Text;

namespace Sorter;

internal sealed class ChunkSorter
{
    private const int LineRecordBufferSize = 1000_000;

    private readonly long _maxChunkBytes;
    private readonly int _maxParallelism;

    private readonly SemaphoreSlim _semaphore;
    private readonly string _tempDir;

    public ChunkSorter(long maxChunkBytes, string tempDir, int maxParallelism)
    {
        _maxChunkBytes = maxChunkBytes;
        _tempDir = tempDir;
        _maxParallelism = maxParallelism;
        _semaphore = new SemaphoreSlim(maxParallelism, maxParallelism);
    }

    public async Task<IReadOnlyList<string>> SplitAndSortAsync(string inputFile)
    {
        Directory.CreateDirectory(_tempDir);

        var chunkFiles = new ConcurrentBag<string>();
        var buffer = ArrayPool<LineRecord>.Shared.Rent(LineRecordBufferSize);
        var bufferCount = 0;
        long currentBytes = 0;
        var chunkIndex = 0;

        using var reader = new StreamReader(
            new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read,
                Defaults.FileBufferSize),
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

            if (currentBytes >= _maxChunkBytes || bufferCount >= buffer.Length)
            {
                var chunk = buffer;
                var count = bufferCount;

                tasks.Add(FlushChunkAsync(chunk, count, chunkIndex++, chunkFiles));
                buffer = ArrayPool<LineRecord>.Shared.Rent(LineRecordBufferSize);
                bufferCount = 0;
                currentBytes = 0;

                if (tasks.Count >= _maxParallelism)
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
            .OrderBy(x => x)
            .ToList()
            .AsReadOnly();
    }

    private async Task FlushChunkAsync(LineRecord[] records, int bufferCount, int index,
        ConcurrentBag<string> chunkFiles)
    {
        await _semaphore.WaitAsync();

        try
        {
            Array.Sort(records, 0, bufferCount);

            var chunkFile = Path.Combine(_tempDir, $"chunk_{index}.txt");
            await using var writer = new StreamWriter(
                new FileStream(chunkFile, FileMode.Create, FileAccess.Write, FileShare.None,
                    Defaults.FileBufferSize),
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
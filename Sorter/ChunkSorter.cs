using System.Buffers;
using System.Collections.Concurrent;
using System.Text;

namespace Sorter;

internal sealed class ChunkSorter
{
    private const int LineRecordBufferSize = 1000_000;
    private const int FileBufferSize = 1024 * 1024; // 1 Mb

    private readonly long _maxChunkBytes;

    private readonly SemaphoreSlim _semaphore;
    private readonly string _tempDir;

    public ChunkSorter(long maxChunkBytes, string tempDir, int maxParallelism)
    {
        _maxChunkBytes = maxChunkBytes;
        _tempDir = tempDir;
        _semaphore = new SemaphoreSlim(maxParallelism, maxParallelism);
    }

    public async Task<List<string>> SplitAndSortAsync(string inputFile)
    {
        Directory.CreateDirectory(_tempDir);

        var chunkFiles = new ConcurrentBag<string>();
        var buffer = ArrayPool<LineRecord>.Shared.Rent(LineRecordBufferSize);
        var bufferCount = 0;
        long currentBytes = 0;
        var chunkIndex = 0;

        using var reader = new StreamReader(
            new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read,
                FileBufferSize),
            Encoding.UTF8);

        var tasks = new List<Task>();
        while (true)
        {
            var line = await reader.ReadLineAsync();
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
            }
        }

        if (bufferCount > 0)
        {
            var chunk = buffer;
            var count = bufferCount;
            tasks.Add(FlushChunkAsync(chunk, count, chunkIndex++, chunkFiles));
        }

        await Task.WhenAll(tasks);

        ArrayPool<LineRecord>.Shared.Return(buffer, true);

        return chunkFiles
            .OrderBy(x => x)
            .ToList();
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
                    FileBufferSize),
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
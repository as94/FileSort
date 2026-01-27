using System.Buffers;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using Core.Sorting.Algorithms;
using Core.Sorting.IO;

namespace Core.Sorting;

public sealed class ChunkSorter
{
    private readonly IChunkSortAlgorithm<LineRecord> _chunkSortAlgorithm;
    private readonly Defaults _defaults;
    private readonly ILineReader<LineRecord> _reader;
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
    }

    public async Task<IReadOnlyList<string>> SplitAndSortAsync(string inputFile)
    {
        var chunkFiles = new ConcurrentBag<string>();
        var channel =
            Channel.CreateBounded<(LineRecord[] Buffer, int Count)>(_defaults.MaxParallelism);

        var chunkIndex = -1;

        var consumerTasks = Enumerable.Range(0, _defaults.MaxParallelism).Select(_ =>
            Task.Run(async () =>
            {
                await foreach (var item in channel.Reader.ReadAllAsync())
                {
                    var buffer = item.Buffer;
                    try
                    {
                        var count = item.Count;
                        var index = Interlocked.Increment(ref chunkIndex);
                        _chunkSortAlgorithm.Sort(buffer, count);

                        var path = Path.Combine(_defaults.TempDir, $"chunk_{index}.txt");
                        _writer.Write(path, buffer.AsSpan(0, count).ToArray());
                        chunkFiles.Add(path);
                    }
                    finally
                    {
                        ArrayPool<LineRecord>.Shared.Return(buffer, true);
                    }
                }
            })).ToArray();

        var buffer = ArrayPool<LineRecord>.Shared.Rent(_defaults.ChunkSorterBufferSize);
        var bufferCount = 0;
        long currentBytes = 0;

        foreach (var record in _reader.Read(inputFile))
        {
            buffer[bufferCount++] = record;
            currentBytes += Encoding.UTF8.GetByteCount(record.ToString());

            if (currentBytes >= _defaults.MaxChunkBytes || bufferCount >= buffer.Length)
            {
                await channel.Writer.WriteAsync((buffer, bufferCount));

                buffer = ArrayPool<LineRecord>.Shared.Rent(_defaults.ChunkSorterBufferSize);
                bufferCount = 0;
                currentBytes = 0;
            }
        }

        if (bufferCount > 0)
        {
            await channel.Writer.WriteAsync((buffer, bufferCount));
        }

        channel.Writer.Complete();
        await Task.WhenAll(consumerTasks);

        return chunkFiles.ToList().AsReadOnly();
    }
}
namespace Sorter;

public sealed class Defaults
{
    private readonly bool _isTestEnvironment;

    private Defaults(bool isTestEnvironment)
    {
        _isTestEnvironment = isTestEnvironment;
    }

    public static Defaults Test => new(true);
    public static Defaults Production => new(false);

    public int FileBufferSize => 1024 * 1024; // 1 Mb

    public long MaxChunkBytes => _isTestEnvironment
        ? 1L * 1024 * 1024 // 1 MB
        : 512L * 1024 * 1024; // 512 MB

    public int MaxMergeFiles => _isTestEnvironment ? 5 : 128;

    public int ChunkSorterBufferSize => 1000_000;

    public int MaxParallelism => Environment.ProcessorCount;

    public string TempDir => "temp";
    public string SortedFileName => "sorted.txt";
}
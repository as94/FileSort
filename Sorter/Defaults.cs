namespace Sorter;

public static class Defaults
{
    public const long MaxChunkBytes = 1L * 1024 * 1024; // 1 MB
    // 512L * 1024 * 1024; // 512 MB


    public const int FileBufferSize = 1024 * 1024; // 1 Mb

    public const int MaxMergeFiles = 5; // 128;

    public const string TempDir = "temp";
    public const string SortedFileName = "sorted.txt";
}
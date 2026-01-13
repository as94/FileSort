using Sorter;

if (args.Length == 0)
{
    Console.WriteLine("Input file path is not specified");
    return;
}

var inputFilePath = args[0];
if (!File.Exists(inputFilePath))
{
    Console.WriteLine("Input file path does not exist");
}

var tempDir = "tmp";

var chunkSorter = new ChunkSorter(
    512L * 1024 * 1024, // 512 MB
    tempDir,
    Environment.ProcessorCount
);

var chunks = await chunkSorter.SplitAndSortAsync(inputFilePath);
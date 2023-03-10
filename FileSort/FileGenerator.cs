using System.Text;

namespace FileSort;

public sealed class FileGenerator
{
    private readonly StringGenerator _generator;
    private readonly int _bufferSize;

    public FileGenerator(StringGenerator generator, int bufferSize = 100_000)
    {
        if (bufferSize > 10_000_000)
        {
            throw new ArgumentException($"{nameof(bufferSize)} shouldn't be more than 10_000_000");
        }
        
        _generator = generator;
        _bufferSize = bufferSize;
    }

    public async Task GenerateFileAsync(
        string fileName = "file.txt",
        int fileSizeInMb = 1000,
        CancellationToken ct = default)
    {
        await using var file = File.Open(fileName, FileMode.OpenOrCreate);
        await using var sw = new StreamWriter(file);
        long currentFileSizeInMb = 0;

        var sb = new StringBuilder();
        var strCount = 0;

        do
        {
            if (ct.IsCancellationRequested)
            {
                return;
            }
            
            var str = _generator.Get();
            sb.AppendLine(str);
            strCount++;

            if (strCount == _bufferSize)
            {
                await sw.WriteAsync(sb.ToString());
                strCount = 0;
                sb.Clear();
                var fileInfo = new FileInfo(fileName);
                currentFileSizeInMb = fileInfo.ConvertToMegabytes();
                Console.WriteLine($"Current file size in MB: {currentFileSizeInMb}");
            }
        } while (currentFileSizeInMb < fileSizeInMb);
    }
}
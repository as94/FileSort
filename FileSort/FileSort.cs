namespace FileSort;

public sealed class FileSort
{
    private readonly string _filePath;
    private const string TmpDirectoryName = "tmp";

    public FileSort(string filePath)
    {
        _filePath = filePath;
    }
    
    public async Task SortAsync(CancellationToken ct)
    {
        if (!Directory.Exists(TmpDirectoryName))
        {
            Directory.CreateDirectory(TmpDirectoryName);
        }

        try
        {
            await SplitToSmallFilesAsync(ct);
            await CreateSortedLargeFileAsync(ct);
        }
        finally
        {
            Directory.Delete(TmpDirectoryName, true);
        }

    }

    private async Task SplitToSmallFilesAsync(CancellationToken ct)
    {
        var linesCountInLargeFile = await GetLinesCountAsync(ct);
        var linesCountInSmallFile = linesCountInLargeFile / 10;

        await CreateSmallFileFromLargeAsync(linesCountInSmallFile, ct);
    }

    private async Task<long> GetLinesCountAsync(CancellationToken ct)
    {
        long lineCount = 0;
        using var reader = File.OpenText(_filePath);
        while (await reader.ReadLineAsync(ct) != null)
        {
            lineCount++;
        }

        return lineCount;
    }

    private async Task CreateSmallFileFromLargeAsync(
        long linesCountInSmallFile,
        CancellationToken ct)
    {
        using var reader = File.OpenText(_filePath);
        var line = await reader.ReadLineAsync(ct);
        var rows = new Row[linesCountInSmallFile];
        var rowsCount = 0;
        var filesCount = 0;

        while (line != null)
        {
            rows[rowsCount++] = new Row(line);

            if (rowsCount == linesCountInSmallFile)
            {
                rowsCount = 0;
                filesCount++;

                var newFilePath = Path.Combine(TmpDirectoryName, $"file_{filesCount + 1}.txt");
                await using var newFile = File.OpenWrite(newFilePath);
                await using var writer = new StreamWriter(newFile);

                foreach (var r in rows.OrderBy(x => x))
                {
                    await writer.WriteLineAsync(r.Value);
                }
            }
            
            line = await reader.ReadLineAsync(ct);
        }
    }

    private async Task CreateSortedLargeFileAsync(CancellationToken ct)
    {
        var files = Directory.GetFiles(TmpDirectoryName)
            .OrderBy(x => x)
            .ToArray();
        var readers = new StreamReader[files.Length];
        for (var i = 0; i < files.Length; i++)
        {
            readers[i] = new StreamReader(File.OpenRead(files[i]));
        }
        
        var currentRows = new string?[files.Length];
        for (var i = 0; i < readers.Length; i++)
        {
            currentRows[i] = await readers[i].ReadLineAsync(ct);
        }

        await using var resultFile = File.OpenWrite($"{Path.GetFileNameWithoutExtension(_filePath)}_SortResult.txt");
        await using var resultFileWriter = new StreamWriter(resultFile);

        var (nextIdx, nextRow) = GetNext(currentRows);

        while (nextRow != null)
        {
            await resultFileWriter.WriteLineAsync(nextRow);
            
            currentRows[nextIdx] = await readers[nextIdx].ReadLineAsync(ct);
            (nextIdx, nextRow) = GetNext(currentRows);
        }
    }

    private (int idx, string? row) GetNext(string?[] rows)
    {
        Row minRow = null;
        var idx = -1;
        for (var i = 0; i < rows.Length; i++)
        {
            var row = rows[i];
            if (row == null)
            {
                continue;
            }

            var currentRow = new Row(row);
            if (minRow == null)
            {
                minRow = currentRow;
                idx = i;
                continue;
            }

            var comparison = Comparer<Row>.Default.Compare(minRow, currentRow);
            if (comparison == 0 || comparison == -1)
            {
                continue;
            }
            
            if (comparison == 1)
            {
                minRow = currentRow;
                idx = i;
            }
        }

        return (idx, minRow?.Value);
    }
}
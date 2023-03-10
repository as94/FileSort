namespace FileSort;

public sealed class FileSorter
{
    private readonly string _filePath;
    private const string TmpDirectoryName = "tmp";

    private const int LinesCountInSmallFile = 100_000_000;

    public FileSorter(string filePath)
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
            await CreateSmallFilesFromLargeAsync(ct);
            await CreateSortedLargeFileAsync(ct);
        }
        finally
        {
            Directory.Delete(TmpDirectoryName, true);
        }

    }

    private async Task CreateSmallFilesFromLargeAsync(
        CancellationToken ct)
    {
        using var reader = File.OpenText(_filePath);
        var line = await reader.ReadLineAsync(ct);
        var rows = new Row?[LinesCountInSmallFile];
        var rowsCount = 0;
        var filesCount = 0;

        while (line != null)
        {
            rows[rowsCount++] = new Row(line);

            if (rowsCount == LinesCountInSmallFile)
            {
                rowsCount = 0;
                filesCount++;

                await CreateSmallFileAsync(filesCount, rows);
            }
            
            line = await reader.ReadLineAsync(ct);
        }

        if (rowsCount > 0)
        {
            await CreateSmallFileAsync(filesCount, rows);
        }
    }

    private static async Task CreateSmallFileAsync(int filesCount, Row?[] rows)
    {
        var newFilePath = Path.Combine(TmpDirectoryName, $"file_{filesCount + 1}.txt");
        await using var writer = new StreamWriter(newFilePath);

        foreach (var r in rows.Where(r => r != null).OrderBy(x => x))
        {
            await writer.WriteLineAsync(r!.Value);
        }
    }

    private async Task CreateSortedLargeFileAsync(CancellationToken ct)
    {
        var resultFilePath = $"{Path.GetFileNameWithoutExtension(_filePath)}_SortResult.txt";
        
        var files = Directory.GetFiles(TmpDirectoryName)
            .OrderBy(x => x)
            .ToArray();

        if (files.Length == 1)
        {
            File.Copy(files[0], resultFilePath);
            return;
        }
        
        var readers = new StreamReader[files.Length];
        try
        {
            for (var i = 0; i < files.Length; i++)
            {
                readers[i] = new StreamReader(File.OpenRead(files[i]));
            }
        
            var currentRows = new string?[files.Length];
            for (var i = 0; i < readers.Length; i++)
            {
                currentRows[i] = await readers[i].ReadLineAsync(ct);
            }

            await using var resultFileWriter = new StreamWriter(resultFilePath);

            var (nextIdx, nextRow) = GetNext(currentRows);

            while (nextRow != null)
            {
                await resultFileWriter.WriteLineAsync(nextRow);
            
                currentRows[nextIdx] = await readers[nextIdx].ReadLineAsync(ct);
                (nextIdx, nextRow) = GetNext(currentRows);
            }
        }
        finally
        {
            foreach (var reader in readers)
            {
                reader.Dispose();
            }
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
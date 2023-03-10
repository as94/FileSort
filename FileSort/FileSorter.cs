using System.Diagnostics;

namespace FileSort;

public sealed class FileSorter
{
    private readonly string _filePath;
    private const string TmpDirectoryName = "tmp";

    private const int LinesCountInSmallFile = 10_000_000;

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
            var sw = Stopwatch.StartNew();
            var newFilePaths = await CreateSmallFilesFromLargeAsync(ct);
            Console.WriteLine($"Elapsed in milliseconds (CreateSmallFilesFromLargeAsync): {sw.ElapsedMilliseconds}");
            
            sw.Restart();
            await CreateSortedLargeFileAsync(newFilePaths, ct);
            Console.WriteLine($"Elapsed in milliseconds (CreateSortedLargeFileAsync): {sw.ElapsedMilliseconds}");
        }
        finally
        {
            Directory.Delete(TmpDirectoryName, true);
        }

    }

    private async Task<List<string>> CreateSmallFilesFromLargeAsync(CancellationToken ct)
    {
        var newFilePaths = new List<string>();
        
        var rows = new Row[LinesCountInSmallFile];
        var rowsCount = 0;
        var filesCount = 0;
        
        var lines = File.ReadLines(_filePath);
        foreach (var line in lines)
        {
            rows[rowsCount] = new Row(line);
            rowsCount++;

            if (rowsCount == LinesCountInSmallFile)
            {
                var newFilePath = await CreateSmallFileAsync(filesCount, rows);
                newFilePaths.Add(newFilePath);
                filesCount++;
                rowsCount = 0;
                rows = new Row[LinesCountInSmallFile];
            }
        }

        if (rowsCount > 0)
        {
            var newFilePath = await CreateSmallFileAsync(filesCount, rows);
            newFilePaths.Add(newFilePath);
        }

        return newFilePaths;
    }

    private static async Task<string> CreateSmallFileAsync(int filesCount, Row[] rows)
    {
        var newFilePath = Path.Combine(TmpDirectoryName, $"file_{filesCount + 1}.txt");
        var sortedRows = rows
            .Where(r => !r.IsNull)
            .OrderBy(x => x)
            .Select(r => r!.Value);

        await File.WriteAllLinesAsync(newFilePath, sortedRows);

        return newFilePath;
    }

    private async Task CreateSortedLargeFileAsync(List<string> files, CancellationToken ct)
    {
        var resultFilePath = $"{Path.GetFileNameWithoutExtension(_filePath)}_SortResult.txt";

        if (files.Count == 1)
        {
            File.Copy(files[0], resultFilePath);
            return;
        }
        
        var readers = new StreamReader[files.Count];
        try
        {
            for (var i = 0; i < files.Count; i++)
            {
                readers[i] = new StreamReader(files[i]);
            }
        
            var currentRows = new string?[files.Count];
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
        Row minRow = new Row();
        var idx = -1;
        for (var i = 0; i < rows.Length; i++)
        {
            var row = rows[i];
            if (row == null)
            {
                continue;
            }

            var currentRow = new Row(row);
            if (minRow.IsNull)
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

        return (idx, minRow.Value);
    }
}
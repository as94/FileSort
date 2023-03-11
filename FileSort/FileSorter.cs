using System.Diagnostics;

namespace FileSort;

internal sealed class FileSorter
{
    private readonly string _filePath;
    private const string TmpDirectoryName = "tmp";

    private const int LinesCountInSmallFile = 10_000_000;

    public FileSorter(string filePath)
    {
        _filePath = filePath;
    }
    
    public void Sort()
    {
        if (!Directory.Exists(TmpDirectoryName))
        {
            Directory.CreateDirectory(TmpDirectoryName);
        }

        try
        {
            var sw = Stopwatch.StartNew();
            var newFilePaths = CreateSmallFilesFromLarge();
            Console.WriteLine($"Elapsed in milliseconds (CreateSmallFilesFromLarge): {sw.ElapsedMilliseconds}");
            
            sw.Restart();
            CreateSortedLargeFile(newFilePaths);
            Console.WriteLine($"Elapsed in milliseconds (CreateSortedLargeFile): {sw.ElapsedMilliseconds}");
        }
        finally
        {
            Directory.Delete(TmpDirectoryName, true);
        }

    }

    private List<string> CreateSmallFilesFromLarge()
    {
        var newFilePaths = new List<string>();
        
        var rows = new Row[LinesCountInSmallFile];
        var rowsCount = 0;
        var filesCount = 0;
        
        var lines = File.ReadLines(_filePath);
        foreach (var line in lines)
        {
            rows[rowsCount] = Row.Create(line);
            rowsCount++;

            if (rowsCount == LinesCountInSmallFile)
            {
                var newFilePath = CreateSmallFile(filesCount, rows);
                newFilePaths.Add(newFilePath);
                filesCount++;
                rowsCount = 0;
                rows = new Row[LinesCountInSmallFile];
            }
        }

        if (rowsCount > 0)
        {
            Array.Resize(ref rows, rowsCount);
            var newFilePath = CreateSmallFile(filesCount, rows);
            newFilePaths.Add(newFilePath);
        }

        return newFilePaths;
    }

    private static string CreateSmallFile(int filesCount, Row[] rows)
    {
        var newFilePath = Path.Combine(TmpDirectoryName, $"file_{filesCount + 1}.txt");
        Array.Sort(rows);
        File.WriteAllLines(newFilePath, rows.Where(r => !r.IsNull).Select(r => r.Value!));

        return newFilePath;
    }

    private void CreateSortedLargeFile(List<string> files)
    {
        var resultFilePath = $"{Path.GetFileNameWithoutExtension(_filePath)}_SortResult.txt";

        if (files.Count == 1)
        {
            File.Copy(files[0], resultFilePath);
            return;
        }
        
        var readers = new StreamReader[files.Count];
        var smallFileRows = new SmallFileRow[files.Count];
        try
        {
            for (var i = 0; i < files.Count; i++)
            {
                var reader = new StreamReader(files[i]);
                readers[i] = reader;

                smallFileRows[i] = new SmallFileRow(Row.Create(reader.ReadLine()), reader);
            }

            using var resultFileWriter = new StreamWriter(resultFilePath);

            var min = smallFileRows.MinBy(x => x.Row);

            while (min is { Row.IsNull: false })
            {
                resultFileWriter.WriteLine(min.Row);

                min.Row = Row.Create(min.Reader.ReadLine());
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
}
using Core.Generating;
using Core.Sorting;
using Core.Sorting.IO;
using FluentAssertions;
using Moq;

namespace Tests.IntegrationTests;

public sealed class FileSorterTests
{
    [Theory]
    [InlineData(1 * 1024 * 1024)] // 1 MB
    [InlineData(5 * 1024 * 1024)] // 5 MB
    public async Task SortFileAsync_SortsGeneratedFile(long sizeInBytes)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "filesorter_test");
        Directory.CreateDirectory(tempDir);
        var inputFile = Path.Combine(tempDir, "input.txt");
        var outputFile = Path.Combine(tempDir, "output.txt");
        var generator = new FileGenerator();
        generator.GenerateFile(inputFile, sizeInBytes);
        var defaults = Defaults.Test;
        var sorter = new FileSorter(defaults, Mock.Of<IConsole>());

        try
        {
            await sorter.SortFileAsync(inputFile, outputFile);

            File.Exists(outputFile).Should().BeTrue("File must be created after sorting");

            var lines = File.ReadLines(outputFile).ToList();
            lines.Should().NotBeEmpty();

            var parsed = lines.Select(LineRecord.Parse).ToList();
            parsed.Should().BeInAscendingOrder();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
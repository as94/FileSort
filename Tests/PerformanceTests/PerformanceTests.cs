using System.Diagnostics;
using Core.Generating;
using Core.Sorting;
using Core.Sorting.IO;
using FluentAssertions;
using Moq;
using Xunit.Abstractions;

namespace Tests.PerformanceTests;

public sealed class PerformanceTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PerformanceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    // [Theory]
    [Theory(Skip = "Performance Tests")]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    public async Task SortFileAsync_1GbFile_CompletesUnderThreshold(long sizeInGb)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "perf_test");
        Directory.CreateDirectory(tempDir);
        var inputFile = Path.Combine(tempDir, $"input_{sizeInGb}GB.txt");
        var outputFile = Path.Combine(tempDir, $"output_{sizeInGb}GB.txt");
        var generator = new FileGenerator();
        generator.GenerateFile(inputFile, sizeInGb * 1024 * 1024 * 1024);
        var sorter = new FileSorter(Defaults.Production, Mock.Of<IConsole>());

        try
        {
            var sw = Stopwatch.StartNew();
            await sorter.SortFileAsync(inputFile, outputFile);
            sw.Stop();

            File.Exists(outputFile).Should().BeTrue();
            _testOutputHelper.WriteLine(sw.Elapsed.ToString());
            sw.Elapsed.Should().BeLessThan(TimeSpan.FromMinutes(sizeInGb),
                $"Sorting {sizeInGb}GB should take less than {sizeInGb} minutes.");
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
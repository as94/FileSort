using System.Text;
using Core.Generating;
using FluentAssertions;

namespace Tests.IntegrationTests;

public sealed class FileGeneratorTests
{
    [Fact]
    public void GenerateFile_CreatesFileWithExpectedSize()
    {
        var generator = new FileGenerator();
        var tempFile = Path.Combine(Path.GetTempPath(), "test_generated_file.txt");
        var targetSize = 50 * 1024L; // 50 KB

        try
        {
            generator.GenerateFile(tempFile, targetSize);

            File.Exists(tempFile).Should().BeTrue("File must be created");

            var fileInfo = new FileInfo(tempFile);
            fileInfo.Length.Should().BeGreaterThanOrEqualTo(targetSize / 2,
                "File must contain sufficient amount of data");
            fileInfo.Length.Should().BeLessThanOrEqualTo(targetSize * 2,
                "File should not be too large.");

            var firstLine = File.ReadLines(tempFile, Encoding.UTF8).FirstOrDefault();
            firstLine.Should().NotBeNullOrEmpty();
            firstLine!.Should().MatchRegex(@"^\d+\. .+");
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }
}
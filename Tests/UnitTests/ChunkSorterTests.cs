using FluentAssertions;
using Moq;
using Sorter;
using Sorter.Algorithms;
using Sorter.IO;

namespace Tests.UnitTests;

public class ChunkSorterTests
{
    [Fact]
    public async Task SplitAndSortAsync_SmallInput_CreatesSingleChunk()
    {
        var defaults = Defaults.Test;
        var records = new[]
        {
            new LineRecord(3, "Banana"),
            new LineRecord(1, "Apple"),
            new LineRecord(2, "Apple")
        };
        var readerMock = new Mock<ILineReader<LineRecord>>();
        readerMock.Setup(r => r.Read(It.IsAny<string>())).Returns(records);
        var writerMock = new Mock<ILineWriter<LineRecord>>();
        writerMock.Setup(w => w.Write(It.IsAny<string>(), It.IsAny<LineRecord[]>()));
        var sorterMock = new Mock<IChunkSortAlgorithm<LineRecord>>();
        sorterMock.Setup(s => s.Sort(It.IsAny<LineRecord[]>(), It.IsAny<int>()))
            .Callback<LineRecord[], int>((arr, count) => { Array.Sort(arr, 0, count); });
        var chunkSorter = new ChunkSorter(sorterMock.Object, readerMock.Object, writerMock.Object,
            defaults);

        var chunks = await chunkSorter.SplitAndSortAsync("dummy.txt");

        chunks.Count.Should().Be(1);
        writerMock.Verify(
            w => w.Write(It.IsAny<string>(),
                It.Is<LineRecord[]>(arr => arr.Length == records.Length)), Times.Once);
        sorterMock.Verify(s => s.Sort(It.IsAny<LineRecord[]>(), records.Length), Times.Once);
    }

    [Fact]
    public async Task SplitAndSortAsync_LargeInput_CreatesMultipleChunks()
    {
        var defaults = Defaults.Test;
        defaults.MaxChunkBytes = 100;
        var records = Enumerable.Range(1, 20)
            .Select(i => new LineRecord(i, "X" + i))
            .ToArray();
        var readerMock = new Mock<ILineReader<LineRecord>>();
        readerMock.Setup(r => r.Read(It.IsAny<string>())).Returns(records);
        var writerMock = new Mock<ILineWriter<LineRecord>>();
        writerMock.Setup(w => w.Write(It.IsAny<string>(), It.IsAny<LineRecord[]>()));
        var sorterMock = new Mock<IChunkSortAlgorithm<LineRecord>>();
        sorterMock.Setup(s => s.Sort(It.IsAny<LineRecord[]>(), It.IsAny<int>()))
            .Callback<LineRecord[], int>((arr, count) => Array.Sort(arr, 0, count));
        var chunkSorter = new ChunkSorter(sorterMock.Object, readerMock.Object, writerMock.Object,
            defaults);

        var chunks = await chunkSorter.SplitAndSortAsync("dummy.txt");

        chunks.Count.Should().BeGreaterThan(1);
        writerMock.Verify(w => w.Write(It.IsAny<string>(), It.IsAny<LineRecord[]>()),
            Times.AtLeast(2));
        sorterMock.Verify(s => s.Sort(It.IsAny<LineRecord[]>(), It.IsAny<int>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task SplitAndSortAsync_EnsuresSortedOrderInEachChunk()
    {
        var defaults = Defaults.Test;
        var records = new[]
        {
            new LineRecord(5, "C"),
            new LineRecord(1, "A"),
            new LineRecord(3, "B")
        };
        var readerMock = new Mock<ILineReader<LineRecord>>();
        readerMock.Setup(r => r.Read(It.IsAny<string>())).Returns(records);
        var writerMock = new Mock<ILineWriter<LineRecord>>();
        IEnumerable<LineRecord> writtenRecords = null!;
        writerMock.Setup(w => w.Write(It.IsAny<string>(), It.IsAny<IEnumerable<LineRecord>>()))
            .Callback<string, IEnumerable<LineRecord>>((path, arr) => writtenRecords = arr);
        var sorter = new ArrayChunkSortAlgorithm<LineRecord>();
        var chunkSorter = new ChunkSorter(sorter, readerMock.Object, writerMock.Object, defaults);

        await chunkSorter.SplitAndSortAsync("dummy.txt");

        writtenRecords.Should().BeInAscendingOrder();
    }
}
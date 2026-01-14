using FluentAssertions;
using Moq;
using Sorter;
using Sorter.Algorithms;
using Sorter.IO;

namespace Tests.UnitTests;

public class ExternalMergerTests
{
    [Fact]
    public void MergeAllChunks_SingleRound_CallsMergeAndWrites()
    {
        var defaults = Defaults.Test;
        defaults.MaxMergeFiles = 2;
        var chunks = new List<string> { "f1", "f2", "f3" };
        var readerMock = new Mock<ILineReader<LineRecord>>();
        readerMock.Setup(r => r.Read(It.IsAny<string>()))
            .Returns<string>(file =>
            [
                new LineRecord(1, file),
                new LineRecord(2, file)
            ]);
        var writerMock = new Mock<ILineWriter<LineRecord>>();
        List<LineRecord[]> writtenChunks = new();
        writerMock.Setup(w => w.Write(It.IsAny<string>(), It.IsAny<IEnumerable<LineRecord>>()))
            .Callback<string, IEnumerable<LineRecord>>((path, data) =>
            {
                writtenChunks.Add(data.ToArray());
            });
        var mergeMock = new Mock<IMergeAlgorithm<LineRecord>>();
        mergeMock.Setup(m => m.Merge(It.IsAny<IReadOnlyList<IEnumerable<LineRecord>>>()))
            .Returns<IReadOnlyList<IEnumerable<LineRecord>>>(sources =>
                sources.SelectMany(x => x).OrderBy(x => x).ToArray());

        var tempMock = new Mock<ITempFileProvider>();
        tempMock.Setup(t => t.CreateMergeOutput(It.IsAny<int>(), It.IsAny<int>()))
            .Returns<int, int>((r, i) => $"merge_{r}_{i}");
        tempMock.Setup(t => t.Delete(It.IsAny<string>()));
        tempMock.Setup(t => t.MoveToFinal(It.IsAny<string>(), It.IsAny<string>()));
        var merger = new ExternalMerger(mergeMock.Object, readerMock.Object, writerMock.Object,
            tempMock.Object, defaults);

        merger.MergeAllChunks(chunks);

        writtenChunks.Count.Should().BeGreaterThan(0);
        foreach (var arr in writtenChunks)
        {
            arr.Should().BeInAscendingOrder();
        }

        tempMock.Verify(t => t.Delete(It.IsAny<string>()), Times.Exactly(5));
        tempMock.Verify(t => t.MoveToFinal(It.IsAny<string>(), defaults.SortedFileName),
            Times.Once);
    }

    [Fact]
    public void MergeChunks_CallsMergeAlgorithmAndWriter()
    {
        var readerMock = new Mock<ILineReader<LineRecord>>();
        readerMock.Setup(r => r.Read("f1"))
            .Returns([
                new LineRecord(2, "B"),
                new LineRecord(1, "A")
            ]);
        var writerMock = new Mock<ILineWriter<LineRecord>>();
        LineRecord[] written = null!;
        writerMock.Setup(w => w.Write("out.txt", It.IsAny<IEnumerable<LineRecord>>()))
            .Callback<string, IEnumerable<LineRecord>>((path, data) => written = data.ToArray());
        var mergeMock = new Mock<IMergeAlgorithm<LineRecord>>();
        mergeMock.Setup(m => m.Merge(It.IsAny<IReadOnlyList<IEnumerable<LineRecord>>>()))
            .Returns<IReadOnlyList<IEnumerable<LineRecord>>>(sources =>
                sources.SelectMany(x => x).OrderBy(x => x).ToArray());
        var tempMock = new Mock<ITempFileProvider>();
        var merger = new ExternalMerger(mergeMock.Object, readerMock.Object, writerMock.Object,
            tempMock.Object, Defaults.Test);

        merger.MergeChunks(["f1"], "out.txt");

        mergeMock.Verify(m => m.Merge(It.IsAny<IReadOnlyList<IEnumerable<LineRecord>>>()),
            Times.Once);
        writerMock.Verify(w => w.Write("out.txt", It.IsAny<IEnumerable<LineRecord>>()), Times.Once);
        written.Should().BeInAscendingOrder();
    }
}
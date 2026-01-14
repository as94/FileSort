using FluentAssertions;
using Sorter;
using Sorter.Algorithms;

namespace Tests.UnitTests;

public class ArrayChunkSortAlgorithmTests
{
    [Fact]
    public void ChunkSort_SortsCorrectly()
    {
        var buffer = new[]
        {
            new LineRecord(5, "Apple"),
            new LineRecord(1, "Apple"),
            new LineRecord(3, "Banana")
        };

        var algo = new ArrayChunkSortAlgorithm<LineRecord>();
        algo.Sort(buffer, buffer.Length);

        buffer.Should().BeInAscendingOrder();
    }
}
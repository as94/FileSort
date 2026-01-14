using FluentAssertions;
using Sorter;
using Sorter.Algorithms;

namespace Tests.UnitTests;

public class KWayMergeAlgorithmTests
{
    [Fact]
    public void Merge_SortsCorrectly()
    {
        var sources = new[]
        {
            new[]
            {
                new LineRecord(2, "Apple"),
                new LineRecord(5, "Banana")
            },
            new[]
            {
                new LineRecord(1, "Apple"),
                new LineRecord(3, "Cherry")
            }
        };

        var algo = new KWayMergeAlgorithm<LineRecord>();
        var result = algo.Merge(sources).ToList();

        result.Should().BeInAscendingOrder();
    }
}
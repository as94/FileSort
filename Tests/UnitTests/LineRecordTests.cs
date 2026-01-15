using Core.Sorting;
using FluentAssertions;

namespace Tests.UnitTests;

public class LineRecordTests
{
    [Fact]
    public void Parse_ValidLine_ReturnsCorrectLineRecord()
    {
        var line = "42. Hello World";

        var record = LineRecord.Parse(line);

        record.Number.Should().Be(42);
        record.Text.Should().Be("Hello World");
    }

    [Fact]
    public void ToString_ReturnsOriginalFormat()
    {
        var record = new LineRecord(123, "Test String");

        record.ToString().Should().Be("123. Test String");
    }

    [Fact]
    public void CompareTo_TextDifferent_SortsByText()
    {
        var a = new LineRecord(1, "Apple");
        var b = new LineRecord(1, "Banana");

        a.CompareTo(b).Should().BeNegative();
        b.CompareTo(a).Should().BePositive();
    }

    [Fact]
    public void CompareTo_TextSame_SortsByNumber()
    {
        var a = new LineRecord(2, "Apple");
        var b = new LineRecord(5, "Apple");

        a.CompareTo(b).Should().BeNegative();
        b.CompareTo(a).Should().BePositive();
    }

    [Fact]
    public void CompareTo_SameTextAndNumber_ReturnsZero()
    {
        var a = new LineRecord(7, "Cherry");
        var b = new LineRecord(7, "Cherry");

        a.CompareTo(b).Should().Be(0);
    }
}
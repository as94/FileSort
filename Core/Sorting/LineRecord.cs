namespace Core.Sorting;

public readonly struct LineRecord : IComparable<LineRecord>
{
    public int Number { get; }
    public string Text { get; }

    public LineRecord(int number, string text)
    {
        Number = number;
        Text = text;
    }

    public static LineRecord Parse(string line)
    {
        var dotIndex = line.IndexOf('.');
        var number = int.Parse(line.AsSpan(0, dotIndex));
        var text = line.AsSpan(dotIndex + 2).ToString();
        return new LineRecord(number, text);
    }

    public int CompareTo(LineRecord other)
    {
        var textCmp = string.CompareOrdinal(Text, other.Text);
        if (textCmp != 0)
        {
            return textCmp;
        }

        return Number.CompareTo(other.Number);
    }

    public override string ToString()
    {
        return $"{Number}. {Text}";
    }
}
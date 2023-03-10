using System.Text.RegularExpressions;

namespace FileSort;

public sealed class Row : IComparable<Row>
{
    private readonly int _id;
    private readonly string _name;

    private static readonly Regex ValidationRegex = new Regex(@"^\d+\. \w+$");
    
    public string Value { get; }

    public Row(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (!ValidationRegex.IsMatch(value))
        {
            throw new ArgumentNullException($"Bad format {value}, should be [number]. [word]");
        }

        var splitResult = value.Split(' ');
        _id = int.Parse(splitResult[0].Remove(splitResult[0].Length - 1));
        _name = splitResult[1];

        Value = value;
    }

    public int CompareTo(Row? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;

        var nameComparison = string.Compare(_name, other._name, StringComparison.InvariantCulture);
        if (nameComparison != 0) return nameComparison;
        
        return _id.CompareTo(other._id);
    }
}
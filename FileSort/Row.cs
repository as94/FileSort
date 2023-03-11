namespace FileSort;

internal readonly struct Row : IComparable<Row>
{
    private readonly int _id;
    private readonly string? _name;
    
    public string? Value { get; }

    public bool IsNull => _id == 0 && _name == null;

    public Row()
    {
        _id = 0;
        _name = null;
        Value = null;
    }

    private Row(string value)
    {
        var splitResult = value.Split(' ');

        _id = int.Parse(splitResult[0].Remove(splitResult[0].Length - 1));
        _name = splitResult[1];
        Value = value;
    }

    public static Row Create(string? value = null)
    {
        return value == null
            ? new Row()
            : new Row(value);
    }

    public int CompareTo(Row other)
    {
        var nameComparison = string.Compare(_name, other._name, StringComparison.InvariantCulture);
        if (nameComparison != 0)
        {
            return nameComparison;
        }
        
        return _id.CompareTo(other._id);
    }
}
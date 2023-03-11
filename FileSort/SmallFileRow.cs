namespace FileSort;

internal sealed class SmallFileRow
{
    public SmallFileRow(Row row, StreamReader reader)
    {
        Row = row;
        Reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }
    
    public Row Row { get; set; }
    public StreamReader Reader { get; }
}
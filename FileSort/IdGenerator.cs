namespace FileSort;

public interface IIdGenerator
{
    int Get();
}

public sealed class IdGenerator : IIdGenerator
{
    private readonly Random _random = new();
    
    public int Get()
    {
        return _random.Next(0, int.MaxValue);
    }
}
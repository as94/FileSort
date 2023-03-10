namespace FileSort;

public sealed class StringGenerator
{
    private readonly IIdGenerator _idGenerator;
    private readonly INameGenerator _nameGenerator;

    private readonly Random _rememberRandom = new();
    private readonly Random _idRepeatRandom = new();
    private readonly Random _nameRepeatRandom = new();

    private int _lastId = 1;
    private string _lastName = "Apple";

    public StringGenerator(IIdGenerator idGenerator, INameGenerator nameGenerator)
    {
        _idGenerator = idGenerator;
        _nameGenerator = nameGenerator;
    }
    
    public string Get()
    {
        var id = _idGenerator.Get();
        var name = _nameGenerator.Get();
        
        if (_rememberRandom.Next(0, 10) == 0)
        {
            _lastId = id;
            _lastName = name;
        }

        var repeatName = _nameRepeatRandom.Next(0, 20) == 0;
        var repeatId = _idRepeatRandom.Next(0, 30) == 0;

        if (repeatName && repeatId)
        {
            return $"{_lastId}. {_lastName}";
        }
        
        if (repeatName)
        {
            return $"{id}. {_lastName}";
        }

        if (repeatId)
        {
            return $"{_lastId}. {name}";
        }

        return $"{id}. {name}";
    }
}
using System.Text;

namespace FileSort;

public interface INameGenerator
{
    string Get();
}

public sealed class NameGenerator : INameGenerator
{
    private readonly Random _charRandom = new();
    private readonly Random _lengthRandom = new();
    private readonly Random _caseRandom = new();

    public string Get()
    {
        var length = _lengthRandom.Next(1, 100);
        
        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            var symbol = _caseRandom.Next(0, 2) == 0
                ? (char)_charRandom.Next('a', 'z' + 1)
                : (char)_charRandom.Next('A', 'Z' + 1);
            
            sb.Append(symbol);
        }

        return sb.ToString();
    }
}
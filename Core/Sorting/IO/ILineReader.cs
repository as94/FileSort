namespace Core.Sorting.IO;

public interface ILineReader<out T>
{
    IEnumerable<T> Read(string source);
}
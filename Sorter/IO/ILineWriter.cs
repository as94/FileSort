namespace Sorter.IO;

public interface ILineWriter<in T>
{
    void Write(string target, IEnumerable<T> records);
}
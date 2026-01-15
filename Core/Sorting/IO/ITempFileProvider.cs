namespace Core.Sorting.IO;

public interface ITempFileProvider
{
    string CreateMergeOutput(int round, int index);
    void Delete(string path);
    void MoveToFinal(string source, string target);
}
namespace Core.Sorting.IO;

public sealed class TempFileProvider : ITempFileProvider
{
    private readonly Defaults _defaults;

    public TempFileProvider(Defaults defaults)
    {
        _defaults = defaults;
    }

    public string CreateMergeOutput(int round, int index)
    {
        return Path.Combine(_defaults.TempDir, $"merge_r{round}_{index}.txt");
    }

    public void Delete(string path)
    {
        File.Delete(path);
    }

    public void MoveToFinal(string source, string target)
    {
        File.Move(source, target, true);
    }
}
namespace Core.Sorting.IO;

public sealed class RealConsole : IConsole
{
    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteLine()
    {
        Console.WriteLine();
    }
}
using System.Text;

var random = new Random();

string[] GetStrings()
{
    var defaultStrings = new[]
    {
        "Apple",
        "Banana is yellow",
        "Cherry is the best",
        "Something something something"
    };

    if (args.Length == 0)
    {
        Console.WriteLine(
            "Strings pool size argument is not specified, will be used default strings");
        return defaultStrings;
    }

    var stringsPoolSizeArgument = args[0];
    if (string.IsNullOrWhiteSpace(stringsPoolSizeArgument))
    {
        Console.WriteLine(
            "Strings pool size argument is not specified, will be used default strings");
        return defaultStrings;
    }

    if (!int.TryParse(stringsPoolSizeArgument, out var stringsPoolSize))
    {
        Console.WriteLine(
            "Strings pool size argument should be integer, will be used default strings");
        return defaultStrings;
    }

    if (stringsPoolSize > 10000)
    {
        Console.WriteLine(
            "Strings pool size should be less than 10000, will be used default strings");
        return defaultStrings;
    }

    Console.WriteLine($"Will be used generated strings, strings pool size is {stringsPoolSize}");
    return Enumerable.Range(0, stringsPoolSize)
        .Select(_ => new string(
            Enumerable.Range(1, random.Next(3, 100)).Select(_ => random.Next(0, 2) == 0
                ? (char)random.Next('a', 'z' + 1)
                : (char)random.Next('A', 'Z' + 1)).ToArray()))
        .ToArray();
}

int GetTargetSizeInGb()
{
    var defaultTargetSizeInGb = 1;

    if (args.Length <= 1)
    {
        Console.WriteLine(
            $"Target size Gb argument is not specified, will be used default target size, which is {defaultTargetSizeInGb} Gb");
        return defaultTargetSizeInGb;
    }

    var targetSizeGbArgument = args[1];
    if (string.IsNullOrWhiteSpace(targetSizeGbArgument))
    {
        Console.WriteLine(
            $"Target size Gb argument is not specified, will be used default target size, which is {defaultTargetSizeInGb} Gb");
        return defaultTargetSizeInGb;
    }

    if (!int.TryParse(targetSizeGbArgument, out var targetSizeGb))
    {
        Console.WriteLine(
            $"Target size Gb argument should be integer, will be used target size, which is {defaultTargetSizeInGb} Gb");
        return defaultTargetSizeInGb;
    }

    Console.WriteLine($"Target size: {targetSizeGb} Gb");
    return targetSizeGb;
}

var strings = GetStrings();
var targetSizeInGb = GetTargetSizeInGb();

var fileName = $"{targetSizeInGb}_Gb.txt";
using var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
using var writer = new StreamWriter(fs);

var targetBytes = targetSizeInGb * 1024L * 1024L * 1024L;
long written = 0;
var newlineBytes = Encoding.UTF8.GetByteCount(writer.NewLine);

while (written < targetBytes)
{
    var number = random.Next(1, 1_000_000);
    var text = strings[random.Next(strings.Length)];
    var line = $"{number}. {text}";

    writer.WriteLine(line);
    written += Encoding.UTF8.GetByteCount(line) + newlineBytes;
}
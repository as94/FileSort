using Core.Generating;

if (args.Length < 2)
{
    Console.WriteLine("Usage: generator <output-file> <size-in-B/KB/MB/GB>");
    return;
}

long ParseSizeToBytes(string input)
{
    if (string.IsNullOrWhiteSpace(input))
    {
        throw new ArgumentException("Size is required");
    }

    input = input.Trim().ToUpperInvariant();
    long multiplier = 1;

    if (input.EndsWith("KB"))
    {
        multiplier = 1024;
    }
    else if (input.EndsWith("MB"))
    {
        multiplier = 1024L * 1024;
    }
    else if (input.EndsWith("GB"))
    {
        multiplier = 1024L * 1024 * 1024;
    }

    var numberPart = input.TrimEnd('K', 'M', 'G', 'B');
    if (!long.TryParse(numberPart, out var value))
    {
        throw new ArgumentException($"Invalid size format: {input}");
    }

    return value * multiplier;
}

var outputFile = args[0];
long sizeInBytes;
try
{
    sizeInBytes = ParseSizeToBytes(args[1]);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return;
}

var generator = new FileGenerator();

Console.WriteLine($"Generating file '{outputFile}' with size {sizeInBytes} bytes...");
generator.GenerateFile(outputFile, sizeInBytes);
Console.WriteLine("Done!");
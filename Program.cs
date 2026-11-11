using System.Globalization;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
if (args.Length > 0 && args[0] == "pilot")
{
    try { PilotReport.Run(args); return 0; }
    catch (Exception e) when (e is FormatException or IOException or ArgumentException or OverflowException or UnauthorizedAccessException)
    { Console.Error.WriteLine(e.Message); return 1; }
}

if (args.Length != 1 || args[0] == "--help")
{
    Console.WriteLine("Usage: dotnet run -- pilot study.tsv  |  original format: <input-file|->");
    return args.Length == 1 ? 0 : 2;
}

try
{
    string[] lines = args[0] == "-" ? Console.In.ReadToEnd().ReplaceLineEndings("\n").TrimEnd('\n').Split('\n') : File.ReadAllLines(args[0]);
    Check(lines.Length >= 1 && lines.Any(line => !string.IsNullOrWhiteSpace(line)), "Input is empty or missing records.");
    decimal total = 0, diverted = 0;
    foreach (var f in lines.Select(s => Fields(s, 4)))
    {
        var land = Num(f[1]);
        var rec = Num(f[2]);
        var compost = Num(f[3]);
        Check(land >= 0 && rec >= 0 && compost >= 0, "Mass cannot be negative.");
        var mass = land + rec + compost;
        diverted += rec + compost;
        total += mass;
        Console.WriteLine($"{f[0]}: {mass} kg sampled, {rec + compost} kg diverted");
    }

    Check(total > 0, "Sample mass must be positive.");
    Console.WriteLine($"Campus diversion: {100 * diverted / total:0.##}%");
    return 0;
}
catch (Exception e)when (e is IOException or FormatException or OverflowException or ArgumentException or UnauthorizedAccessException or DivideByZeroException)
{
    Console.Error.WriteLine(e.Message);
    return 1;
}

static void Check(bool valid, string message)
{
    if (!valid)
        throw new FormatException(message);
}

static string[] Fields(string line, int count)
{
    var fields = line.Split('\t', StringSplitOptions.TrimEntries);
    Check(fields.Length == count && fields.All(s => s.Length > 0), $"Expected {count} tab-separated values in '{line}'.");
    return fields;
}

static decimal Num(string value) => decimal.Parse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

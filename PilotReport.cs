static class PilotReport
{
    public static void Run(string[] args)
    {
        if (args.Length != 2)
            throw new FormatException("Usage: pilot study.tsv");
        var readings = Rows(args[1], 7).Select(r =>
        {
            var date = Day(r[0]);
            Require(r[2] is "baseline" or "pilot", "Unknown phase.");
            var land = Amount(r[3]);
            var rec = Amount(r[4]);
            var compost = Amount(r[5]);
            var rejected = Amount(r[6]);
            Require(rejected <= rec + compost && land + rec + compost > 0, "Invalid rejected mass or empty sample.");
            return (Date: date, Site: r[1], Phase: r[2], Total: land + rec + compost, Land: land + rejected);
        }).ToArray();
        foreach (var site in readings.GroupBy(t => t.Site))
        {
            var phases = site.GroupBy(t => t.Phase).ToDictionary(g => g.Key, g => (Mass: g.Sum(t => t.Total), Land: g.Sum(t => t.Land)));
            foreach (var g in site.OrderBy(t => t.Date))
                Console.WriteLine($"{g.Site} {g.Date:yyyy-MM-dd} {g.Phase}: {g.Land}/{g.Total} kg to landfill");
            if (!phases.TryGetValue("baseline", out var before) || !phases.TryGetValue("pilot", out var after))
            {
                Console.WriteLine($"{site.Key}: comparison incomplete");
                continue;
            }

            var adjusted = after.Land - before.Land / before.Mass * after.Mass;
            Console.WriteLine($"{site.Key}: raw landfill change {after.Land - before.Land:0.##} kg; same-mass change {adjusted:0.##} kg at {after.Mass} kg sampled; diversion change {100 * (before.Land / before.Mass - after.Land / after.Mass):0.##} percentage points");
        }
    }

    static List<string[]> Rows(string file, int width = 0, bool allowEmpty = false)
    {
        using var reader = file == "-" ? Console.In : File.OpenText(file);
        var rows = new List<string[]>();
        int line = 0;
        while (reader.ReadLine()is string s)
        {
            line++;
            if (string.IsNullOrWhiteSpace(s) || s.TrimStart().StartsWith("#"))
                continue;
            var cells = s.Split('\t', StringSplitOptions.TrimEntries);
            if ((width > 0 && cells.Length != width) || (!allowEmpty && cells.Any(string.IsNullOrEmpty)))
                throw new FormatException($"Invalid columns on line {line}.");
            rows.Add(cells);
        }

        if (rows.Count == 0)
            throw new FormatException("Empty input.");
        return rows;
    }

    static void Require(bool condition, string message)
    {
        if (!condition)
            throw new FormatException(message);
    }

    static decimal Amount(string text)
    {
        var value = decimal.Parse(text, System.Globalization.NumberStyles.AllowLeadingSign | System.Globalization.NumberStyles.AllowDecimalPoint);
        Require(value >= 0, "Quantity must be nonnegative.");
        return value;
    }

    static DateOnly Day(string s) => DateOnly.ParseExact(s, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
}

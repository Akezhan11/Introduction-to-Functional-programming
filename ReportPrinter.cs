using System.Globalization;

// ============================================================================
// TASK 3 - The output side. EVERY method here has a side effect: it writes to
// the console and returns nothing. All the calculation lives elsewhere, so
// this file is the only place that has to change if the program later writes
// to a file, a web page or a test log.
//
// InvariantCulture is used so the decimal separator is always '.', whatever
// the regional settings of the machine are.
// ============================================================================

public static class ReportPrinter
{
    public static void ShowHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }

    /// <summary>Task 3: a method whose only job is displaying one value.</summary>
    public static void ShowAverage(decimal average)
    {
        Console.WriteLine($"Average of valid scores : {Format(average)}");
    }

    public static void ShowIndividualResults(AnalysisReport report)
    {
        ShowHeader("Individual results");
        foreach (EntryResult entry in report.Entries)
            Console.WriteLine(entry.Line);
    }

    public static void ShowBonus(AnalysisReport report)
    {
        ShowHeader($"Bonus (+{ScoreRules.BonusPoints} for scores above {ScoreRules.BonusAppliesAbove})");

        bool any = false;
        foreach (EntryResult entry in report.Entries)
        {
            if (!entry.IsValid || entry.ScoreWithBonus == entry.Score)
                continue;

            Console.WriteLine($"{entry.Score} -> {entry.ScoreWithBonus}");
            any = true;
        }

        if (!any)
            Console.WriteLine("No entry qualified for a bonus.");
    }

    public static void ShowSummary(AnalysisReport report)
    {
        ShowHeader("Summary");
        Console.WriteLine($"Valid scores            : {report.ValidCount}");
        Console.WriteLine($"Rejected scores         : {report.RejectedCount}");
        Console.WriteLine($"Passed (>= {ScoreRules.DefaultPassThreshold})          : {report.PassedCount}");
        ShowAverage(report.Average);
        Console.WriteLine($"Average with bonus      : {Format(report.AverageWithBonus)}");
        Console.WriteLine($"Average, integer division: {report.IntegerAverage}   <- fractional part lost");
    }

    public static void ShowFractions(AnalysisReport report)
    {
        ShowHeader("Scores as a fraction of 1");
        foreach (EntryResult entry in report.Entries)
        {
            if (entry.IsValid)
                Console.WriteLine($"{entry.Score} -> {Format(ScoreMath.AsFraction(entry.Score))}");
        }
    }

    public static void ShowMessage(string message) => Console.WriteLine(message);

    private static string Format(decimal value) =>
        value.ToString("0.00", CultureInfo.InvariantCulture);
}

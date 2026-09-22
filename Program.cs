const int PassThreshold = ScoreRules.DefaultPassThreshold;

List<string> givenInputs = new(Baseline.Inputs);

Console.WriteLine("Score analyzer - Assignment 1, Week 1");
Console.WriteLine("  1  Analyze the given dataset");
Console.WriteLine("  2  Interactive mode (enter scores, 'done' to finish)");
Console.WriteLine("  3  Evidence: hidden dependency before and after refactoring");
Console.WriteLine("  4  Original starter code output (baseline for comparison)");
Console.Write("Choose 1-4: ");

string choice = (Console.ReadLine() ?? "1").Trim();

switch (choice)
{
    case "2":
        RunAnalysis(ReadInteractiveInputs(), "Interactive input");
        break;

    case "3":
        ShowPurityEvidence();
        break;

    case "4":
        ReportPrinter.ShowHeader("Original starter code output");
        Baseline.Run();
        break;

    default:
        RunAnalysis(givenInputs, "Given dataset");
        break;
}

void RunAnalysis(List<string> inputs, string title)
{
    ReportPrinter.ShowHeader(title + $" ({inputs.Count} entries)");

    AnalysisReport report = ScoreAnalyzer.Analyze(inputs, PassThreshold);

    ReportPrinter.ShowIndividualResults(report);
    ReportPrinter.ShowBonus(report);
    ReportPrinter.ShowFractions(report);
    ReportPrinter.ShowSummary(report);
}

List<string> ReadInteractiveInputs()
{
    List<string> collected = new();

    Console.WriteLine();
    Console.WriteLine("Enter one score per line. Type 'done' to finish.");

    while (true)
    {
        string? line = Console.ReadLine();

        if (line is null)
            break;

        if (line.Trim().Equals("done", StringComparison.OrdinalIgnoreCase))
            break;

        collected.Add(line);
    }

    return collected;
}

void ShowPurityEvidence()
{
    const string sample = "60";

    ReportPrinter.ShowHeader("Original function: hidden dependency");

    Baseline.PassThreshold = 50;
    ReportPrinter.ShowMessage($"external passThreshold = 50 -> Describe(\"{sample}\") = {Baseline.Describe(sample)}");

    Baseline.PassThreshold = 70;
    ReportPrinter.ShowMessage($"external passThreshold = 70 -> Describe(\"{sample}\") = {Baseline.Describe(sample)}");

    Baseline.PassThreshold = 50;
    ReportPrinter.ShowMessage("Same argument, different results: the function is not pure.");

    ReportPrinter.ShowHeader("Refactored function: explicit input only");

    int scoreValue = int.Parse(sample);
    ReportPrinter.ShowMessage($"Classify({sample}, 50) = {ScoreRules.Classify(scoreValue, 50)}");

    Baseline.PassThreshold = 70;
    ReportPrinter.ShowMessage($"Classify({sample}, 50) = {ScoreRules.Classify(scoreValue, 50)}   (after unrelated state changed)");
    Baseline.PassThreshold = 50;

    ReportPrinter.ShowMessage($"Classify({sample}, 70) = {ScoreRules.Classify(scoreValue, 70)}   (different result only because the ARGUMENT changed)");
    ReportPrinter.ShowMessage("Same arguments, same result: the function is pure.");
}

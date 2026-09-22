# Assignment 1 — Score Analyzer

- **Name:** _(fill in)_
- **Group:** _(fill in)_
- **Assignment:** Assignment 1 — Score analyzer refactoring
- **Academic week:** Week 1

---

## How to build and run

Requirements: .NET 10 SDK.

```
cd Introduction-to-Functional-programming
dotnet build
dotnet run
```

The program shows a menu:

| Option | What it does |
|---|---|
| 1 | Analyzes the required dataset and prints individual results + summary |
| 2 | Interactive mode: one score per line, `done` finishes input |
| 3 | Evidence of the hidden dependency before and after refactoring |
| 4 | Output of the unchanged starter code (baseline for comparison) |

Checks performed:

1. Option 4 and the "Individual results" block of option 1 are compared line by line — they are identical, so the refactoring preserved the baseline behaviour.
2. Option 3 shows the same input producing two different results in the original function and one stable result in the refactored one.
3. Option 2 was run with ` 95 `, `42`, `hello`, `200`, `done` to confirm that the interactive path uses the same validation and calculation functions.

Source files:

| File | Responsibility | Task |
|---|---|---|
| `Baseline.cs` | unchanged starter code, kept for comparison | starter |
| `ScoreValidation.cs` | `Validate` + `ValidationResult` | 1 |
| `ScoreRules.cs` | `Classify` (switch), `IsPassed`, `AddBonus` | 2, 4 |
| `ScoreMath.cs` | `ToDecimal`, `Sum`, `Average`, `IntegerAverage`, `AsFraction` | 3 |
| `ScoreAnalyzer.cs` | composes the pure functions into an `AnalysisReport` | 4 |
| `ReportPrinter.cs` | console output only | 3 |
| `Program.cs` | menu, user input, orchestration | 4 |

---

## Task 1 — Refactor validation

### Approach

The three consecutive checks of the starter code (`TryParse`, `score < 0`, `score > 100`) were replaced by a single call to `ScoreValidation.Validate`. The function takes the raw text and returns a `ValidationResult` value that carries three things:

- `IsValid` — whether the entry can be used in further processing,
- `Score` — the parsed number, meaningful only when `IsValid` is true,
- `Error` — `"Invalid integer"` or `"Out of range"`, empty when valid.

The distinction between the two error kinds is preserved because they are two different values of `Error`, produced by two different branches inside the one function.

### Why this makes validation reusable

The function returns **data**, not screen output, and it reads nothing except its parameter. Therefore the same call can be used by:

- the individual-result formatter (`ScoreRules.Describe`),
- the analyzer that counts valid/rejected entries and collects scores for the average,
- the interactive mode, which feeds user-typed lines through exactly the same function,
- a future unit test, which can assert on the returned value without capturing console output.

The checks are written **once**. Nothing was copied into several places: callers ask a question and branch on the answer instead of repeating the conditions.

### Listing — `ScoreValidation.cs`

```csharp
public readonly record struct ValidationResult(bool IsValid, int Score, string Error)
{
    public static ValidationResult Valid(int score) => new(true, score, "");

    public static ValidationResult Invalid(string error) => new(false, 0, error);
}

public static class ScoreValidation
{
    public const int MinScore = 0;
    public const int MaxScore = 100;

    public const string InvalidIntegerMessage = "Invalid integer";
    public const string OutOfRangeMessage = "Out of range";

    public static ValidationResult Validate(string? text)
    {
        if (!int.TryParse(text, out int score))
            return ValidationResult.Invalid(InvalidIntegerMessage);

        if (score < MinScore || score > MaxScore)
            return ValidationResult.Invalid(OutOfRangeMessage);

        return ValidationResult.Valid(score);
    }
}
```

Whitespace handling: `int.TryParse` uses `NumberStyles.Integer` by default, which allows leading and trailing whitespace, so `" 70 "` is accepted without any manual trimming.

---

## Task 2 — Decisions as pure functions

### Approach

`Classify(int score, int passThreshold)` takes the threshold as an explicit parameter and uses a `switch` statement with relational patterns and `when` guards. The case order reproduces the original `if / else if` chain, so results are unchanged for the same rules.

### Evidence of the hidden dependency (menu option 3)

```
=== Original function: hidden dependency ===
external passThreshold = 50 -> Describe("60") = 60: Satisfactory
external passThreshold = 70 -> Describe("60") = 60: Fail
Same argument, different results: the function is not pure.

=== Refactored function: explicit input only ===
Classify(60, 50) = Satisfactory
Classify(60, 50) = Satisfactory   (after unrelated state changed)
Classify(60, 70) = Fail   (different result only because the ARGUMENT changed)
Same arguments, same result: the function is pure.
```

**Explanation of the different results.** In the original, `Describe` reads `passThreshold` from the enclosing scope. The argument `"60"` did not change, but the answer did, because part of the function's real input is invisible in its signature. In the refactored version the threshold is one of the arguments: changing unrelated external state (`Baseline.PassThreshold`) has no effect, and the answer only changes when the argument itself changes.

### Is the original function pure?

No. It fails the first rule of purity: the same argument can yield different results, because the result depends on mutable state outside the parameter list. (It has no side effect of its own — it does not print or assign — but purity requires both properties.)

### What the refactoring changes

The dependency becomes visible in the signature. `Classify(int score, int passThreshold)` declares everything it needs, so the function can be read, reasoned about, reused and tested in isolation, and its result can be cached or reordered safely.

### Why replacing `if` with `switch` alone would not solve the problem

`if` and `switch` are two ways of writing the same branching. Purity is not about the shape of the branch, it is about where the data comes from. A `switch` whose guard still reads an outer mutable variable is exactly as impure as the `if` version. The fix is moving the threshold into the parameter list; the `switch` only makes the rule table easier to read.

### Which operations still have side effects

- every `Console.WriteLine` in `ReportPrinter`,
- `Console.ReadLine` in the interactive mode and in the menu (it also returns a different value each call, so it is not deterministic either),
- the assignments to `Baseline.PassThreshold` inside the purity demonstration,
- filling the `List<>` collections inside `ScoreAnalyzer` (local mutation, not observable from outside the function, so `Analyze` is still referentially transparent for callers).

Side effects are not a defect — a program with none would be useless. The point is that they are confined to `Program.cs` and `ReportPrinter.cs`, while every rule and calculation is pure.

### Listing — `ScoreRules.cs`

```csharp
public static class ScoreRules
{
    public const int DefaultPassThreshold = 50;
    public const int GoodFrom = 70;
    public const int ExcellentFrom = 90;

    public const int BonusAppliesAbove = 80;
    public const int BonusPoints = 2;

    public static string Classify(int score, int passThreshold)
    {
        switch (score)
        {
            case var s when s >= ExcellentFrom:
                return "Excellent";

            case var s when s >= GoodFrom:
                return "Good";

            case var s when s >= passThreshold:
                return "Satisfactory";

            default:
                return "Fail";
        }
    }

    public static string Describe(string? text, int passThreshold)
    {
        ValidationResult result = ScoreValidation.Validate(text);

        if (!result.IsValid)
            return result.Error;

        return $"{result.Score}: {Classify(result.Score, passThreshold)}";
    }

    public static bool IsPassed(int score, int passThreshold) => score >= passThreshold;

    public static int AddBonus(int score)
    {
        if (score <= BonusAppliesAbove)
            return score;

        int bonused = score + BonusPoints;
        return bonused > ScoreValidation.MaxScore ? ScoreValidation.MaxScore : bonused;
    }
}
```

---

## Task 3 — Conversion and representation

### Integer division

The valid scores of the dataset are 85, 70, 49, 100 and 0. Their sum is 304 and their count is 5.

- `304 / 5` with two `int` operands evaluates to **60**: C# picks integer division, and the remainder 4 is discarded — not rounded, simply dropped. The reported average would be 0.8 points too low.
- `ToDecimal(304) / ToDecimal(5)` evaluates to **60.8**, because at least one operand is `decimal`, so decimal division is used.

Both values are printed by the program so the difference is visible in the output.

`decimal` was chosen over `double` because it stores decimal fractions exactly, which is what a grade average should do.

### Value-returning method vs output method

- `ScoreMath.Average(scores)` computes and **returns** a `decimal`. It has no effect on the world; its result can be stored, compared, added to another calculation, or asserted in a test.
- `ReportPrinter.ShowAverage(average)` **returns nothing** and writes one line to the console. Its whole purpose is the side effect.

Keeping them apart matters because:

- **testing** — a test can call `Average` and compare the returned number directly; testing a method that only prints would require intercepting the console stream,
- **reuse** — the same `Average` serves the console output, the bonus average and any future file or GUI output,
- **change** — switching from console to a file or a web response touches only `ReportPrinter`.

### Listing — `ScoreMath.cs`

```csharp
public static class ScoreMath
{
    public static decimal ToDecimal(int value) => (decimal)value;

    public static int Sum(List<int> scores)
    {
        int total = 0;
        foreach (int score in scores)
            total += score;
        return total;
    }

    public static decimal Average(List<int> scores)
    {
        if (scores.Count == 0)
            return 0m;

        return ToDecimal(Sum(scores)) / ToDecimal(scores.Count);
    }

    public static int IntegerAverage(List<int> scores)
    {
        if (scores.Count == 0)
            return 0;

        return Sum(scores) / scores.Count;
    }

    public static decimal AsFraction(int score) =>
        ToDecimal(score) / ToDecimal(ScoreValidation.MaxScore);
}
```

`AsFraction` is the additional small pure calculation: it converts the score with `ToDecimal` before dividing by 100, so `85` becomes `0.85` instead of `0`.

### Listing — `ReportPrinter.cs` (output only)

```csharp
using System.Globalization;

public static class ReportPrinter
{
    public static void ShowHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine("=== " + title + " ===");
    }

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
```

---

## Task 4 — The complete analyzer

### Approach

`ScoreAnalyzer.Analyze` walks the entries once, calls `Validate`, `Classify`, `IsPassed` and `AddBonus`, and returns an `AnalysisReport` containing the per-entry lines and all summary numbers. It prints nothing, so the same analysis serves the dataset mode and the interactive mode. `Program.cs` picks the source of the entries and hands the report to `ReportPrinter`.

The input collection is preserved: `Baseline.Inputs` is copied into a new list and never modified.

**Assumption about the bonus.** The task requires the baseline output for individual entries to be preserved, so the individual lines keep the original score and label, and the bonus is reported in its own section together with a bonus-adjusted average. The bonus is capped at 100 so a bonused score stays inside the allowed range (100 therefore stays 100).

### Expected and actual output — required dataset

Expected individual results are the ten lines produced by the starter code. Actual output (menu option 1):

```
=== Given dataset (10 entries) ===

=== Individual results ===
85: Good
70: Good
49: Fail
100: Excellent
0: Fail
Invalid integer
Invalid integer
Invalid integer
Out of range
Out of range

=== Bonus (+2 for scores above 80) ===
85 -> 87

=== Scores as a fraction of 1 ===
85 -> 0.85
70 -> 0.70
49 -> 0.49
100 -> 1.00
0 -> 0.00

=== Summary ===
Valid scores            : 5
Rejected scores         : 5
Passed (>= 50)          : 3
Average of valid scores : 60.80
Average with bonus      : 61.20
Average, integer division: 60   <- fractional part lost
```

Baseline output (menu option 4) is identical to the "Individual results" block above, which is the evidence that behaviour was preserved.

Rejected entries and why: `"abc"` and `""` and `"72.5"` are not whole numbers (`Invalid integer`); `"-1"` and `"101"` parse but fall outside 0..100 (`Out of range`).

### Actual output — interactive mode

Input: ` 95 `, `42`, `hello`, `200`, `done`.

```
=== Interactive input (4 entries) ===

=== Individual results ===
95: Excellent
42: Fail
Invalid integer
Out of range

=== Bonus (+2 for scores above 80) ===
95 -> 97

=== Scores as a fraction of 1 ===
95 -> 0.95
42 -> 0.42

=== Summary ===
Valid scores            : 2
Rejected scores         : 2
Passed (>= 50)          : 1
Average of valid scores : 68.50
Average with bonus      : 69.50
Average, integer division: 68   <- fractional part lost
```

### Listing — `ScoreAnalyzer.cs`

```csharp
public readonly record struct EntryResult(
    string RawInput,
    bool IsValid,
    int Score,
    int ScoreWithBonus,
    string Line);

public readonly record struct AnalysisReport(
    List<EntryResult> Entries,
    int ValidCount,
    int RejectedCount,
    int PassedCount,
    decimal Average,
    int IntegerAverage,
    decimal AverageWithBonus);

public static class ScoreAnalyzer
{
    public static AnalysisReport Analyze(List<string> inputs, int passThreshold)
    {
        List<EntryResult> entries = new();
        List<int> validScores = new();
        List<int> bonusedScores = new();
        int passedCount = 0;

        foreach (string input in inputs)
        {
            ValidationResult validation = ScoreValidation.Validate(input);

            if (!validation.IsValid)
            {
                entries.Add(new EntryResult(input, false, 0, 0, validation.Error));
                continue;
            }

            int score = validation.Score;
            int withBonus = ScoreRules.AddBonus(score);
            string line = $"{score}: {ScoreRules.Classify(score, passThreshold)}";

            entries.Add(new EntryResult(input, true, score, withBonus, line));
            validScores.Add(score);
            bonusedScores.Add(withBonus);

            if (ScoreRules.IsPassed(score, passThreshold))
                passedCount++;
        }

        return new AnalysisReport(
            Entries: entries,
            ValidCount: validScores.Count,
            RejectedCount: inputs.Count - validScores.Count,
            PassedCount: passedCount,
            Average: ScoreMath.Average(validScores),
            IntegerAverage: ScoreMath.IntegerAverage(validScores),
            AverageWithBonus: ScoreMath.Average(bonusedScores));
    }
}
```

### Listing — `Program.cs`

```csharp
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
```

### Listing — `Baseline.cs` (unchanged starter code)

```csharp
public static class Baseline
{
    public static int PassThreshold = 50;

    public static readonly string[] Inputs =
    {
        "85", " 70 ", "49", "100", "0",
        "abc", "", "72.5", "-1", "101"
    };

    public static void Run()
    {
        foreach (string input in Inputs)
            Console.WriteLine(Describe(input));
    }

    public static string Describe(string text)
    {
        if (!int.TryParse(text, out int score)) return "Invalid integer";
        if (score < 0) return "Out of range";
        if (score > 100) return "Out of range";

        string label;
        if (score >= 90)
            label = "Excellent";
        else if (score >= 70)
            label = "Good";
        else if (score >= PassThreshold)
            label = "Satisfactory";
        else
            label = "Fail";
        return $"{score}: {label}";
    }
}
```

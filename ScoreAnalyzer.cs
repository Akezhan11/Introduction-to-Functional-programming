// ============================================================================
// TASK 4 - Composition. This class glues the pure pieces together and still
// prints nothing: it turns a list of raw entries into a data object that the
// printer can render. Same input -> same AnalysisReport, always.
// ============================================================================

/// <summary>One processed entry: the raw text, the baseline line and the bonus.</summary>
public readonly record struct EntryResult(
    string RawInput,
    bool IsValid,
    int Score,
    int ScoreWithBonus,
    string Line);

/// <summary>Everything the printer needs to show, computed and nothing else.</summary>
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
                // Rejected: the line is just the error message, as in the baseline.
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

// ============================================================================
// TASK 3 - Conversion and calculation. All functions here are pure:
// they take values in, return values out, and print nothing.
// ============================================================================

public static class ScoreMath
{
    /// <summary>
    /// Integer-to-decimal converter. Wrapping the conversion in a named method
    /// makes the intent explicit at every call site: from here on the value
    /// takes part in decimal arithmetic, so the fractional part survives.
    /// </summary>
    public static decimal ToDecimal(int value) => (decimal)value;

    /// <summary>Sum of a list of scores. Separate so it can be reused and tested.</summary>
    public static int Sum(List<int> scores)
    {
        int total = 0;
        foreach (int score in scores)
            total += score;
        return total;
    }

    /// <summary>
    /// Average that keeps the fractional part.
    /// Both operands are converted to decimal first - if they stayed int,
    /// C# would perform integer division and silently drop the remainder.
    /// </summary>
    public static decimal Average(List<int> scores)
    {
        if (scores.Count == 0)
            return 0m;

        return ToDecimal(Sum(scores)) / ToDecimal(scores.Count);
    }

    /// <summary>
    /// The same average computed with integer division, kept only as evidence
    /// for the report: 304 / 5 evaluates to 60, not 60.8.
    /// </summary>
    public static int IntegerAverage(List<int> scores)
    {
        if (scores.Count == 0)
            return 0;

        return Sum(scores) / scores.Count;
    }

    /// <summary>
    /// Expresses a score as a fraction of the maximum, e.g. 85 -> 0.85.
    /// Uses ToDecimal so the fractional part is preserved.
    /// </summary>
    public static decimal AsFraction(int score) =>
        ToDecimal(score) / ToDecimal(ScoreValidation.MaxScore);
}

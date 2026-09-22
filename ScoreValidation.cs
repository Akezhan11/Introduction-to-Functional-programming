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

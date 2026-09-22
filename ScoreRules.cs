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

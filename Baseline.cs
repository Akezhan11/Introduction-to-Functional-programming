 static class Baseline
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

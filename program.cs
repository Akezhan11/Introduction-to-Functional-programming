using System;
using System.Collections.Generic;

class Program
{
    static void Main()
    {
        string[] givenInputs =
        {
            "85",
            " 70 ",
            "49",
            "100",
            "0",
            "abc",
            "",
            "72.5",
            "-1",
            "101"
        };

        Console.WriteLine("Score Analyzer");
        Console.WriteLine("1 - Given inputs");
        Console.WriteLine("2 - Interactive mode");
        Console.Write("Choose mode: ");

        string choice = Console.ReadLine();

        string[] inputs;

        if (choice == "2")
        {
            List<string> collectedInputs = new List<string>();

            Console.WriteLine("Enter scores one per line.");
            Console.WriteLine("Enter 'done' to finish.");

            while (true)
            {
                string input = Console.ReadLine();

                if (input == "done")
                {
                    break;
                }

                collectedInputs.Add(input);
            }

            inputs = collectedInputs.ToArray();
        }
        else
        {
            inputs = givenInputs;
        }

        int validCount = 0;
        int rejectedCount = 0;
        int passedCount = 0;
        int sum = 0;

        const int passThreshold = 50;

        Console.WriteLine();
        Console.WriteLine("Individual results:");

        foreach (string input in inputs)
        {
            if (ValidateScore(input, out int score, out string error))
            {
                validCount++;

                int finalScore = AddBonus(score);

                string label = ClassifyScore(finalScore, passThreshold);

                Console.WriteLine($"{finalScore}: {label}");

                if (finalScore >= passThreshold)
                {
                    passedCount++;
                }

                sum += finalScore;
            }
            else
            {
                rejectedCount++;
                Console.WriteLine(error);
            }
        }

        decimal average = CalculateAverage(sum, validCount);

        Console.WriteLine();
        Console.WriteLine("Summary:");
        Console.WriteLine($"Valid count: {validCount}");
        Console.WriteLine($"Rejected count: {rejectedCount}");
        Console.WriteLine($"Passed count: {passedCount}");
        Console.WriteLine($"Average: {average}");

        Console.WriteLine();
        Console.WriteLine($"85 as fraction: {ScoreToFraction(85)}");
    }


    // TASK 1
    static bool ValidateScore(string text, out int score, out string error)
    {
        if (!int.TryParse(text, out score))
        {
            error = "Invalid integer";
            return false;
        }

        if (score < 0 || score > 100)
        {
            error = "Out of range";
            return false;
        }

        error = "";
        return true;
    }


    // TASK 2
    static string ClassifyScore(int score, int passThreshold)
    {
        switch (score)
        {
            case >= 90:
                return "Excellent";

            case >= 70:
                return "Good";

            case int s when s >= passThreshold:
                return "Satisfactory";

            default:
                return "Fail";
        }
    }


    // TASK 3
    static decimal ToDecimal(int value)
    {
        return (decimal)value;
    }


    static decimal CalculateAverage(int sum, int count)
    {
        if (count == 0)
        {
            return 0;
        }

        return ToDecimal(sum) / count;
    }


    static void DisplayAverage(decimal average)
    {
        Console.WriteLine($"Average: {average}");
    }


    static decimal ScoreToFraction(int score)
    {
        return ToDecimal(score) / 100;
    }


    // TASK 4
    static int AddBonus(int score)
    {
        if (score > 80)
        {
            return score + 2;
        }

        return score;
    }
}
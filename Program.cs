using System;

public delegate decimal DiscountRule(decimal subtotal);

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--test") return Tests();
        if (args.Length > 0 && args[0] == "--original-tests") return OriginalTests();
        if (args.Length > 0 && args[0] == "--debug-original")
        {
            decimal subtotal = 105m;
            decimal rate = Starter.Pricing.DiscountRate(subtotal);
            decimal discounted = subtotal - subtotal * rate;
            decimal shipping = Starter.Pricing.Shipping(subtotal, false);
            decimal actual = Starter.Pricing.Payable(subtotal, false);
            Console.WriteLine("Actual: " + actual + "; expected: 114.75");
            return 0;
        }

        DiscountRule standard = DiscountRate;
        DiscountRule noDiscount = subtotal => 0m;
        decimal[] prices = { 40m, 30m };
        int[] quantities = { 2, 1 };

        Console.WriteLine("Example 1: standard");
        ShowOrder(prices, quantities, standard);
        Console.WriteLine("Example 1: no discount");
        ShowOrder(prices, quantities, noDiscount);

        decimal[] secondPrices = { 50m };
        int[] secondQuantities = { 2 };
        Console.WriteLine("Example 2: standard");
        ShowOrder(secondPrices, secondQuantities, standard);
        Console.WriteLine("Example 2: no discount");
        ShowOrder(secondPrices, secondQuantities, noDiscount);
        return 0;
    }

    static decimal LineTotal(decimal price, int quantity)
    {
        return price * quantity;
    }

    static decimal Subtotal(decimal[] prices, int[] quantities)
    {
        decimal total = 0m;
        for (int i = 0; i < prices.Length; i++)
        {
            total = total + LineTotal(prices[i], quantities[i]);
        }
        return total;
    }

    static decimal DiscountRate(decimal subtotal)
    {
        if (subtotal >= 200m) return 0.10m;
        if (subtotal >= 100m) return 0.05m;
        return 0m;
    }

    static decimal DiscountAmount(decimal subtotal, DiscountRule rule)
    {
        return subtotal * rule(subtotal);
    }

    static decimal DiscountedSubtotal(decimal subtotal, DiscountRule rule)
    {
        return subtotal - DiscountAmount(subtotal, rule);
    }

    static decimal Shipping(decimal discounted, bool isEmpty)
    {
        if (isEmpty) return 0m;
        if (discounted >= 100m) return 0m;
        return 15m;
    }

    static decimal Payable(decimal subtotal, bool isEmpty, DiscountRule rule)
    {
        decimal discounted = DiscountedSubtotal(subtotal, rule);
        return discounted + Shipping(discounted, isEmpty);
    }

    static void ShowOrder(decimal[] prices, int[] quantities, DiscountRule rule)
    {
        decimal subtotal = Subtotal(prices, quantities);
        decimal discounted = DiscountedSubtotal(subtotal, rule);
        bool isEmpty = prices.Length == 0;
        Action<string> display = Console.WriteLine;

        display("Subtotal: " + subtotal.ToString("F2"));
        display("Discount amount: " + DiscountAmount(subtotal, rule).ToString("F2"));
        display("Discounted subtotal: " + discounted.ToString("F2"));
        display("Shipping: " + Shipping(discounted, isEmpty).ToString("F2"));
        display("Payable: " + Payable(subtotal, isEmpty, rule).ToString("F2"));
        display("");
    }

    static int Check(string name, bool passed)
    {
        if (passed)
        {
            Console.WriteLine("PASS " + name);
            return 0;
        }
        Console.WriteLine("FAIL " + name);
        return 1;
    }

    static int Summary(int total, int failed)
    {
        Console.WriteLine("Passed: " + (total - failed));
        Console.WriteLine("Failed: " + failed);
        if (failed > 0) return 1;
        return 0;
    }

    static int Tests()
    {
        DiscountRule standard = DiscountRate;
        DiscountRule noDiscount = subtotal => 0m;
        decimal[] prices = { 40m, 30m };
        int[] quantities = { 2, 1 };
        decimal[] emptyPrices = { };
        int[] emptyQuantities = { };
        int failed = 0;

        failed += Check("R1 line totals", LineTotal(40m, 2) == 80m
            && LineTotal(2.35m, 3) == 7.05m);
        failed += Check("T2 collection subtotal", Subtotal(prices, quantities) == 110m);
        failed += Check("R2 discount boundaries", DiscountRate(99.99m) == 0m
            && DiscountRate(100m) == 0.05m && DiscountRate(199.99m) == 0.05m
            && DiscountRate(200m) == 0.10m);
        failed += Check("R3 empty order", Subtotal(emptyPrices, emptyQuantities) == 0m
            && Shipping(0m, true) == 0m && Payable(0m, true, standard) == 0m
            && Payable(0m, true, noDiscount) == 0m);
        failed += Check("R4 shipping boundary", Shipping(99.99m, false) == 15m
            && Shipping(100m, false) == 0m && Shipping(100.01m, false) == 0m);
        failed += Check("R5 standard payable", Payable(110m, false, standard) == 104.50m);
        failed += Check("R6 shipping after discount", Payable(105m, false, standard) == 114.75m);
        failed += Check("T8 no discount", Payable(110m, false, noDiscount) == 110m
            && Payable(200m, false, noDiscount) == 200m);
        failed += Check("T9 second example", Payable(100m, false, standard) == 110m
            && Payable(100m, false, noDiscount) == 100m);
        failed += Check("T10 no early rounding", DiscountAmount(100.01m, standard) == 5.0005m
            && Payable(100.01m, false, standard) == 110.0095m);

        decimal total = Subtotal(prices, quantities);
        Payable(total, false, standard);
        Payable(total, false, noDiscount);
        failed += Check("T11 input unchanged", prices.Length == 2 && quantities.Length == 2
            && prices[0] == 40m && prices[1] == 30m
            && quantities[0] == 2 && quantities[1] == 1);

        decimal first = Payable(Subtotal(prices, quantities), false, standard);
        Payable(Subtotal(prices, quantities), false, noDiscount);
        decimal second = Payable(Subtotal(prices, quantities), false, standard);
        failed += Check("T12 repeated calls", first == 104.50m && second == 104.50m);
        return Summary(12, failed);
    }

    static int OriginalTests()
    {
        int failed = 0;
        failed += Check("R1 line total: expected 80", Starter.Pricing.LineTotal(40m, 2) == 80m);
        failed += Check("R2 rate at 200: expected 0.10", Starter.Pricing.DiscountRate(200m) == 0.10m);
        failed += Check("R3 empty shipping: expected 0", Starter.Pricing.Shipping(0m, true) == 0m);
        failed += Check("R4 shipping at 100: expected 0", Starter.Pricing.Shipping(100m, false) == 0m);
        failed += Check("R5 payable at 110: expected 104.50", Starter.Pricing.Payable(110m, false) == 104.50m);
        failed += Check("R6 payable at 105: expected 114.75", Starter.Pricing.Payable(105m, false) == 114.75m);
        return Summary(6, failed);
    }
}

namespace Starter
{
    public static class Pricing
    {
        public static decimal LineTotal(decimal price, int quantity)
            => price + quantity;

        public static decimal DiscountRate(decimal subtotal)
        {
            if (subtotal >= 100m) return 0.05m;
            if (subtotal >= 200m) return 0.10m;
            return 0m;
        }

        public static decimal Shipping(decimal discounted, bool isEmpty)
            => discounted > 100m ? 0m : 15m;

        public static decimal Payable(decimal subtotal, bool isEmpty)
            => subtotal - DiscountRate(subtotal)
               + Shipping(subtotal, isEmpty);
    }
}
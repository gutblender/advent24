using System.Runtime.CompilerServices;

namespace day11_pebbles;
using NumberType = UInt128;

internal class Program
{
    const int NumBlinks = 25;
    const int NumBlinksPart2 = 75;
    static readonly NumberType Multiplier = 2024UL;
    static readonly NumberType[] PowTen; // starts with 10, index is 1-1 based
    static Program()
    {
        List<NumberType> powTenList = [];
        for (NumberType num = 10, last = 1; num/10 >= last; num *= 10)
        {
            powTenList.Add(last = num);
        }
        PowTen = powTenList.ToArray();

        // test 
        NumberType testnum = 6;
        for (int numDigits = 1; numDigits <= PowTen.Length; numDigits++, testnum *= 10)
        {
            int order = NumDigits(testnum);
            if (numDigits != order)
                throw new();
        }
    }
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World! Day 11 here.\nPaste Input: ");

        if (!(Console.ReadLine() is string line && !string.IsNullOrEmpty(line)))
            return;

        var stones = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(item => NumberType.Parse(item))
            .ToList();

        // initialize counts dicts
        Dictionary<NumberType, NumberType> counts = [];
        var newCounts = counts.ToDictionary();

        foreach (var stone in stones)
        {
            counts.TryAdd(stone, 1);
        }

        void Blink()
        {
            // each blink, swizzle the stone counts, don't double-process stones.

            // process 0s and 1s
            if (counts.TryGetValue(0, out NumberType zeroCount))
            {
                newCounts[1] = zeroCount; // turn those into 1s
                counts.Remove(0);
            }

            foreach (var pair in counts) checked
            {
                int order;
                if (0 != ((order = NumDigits(pair.Key)) % 2))
                    newCounts.PlusEqual(Multiplier * pair.Key, pair.Value);
                else
                {
                    // split the number.
                    NumberType splitter = PowTen[order / 2 - 1];
                    NumberType right = pair.Key % splitter;
                    NumberType left = pair.Key / splitter;
                    newCounts.PlusEqual(left, pair.Value);
                    newCounts.PlusEqual(right, pair.Value);
                }
            }
            counts.Clear();

            // swap the references
            (newCounts, counts) = (counts, newCounts);
        }
        NumberType Count()
        {
            NumberType total = 0;
            foreach (var pair in counts)
                total += pair.Value;
            return total;
        }

        int blink;
        for (blink = 0; blink < NumBlinks; blink++)
            Blink();

        Console.WriteLine($"Part 1 solution: {Count()}");

        while (blink++ < NumBlinksPart2)
            Blink();

        Console.WriteLine($"Part 2 solution: {Count()}");
    }

    static int NumDigits(NumberType num)
    {
        int exp;
        for (exp = 0; exp < PowTen.Length; exp++)
        {
            if (PowTen[exp] > num)
                break;
        }
        return 1 + exp;
    }
}

static class Extensions
{
    public static NumberType PlusEqual<TKey>(this Dictionary<TKey, NumberType> dict, TKey index, NumberType addend)
        where TKey : notnull
    {
        if (dict.TryGetValue(index, out NumberType existing))
        {
            NumberType sum = existing + addend;
            dict[index] = sum;
            return sum;
        }
        else
        {
            dict[index] = addend;
            return addend;
        }
    }
}

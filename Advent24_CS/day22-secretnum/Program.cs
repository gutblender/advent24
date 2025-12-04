namespace day22_secretnum;

internal class Program
{
    const ulong PRUNE = 0x1000000;
    const int NumIterations = 2000;
    const int NumChanges = 4; // part 2 component
    const int InvalidBananas = -1;
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World! Day 22 here.\nPaste Input: ");

        ulong Mutate(ulong num)
        {
            num ^= (num << 6);
            num %= PRUNE;

            num ^= (num >> 5);
            num %= PRUNE;

            num ^= (num << 11);
            num %= PRUNE;
            return num;
        }

        // Part 2 solution:
        // enumerate all sales strategies according to a Uint32 encoding
        // which is the sbyte changes for each of NumChanges mutations
        // happening to be 4, which will simply multiply into the encoding
        // as base-19 numbers. Instead of a dictionary use an array for speed.
        // from -9 to +9 changes only, there are 19^4 = 130k strategies.
        const int NumStrategies = 19 * 19 * 19 * 19; // 19 ^ NumChanges
        Span<int> strategyTotals = stackalloc int[NumStrategies]; // TODO: stack size 1MB might be problematic.

        int EncodeChange(int change) => (change + 9); // this should now be positive. good for indices
        int AddChange(int sequence, int change) => (sequence * 19 + EncodeChange(change)) % NumStrategies; // 19^5 is still only 2.4M.

        ulong total = 0; // part 1 total
        int bestStrategy = 0, mostBananas = 0; // best sequence, find while calculating.

        // every line needs to find best possible outcome for each sequence. Ensure this gets cleared.
        Span<int> strategy = stackalloc int[NumStrategies]; // TODO: stack size 1MB might be problematic.

        for (string? line
            ; !string.IsNullOrEmpty(line = Console.ReadLine())
            && ulong.TryParse(line, out ulong num)
            ; strategy.Fill(InvalidBananas))
        {
            int iters, seq = 0, lastBananas = 0;
            for (iters = 0; iters < NumChanges - 1; iters++)
            {// initialize the sequence first
                ulong num2 = Mutate(num);
                int bananas = (int)(num2 % 10);
                int change = bananas - lastBananas;
                seq = AddChange(seq, change);
                num = num2;
                lastBananas = bananas;
            }
            for ( ; iters < NumIterations; iters++)
            {
                ulong num2 = Mutate(num);
                int bananas = (int)(num2 % 10);
                int change = bananas - lastBananas;
                seq = AddChange(seq, change);
                // the monkey will act the first time it sees the sequence;
                // so only write to each location ONCE.
                if (strategy[seq] == InvalidBananas) 
                    strategy[seq] = bananas;
                num = num2;
                lastBananas = bananas;
            }
            total += num; // add up part 1 solution

            for (int i = 0; i < NumStrategies; i++)
                if (strategy[i] != InvalidBananas)
                    strategyTotals[i] += strategy[i];
        }

        for (int i = 0; i < NumStrategies; i++)
        {
            if (mostBananas < strategyTotals[i])
            {
                mostBananas = strategyTotals[i];
                bestStrategy = i;
            }
        }

        Console.WriteLine($"Part 1 Solution: {total}");
        Console.WriteLine($"Part 2 Solution: {mostBananas}");
    }
}

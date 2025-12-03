using System.Text.RegularExpressions;

namespace day24_logicgates;

enum Operators
{
    AND,
    OR,
    XOR
}

internal class Program
{
    static readonly Regex RegWireStatus = new(@"(?<wire>[a-z0-9]+): (?<value>[01])");
    static readonly Regex RegEquation = new(@"(?<left>[a-z0-9]+) (?<operator>AND|OR|XOR) (?<right>[a-z0-9]+) -> (?<output>[a-z0-9]+)");
    static readonly Regex RegOutput = new(@"z(?<number>[0-9]+)");
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World! Day 24 here.\nPaste input below:");

        Dictionary<string, bool> signals = [];
        List<Equation> equations = [];

        for (string? line;
            !string.IsNullOrWhiteSpace(line = Console.ReadLine());)
        {
            var matchWire = RegWireStatus.Match(line);
            if (!matchWire.Success)
                break;

            var wire = matchWire.Groups["wire"].Value;
            bool value = matchWire.Groups["value"].Value == "1";
            signals[wire] = value;
        }

        // get equations
        for (string? line;
            !string.IsNullOrWhiteSpace(line = Console.ReadLine());)
        {
            var matchEqn = RegEquation.Match(line);
            if (!matchEqn.Success)
                break;

            Operators op = matchEqn.Groups["operator"].Value switch
            {
                "AND" => Operators.AND,
                "OR" => Operators.OR,
                "XOR" => Operators.XOR,
                _ => throw new Exception("Unknown operator")
            };
            var eqn = new Equation(
                matchEqn.Groups["left"].Value,
                matchEqn.Groups["right"].Value,
                op,
                matchEqn.Groups["output"].Value
            );
            equations.Add(eqn);
        }

        // add up z - Part 1
        ulong z = Solve(equations, signals);

        Console.WriteLine($"Part 1 z: {z}");

        // there exist equation pairs which are useless to attempt swapping
        // because their two outputs never appear uniquely
        // meaning they ONLY appear in equations together as inputs,
        // so swapping them does nothing.
        // be sure to double-reference them for easy lookups, i.e. dict[A] = B and dict[B] = A.
        // TBH, this sounds pretty stringent so maybe I'll skip this step and leave this empty.
        Dictionary<Equation, Equation> ForbiddenSwaps = [];

        Func<int, string> GetXWireName = bit => $"x{bit:D2}";
        Func<int, string> GetYWireName = bit => $"y{bit:D2}";
        //Func<int, string> GetZWireName = bit => $"z{bit:D2}";
        ulong x = BuildNum(signals, GetXWireName);
        ulong y = BuildNum(signals, GetYWireName);
        ulong ShouldZ = x + y;

        // solution part 2 data structures
        const int NumSwaps = 4;
        //HashSet<Equation> SwappingEqns = [];
        Swap[] Swaps = new Swap[NumSwaps];

        // solution part 2 tools which act upon the data structures
        int FindFirstAvailableEquationNotSwapped(int startIndex = 0)
        {
            for (int i = startIndex; i < equations.Count; i++)
            {
                Equation eqn = equations[i];
                if (!eqn.IsSwapped) //SwappingEqns.Contains(eqn))
                    return i;
            }
            return -1;
        }
        void BreakSwap(int swapIndex)
        {
            var swap = Swaps[swapIndex];
            Equation eLeft = equations[swap.Left];
            Equation eRight = equations[swap.Right];
            eLeft.SwapNone();
            eRight.SwapNone();
            //SwappingEqns.Remove(eLeft);
            //SwappingEqns.Remove(eRight);
        }
        void MakeSwap(int swapIndex)
        {
            var swap = Swaps[swapIndex];
            Equation eLeft = equations[swap.Left];
            Equation eRight = equations[swap.Right];
            eLeft.Swap(eRight);
            //SwappingEqns.Add(eLeft);
            //SwappingEqns.Add(eRight);
        }

        // Solution Part 2
        int swap;
        for (swap = 0; swap < 4; swap++) // init swaps
        {
            int left = 2 * swap, right = left + 1;
            Swaps[swap] = new() { Left = left, Right = right };
            MakeSwap(swap);
        }

        List<Swap[]> solutions = [];

        var rand = new Random();
        for (bool ok = true; ok;)
        {
            // try this solution.
            if (ShouldZ == Solve(equations, signals, ShouldZ))
            { // found a solution!
                // simply select outputs, swapped or not doesn't matter, and sort them.
                solutions.Add(Swaps.ToArray()); // these should copy by value.
            }

            // setup for next solution:
            // as this loop continues, it must break increasingly leftward swap relationships
            // to make room for those right. 
            // it will typically stay right as long as it can remake a new swap there. 
            // always start at the end - first swap to "increment"
            for (swap = NumSwaps - 1; swap >= 0; swap--)
            {
                BreakSwap(swap); // break relationships

                int next;
                if (0 <= (next = FindFirstAvailableEquationNotSwapped(Swaps[swap].Right + 1)))
                { // move the right.
                    Swaps[swap].Right = next;
                    break; // this swap is now complete, next loop will remake this and all following swaps.
                }
                else if (0 <= (next = FindFirstAvailableEquationNotSwapped(Swaps[swap].Left + 1)))
                { // overflow, have to move the left up
                    Swaps[swap].Left = next;
                    // now find a new right.
                    int newRight;
                    if (0 <= (newRight = FindFirstAvailableEquationNotSwapped(Swaps[swap].Left + 1)))
                    {
                        Swaps[swap].Right = newRight;
                        break; // this swap is now complete, next loop will remake this and all following swaps.
                    }
                }

                // 
            }
            if (swap < 0)
                break; // all combinations exhausted. the set should be empty.

            // where the break-swap loop left off, that swap is good just needs to be remade.
            MakeSwap(swap);

            // now rebuild after this swap index.
            // never go left of your leftmost set swap.
            for (int floor = Swaps[swap].Left + 1; ok && ++swap < NumSwaps;)
            {
                int left = FindFirstAvailableEquationNotSwapped(floor);
                if (left < 0) 
                    break; // no more equations to swap.
                int right = FindFirstAvailableEquationNotSwapped(left + 1);
                if (right < 0) 
                    break; // no more equations to swap.

                Swaps[swap].Left = left;
                Swaps[swap].Right = right;
                MakeSwap(swap);

                floor = right + 1;
            }
            if (swap < NumSwaps)
                break; // ran out of swaps. The set will be incomplete.
        }

        for (List<Swap[]> remove = []; solutions.Count > 1; remove.Clear())
        {
            // come up with new random x + y = z. TODO mask off x & y programmatically according to # bits given...
            // the system just doesn't have enough logic wires to handle bits above those.
            x = (ulong)rand.NextInt64() & ((1UL << 45) - 1);
            y = (ulong)rand.NextInt64() & ((1UL << 45) - 1);
            ShouldZ = x + y;

            // rebuild signals for new x and y.
            signals.Clear();
            BuildWires(x, signals, GetXWireName);
            BuildWires(y, signals, GetYWireName);

            foreach (var soln in solutions)
            {
                // clear all swaps.
                foreach (var eqn in equations)
                    eqn.SwapNone();

                // make all the swaps
                Swaps = soln;
                for (int i = 0; i < Swaps.Length; i++)
                    MakeSwap(i);
                if (ShouldZ != Solve(equations, signals, ShouldZ))
                    remove.Add(soln); // you suck
            }
            foreach (var rem in remove)
                solutions.Remove(rem);
        }

        string readout = string.Join("\n\t", solutions.Select(soln => string.Join(',', soln)));
        Console.WriteLine($"Part 2 solutions:\n\t{readout}");
    }

    struct Swap
    { 
        public int Left, Right; 
        public Swap() { Clear(); }
        public void Clear() { Left = -1; Right = -1; }
    }

    static ulong BuildNum(Dictionary<string, bool> wires, Func<int, string> GetWireName)
    {
        ulong z = 0;
        for (int bit = 0; bit < 8 * sizeof(ulong); bit++)
        {
            string wireName = GetWireName(bit);
            if (wires.TryGetValue(wireName, out bool value) && value)
                z |= 1UL << bit;
        }
        return z;
    }
    static void BuildWires(ulong num, Dictionary<string, bool> wires, Func<int, string> GetWireName)
    {
        for (int bit = 0; bit < 8 * sizeof(ulong); bit++, num >>= 1)
        {
            string wireName = GetWireName(bit);
            wires[wireName] = (num & 1UL) != 0;
        }
    }

    static ulong Solve(IReadOnlyList<Equation> equations, IReadOnlyDictionary<string, bool> sigs
        , ulong? expected = null)
    {
        var signals = sigs.ToDictionary();
        // "eat" equations
        for (List<Equation> eqns = equations.ToList(), toRemove = []; (eqns.Count > 0); toRemove.Clear())
        {
            foreach (var eqn in eqns)
            {
                if (signals.TryGetValue(eqn.A, out bool a) && signals.TryGetValue(eqn.B, out bool b))
                {
                    bool result = eqn.Operator switch
                    {
                        Operators.AND => a && b,
                        Operators.OR => a || b,
                        Operators.XOR => a != b,
                        _ => throw new Exception("Unknown operator")
                    };

                    if (expected is not null)
                    {
                        var match = RegOutput.Match(eqn.Output);
                        if (match.Success && int.TryParse(match.Groups["number"].Value, out int bit)
                            && bit >= 0 && bit < 8*sizeof(ulong))
                        {
                            bool expectedBit = ((expected.Value >> bit) & 1) != 0;
                            if (result != expectedBit)
                                return ~expected.Value; // this already ain't gonna work.
                        }
                    }

                    signals[eqn.Output] = result;
                    toRemove.Add(eqn);

                }
            }
            if (0 >= toRemove.Count)
                return expected.HasValue ? expected.Value : 0; // circular!
            foreach (var rem in toRemove)
                eqns.Remove(rem);
        }

        // add up z
        ulong z = BuildNum(signals, bit => $"z{bit:D2}");
        return z;
    }
}

class Equation(string a, string b, Operators op, string output)
{
    public readonly string A = a, B = b;
    private readonly string myout = output;
    public readonly Operators Operator = op;
    public bool HasInput(string input) => A == input || B == input;

    public bool IsSwapped => swap is not null;
    private Equation? swap = null;
    public string Output => swap?.myout ?? myout;
    public void SwapNone() => Swap(null);
    public void Swap(Equation? other)
    {
        if (swap is not null) swap.swap = null;
        swap = other;
        if (swap is not null)
        {
            if (swap.swap is not null)
                throw new InvalidOperationException("Equation already swapped");
            swap.swap = this;
        }
    }
}
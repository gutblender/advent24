using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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

        for (string line;
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

        // there exist equation pairs which are useless to attempt swapping
        // because their two outputs never appear uniquely
        // meaning they ONLY appear in equations together as inputs,
        // so swapping them does nothing.
        // be sure to double-reference them for easy lookups, i.e. dict[A] = B and dict[B] = A.
        // TBH, this sounds pretty stringent so maybe I'll skip this step and leave this empty.
        //Dictionary<Equation, Equation> ForbiddenSwaps = [];

        //ulong x = BuildNum(signals, bit => $"x{bit:D2}");
        //ulong y = BuildNum(signals, bit => $"y{bit:D2}");
        //ulong ShouldZ = x + y;

        //const int NumSwaps = 4;
        //HashSet<Equation> SwappingEqns = [];
        //Tuple<int, int>[] Swaps = new Tuple<int, int>[NumSwaps];
        //int swap;
        //for (swap = 0; swap < 4; swap++) // init
        //{
        //    int left = 2 * swap, right = left + 1;
        //    Swaps[swap] = Tuple.Create(2 * swap, 2 * swap + 1);
        //    Equation eLeft = equations[left];
        //    Equation eRight = equations[right];
        //    eLeft.Swap(eRight);
        //    SwappingEqns.Add(eLeft);
        //    SwappingEqns.Add(eRight);
        //}

        //swap = NumSwaps - 1; // start at the end
        //for (bool solved = false; !solved; )
        //{
        //    // try this solution. 

        //    // setup for next solution.
        //}

        // add up z
        ulong z = Solve(equations, signals);

        Console.WriteLine($"Part 1 z value: {z}");
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
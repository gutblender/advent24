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
        for (string line;
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

        // "eat" equations
        while (equations.Count > 0)
        {
            var toRemove = new List<Equation>();
            foreach (var eqn in equations)
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
                    signals[eqn.Output] = result;
                    toRemove.Add(eqn);
                }
            }
            foreach (var rem in toRemove)
                equations.Remove(rem);
        }

        // add up z
        ulong z = 0;
        for (int bit = 0; bit < 8*sizeof(ulong); bit++)
        {
            string wireName = $"z{bit:D2}";
            if (signals.TryGetValue(wireName, out bool value) && value)
                z |= 1UL << bit;
        }

        Console.WriteLine($"Final z value: {z}");
    }
}

class Equation(string a, string b, Operators op, string output)
{
    public readonly string A = a, B = b, Output = output;
    public readonly Operators Operator = op;
    public bool HasInput(string input) => A == input || B == input;
}
using System.Text.RegularExpressions;

namespace day7_operators
{
    internal class Program
    {
        static readonly Regex regLine = new(@"^(?<res>[0-9]+):(?<vals>( [0-9]+)+)$");

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Problem #7 Here.\n");
            Console.WriteLine("Paste your input below, and hit Enter a couple times to input a blank line to trigger processing:\n");

            List<Line> lines = new();
            int lineno = 0;
            for (string? text; !string.IsNullOrWhiteSpace(text = Console.ReadLine()); lineno++)
            {
                var match = regLine.Match(text);
                ulong res = ulong.Parse(match.Groups["res"].Value);

                ulong[] vals = match.Groups["vals"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(v => ulong.Parse(v)).ToArray();

                Line line = new(lineno, res, vals);
                lines.Add(line);
            }

            ulong sum = 0;
            for (uint part = 1, numOps = 2; part <= 2; part++, numOps++, sum = 0)
            {
                foreach (var line in lines)
                {
                    // try operators:
                    int nv = line.vals.Length; // number of vals. Num of operators is this minus one.
                    uint limit = 1;
                    for (int pow = 1; pow < nv; pow++, limit *= numOps) ;

                    for (uint trymask = 0; trymask < limit; trymask++)
                    { // try all combos
                        ulong res = line.vals[0];

                        uint iv = 1;
                        for (uint instruction = trymask
                            ; iv < nv 
                            ; iv++, instruction /= numOps)
                        { 
                            // now do the math
                            switch (instruction % numOps)
                            {
                                case 0: res += line.vals[iv]; break;
                                case 1: res *= line.vals[iv]; break;
                                case 2:
                                    res = ulong.Parse( // concat
                                        res.ToString() + line.vals[iv].ToString());
                                    break;

                                default: throw new NotImplementedException();
                            }
                        }

                        if (iv == nv && res == line.result)
                        {
                            sum += res;
                            break;
                        }
                    }
                }

                Console.WriteLine($"Part {part}: I found a total of {sum}.");
            }

        }

        struct Line(int lno, ulong res, ulong[] v)
        {
            public readonly int lineno = lno;
            public readonly ulong result = res;
            public readonly ulong[] vals = v;
        }
    }
}

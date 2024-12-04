using System.Text.RegularExpressions;

namespace day3_corruption
{
    internal class Program
    {
        static readonly Regex regMul = new Regex(@"mul\((?<f1>[0-9]{1,3}),(?<f2>[0-9]{1,3})\)");
        static readonly Regex regDont = new Regex(@"don't\(\)(?<stuff>.*?)do\(\)");
        static readonly Regex regNever = new Regex(@"don't\(\)(?<stuff>((?!do\(\)).)*?)$");
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Problem #3 Here.\n");
            Console.WriteLine("Paste your input below, and hit Enter a couple times to input a blank line to trigger processing:\n");


            int ubersum = 0;
            int uberDont = 0;
            string content = "";
            for (string line; !string.IsNullOrWhiteSpace(line = Console.ReadLine());)
                content += line;

            content.ReplaceLineEndings("");

            {
                int sum = GetSum(content);
                Console.WriteLine($"\nOk, those instructions totaled {sum}.");
                ubersum += sum;

                int sumDont = 0;
                foreach (var match in regDont.Matches(content).Cast<Match>())
                {
                    int dont = GetSum(match.Groups["stuff"].Value);
                    sumDont += dont;
                }
                foreach (var match in regNever.Matches(content).Cast<Match>())
                {
                    if (match.Value.Contains("do()"))
                        continue;
                    int dont = GetSum(match.Groups["stuff"].Value);
                    sumDont += dont;
                }


                Console.WriteLine($"You might want to exclude {sumDont} from that.");
                Console.WriteLine("Keep going for more!\n");
                uberDont += sumDont;
            }
            Console.WriteLine($"\nPhew, ALL instructions totaled {ubersum}!");
            Console.WriteLine($"Excluded instructions totaled {uberDont}!");
            Console.WriteLine("Bye now!");
        }

        static int GetSum(string line)
        {
            int sum = 0;
            foreach (var match in regMul.Matches(line).Cast<Match>())
            {
                int f1 = int.Parse(match.Groups["f1"].Value);
                int f2 = int.Parse(match.Groups["f2"].Value);
                int prod = f1 * f2;
                sum += prod;
            }
            return sum;
        }
    }
}

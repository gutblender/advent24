using System.Collections.Generic;
using System.Linq;

namespace day2_reports
{
    using IntType = int;
    using ListType = List<int>;

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Problem #2\n");
            Console.WriteLine("Paste your input below, and hit Enter a couple times to input a blank line to trigger processing:\n");

            int safe = 0, barely = 0;
            for (string line; !string.IsNullOrWhiteSpace(line = Console.ReadLine()); )
            {
                var split = line.Split(' ');
                var report = split.Select(s => IntType.Parse(s)).ToList();

                if (IsSafe(report))
                    safe++;
                else
                {
                    for (int i = 0; i < report.Count; i++)
                        if (IsSafe(Skipper(report, i)))
                        {
                            barely++;
                            break;
                        }
                }
            }
            Console.WriteLine($"{safe} Reports are safe.");
            Console.WriteLine($"{barely} Reports are BARELY made safe by the Problem Dampener.");
            Console.WriteLine($"Now {safe+barely} Reports are Safe!\n");
        }
        static ListType Skipper(ListType list, int iSkip)
        {
            ListType ret = new ListType(list);
            ret.RemoveAt(iSkip);
            return ret;
        }
        static IEnumerable<IntType> GetDiffTrain(ListType list)
        {
            if (list.Count > 1)
            {
                IntType num = list[0];
                for (int i = 1; i < list.Count; i++)
                {
                    yield return list[i] - num;
                    num = list[i];
                }
            }
        }

        //static int CntNotDescending(ListType list) => GetDiffTrain(list).Count(d => d >= 0);
        //static int CntNotAscending(ListType list) => GetDiffTrain(list).Count(d => d <= 0);
        //static int CntNotGE1(ListType list) => GetDiffTrain(list).Count(d => Math.Abs(d) < 1);
        //static int CntNotLE3(ListType list) => GetDiffTrain(list).Count(d => Math.Abs(d) > 3);
        //static int CntNotSafe(ListType list) => Math.Min(CntNotAscending(list), CntNotDescending(list)) + CntNotGE1(list) + CntNotLE3(list);
        static bool IsDescending(ListType list) => !GetDiffTrain(list).Any(d => d >= 0);
        static bool IsAscending(ListType list) => !GetDiffTrain(list).Any(d => d <= 0);
        static bool IsGE1(ListType list) => !GetDiffTrain(list).Any(d => Math.Abs(d) < 1);
        static bool IsLE3(ListType list) => !GetDiffTrain(list).Any(d => Math.Abs(d) > 3);
        static bool IsSafe(ListType list) => (IsAscending(list) || IsDescending(list)) && IsGE1(list) && IsLE3(list);
    }
}

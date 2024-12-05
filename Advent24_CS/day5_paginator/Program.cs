using System.Text.RegularExpressions;

namespace day5_paginator
{
    using SetType = HashSet<int>;
    using ListType = int[];

    internal class Program
    {
        static readonly Regex regRule = new Regex(@"(?<lower>[0-9]+)\|(?<upper>[0-9]+)");
        const int MaxPageNum = 100;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Problem #5 Here.\n");
            Console.WriteLine("Paste your input below, and hit Enter a couple times to input a blank line to trigger processing:\n");

            int MakeRule(int lower, int upper) => MaxPageNum * upper + lower;

            List<Rule> rules = new List<Rule>();
            //SetType ruleSet = new();
            SetType[] rulesDict = new SetType[MaxPageNum];
            for (string line; !string.IsNullOrWhiteSpace(line = Console.ReadLine());)
            { // reading rules
                var match = regRule.Match(line);
                if (match.Success
                    && int.TryParse(match.Groups["lower"].Value, out int l)
                    && int.TryParse(match.Groups["upper"].Value, out int u))
                {
                    rules.Add(new Rule(l, u));
                    int r = MakeRule(l, u);
                    //ruleSet.Add(r);

                    // init as we go
                    if (null == rulesDict[l])
                        rulesDict[l] = new();

                    rulesDict[l].Add(u);
                }
            }

            // somehow this program works without the below.
            // they give you enough explicit rules that finding implicit ones is unnecessary.

            //for (int iter = 0; iter < MaxPageNum-1; iter++) // gotta do this N-1 times
            //for (int left = 0; left < MaxPageNum; left++)
            //{
            //    if (rulesDict[left] == null)
            //        continue;

            //    SetType union = new(rulesDict[left]);
            //    foreach (int u in rulesDict[left])
            //        union.UnionWith(rulesDict[u]);
            //    rulesDict[left] = union;
            //}

            //int[] order = rulesDict.Select(set => set == null ? 0 : set.Count).ToArray();


            int sum = 0, sum2 = 0;
            for (string line; !string.IsNullOrWhiteSpace(line = Console.ReadLine());)
            { // reading sets
                ListType list = line.Split(',').Select(s => int.Parse(s)).ToArray();
                int middle = list[list.Length / 2];

                SetType set = new(list);

                // check rules
                bool CheckRules(ListType list)
                {
                    Dictionary<int, int> dict = new();
                    for (int i = 0; i < list.Length; i++)
                        dict.Add(list[i], i);

                    foreach (var rule in rules)
                    {
                        if (rule.Applies(set) &&
                            dict[rule.Lower] >= dict[rule.Upper])
                        { // broke the rule.
                            return false;
                        }
                    }
                    return true;
                }

                if (CheckRules(list)) 
                    sum += middle;
                else
                {
                    var ordered = list.OrderByDescending(page => set.Count(other => rulesDict[page].Contains(other))).ToArray();
                    int sortedMiddle = ordered[ordered.Length / 2];
                    sum2 += sortedMiddle;
                }

            }

            Console.WriteLine($"\nThe answer to Part 1 is {sum}!\n");
            Console.WriteLine($"\nThe answer to Part 2 is {sum2}!\n");
        }

        internal struct Rule(int lower, int upper)
        {
            public readonly int Lower = lower, Upper = upper;

            public bool Applies(SetType set) => set.Contains(Lower) && set.Contains(Upper);
        }
    }
}

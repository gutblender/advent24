using System.Drawing;

namespace day8_antinodes
{
    using MapType = List<char[]>;

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Problem #8 Here.\n");
            Console.WriteLine("Paste your input below, and hit Enter a couple times to input a blank line to trigger processing:\n");

            MapType map = new();
            Dictionary<char, List<Point>> antennae = new();
            int y = 0, width = 0;
            int cnt = 0;
            for (string? line; !string.IsNullOrWhiteSpace(line = Console.ReadLine()); y++)
            {
                map.Add(line!.Trim().ToCharArray());

                if (width < line.Length)
                    width = line.Length;

                for (int x = 0; x < line.Length; x++)
                { // hunt for antennae
                    char c = line[x];
                    if (c == '.') continue;

                    if (!antennae.TryGetValue(c, out List<Point> list) || list == null)
                    {
                        list = new();
                        antennae[c] = list;
                    }
                    list.Add(new (x, y));
                    cnt++;
                }
            }

            Rectangle bounds = new Rectangle(0, 0, width, y);

            uint MakePointHash(Point p) => (uint)p.X + ((uint)p.Y << 16);

            HashSet<uint> antinodes = new();
            HashSet<uint> anti2 = new();
            foreach (var kvp in antennae)
            {
                var points = kvp.Value;
                if (points.Count < 2) continue;

                for (int a1 = 0; a1 < points.Count - 1; a1++)
                {
                    for (int a2 = a1 + 1; a2 < points.Count; a2++)
                    {
                        int dx = points[a2].X - points[a1].X;
                        int dy = points[a2].Y - points[a1].Y;
                        Size sz = new(dx, dy);

                        Point p2 = points[a2];
                        Point p1 = points[a1];

                        foreach (var point in new[] { p1 - sz, p2 + sz })
                        {
                            if (bounds.Contains(point))
                                antinodes.Add(MakePointHash(point));
                        }

                        // add to point 2, sub from point 1
                        for (Point anti = p2; bounds.Contains(anti); anti += sz)
                            anti2.Add(MakePointHash(anti));
                        for (Point anti = p1; bounds.Contains(anti); anti -= sz)
                            anti2.Add(MakePointHash(anti));
                    }
                }
            }


            Console.WriteLine($"I found {antinodes.Count} antinodes!");
            Console.WriteLine($"I found {anti2.Count} resonant antinodes!");
        }
        internal readonly struct Pt(int x, int y)
        {
            public readonly int X = x, Y = y;
        }
    }
}

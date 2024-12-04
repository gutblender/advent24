using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;

namespace day4_xmasWordSearch
{
    internal class Program
    {
        static readonly string seq = @"XMAS";
        public enum Direction
        {
            Up, UpRt, Rt, DnRt, Dn, DnLf, Lf, UpLf, END,
            Start = 0
        }
        public static Direction Opposite(Direction direction)
        {
            return (Direction)(((int)direction + 4) % (int)Direction.END);
        }
        public static bool IsDiagonal(Direction dir)
        {
            return 0 != ((int)dir % 2);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Problem #4 Here.\n");
            Console.WriteLine("Paste your input below, and hit Enter a couple times to input a blank line to trigger processing:\n");

            List<string> map = new List<string>();
            int width = 0;
            for (string line; !string.IsNullOrWhiteSpace(line = Console.ReadLine());)
            {
                map.Add(line.Trim());
            }

            width = map.First().Length;
            int success = 0, success2 = 0;

            char MapGet(Pt pt) => map[pt.Y][pt.X];
            bool OkGo(Pt pt, Direction dir)
            {
                bool ok = true;
                switch (dir)
                {
                    case Direction.Up:
                        if (pt.Y <= 0) ok = false;
                        break;
                    case Direction.UpRt:
                        if (pt.Y <= 0) ok = false;
                        if (pt.X >= (width - 1)) ok = false;
                        break;
                    case Direction.Rt:
                        if (pt.X >= (width - 1)) ok = false;
                        break;
                    case Direction.DnRt:
                        if (pt.X >= (width - 1)) ok = false;
                        if (pt.Y >= (map.Count - 1)) ok = false;
                        break;
                    case Direction.Dn:
                        if (pt.Y >= (map.Count - 1)) ok = false;
                        break;
                    case Direction.DnLf:
                        if (pt.X <= 0) ok = false;
                        if (pt.Y >= (map.Count - 1)) ok = false;
                        break;
                    case Direction.Lf:
                        if (pt.X <= 0) ok = false;
                        break;
                    case Direction.UpLf:
                        if (pt.X <= 0) ok = false;
                        if (pt.Y <= 0) ok = false;
                        break;
                }
                return ok;
            }

            for (int xx = 0; xx < width; xx++)
            {
                for (int xy = 0; xy < map.Count; xy++)
                {
                    int crossingCount = 0;

                    for (Direction dir = Direction.Start; dir < Direction.END; dir++)
                    {
                        int sp = 0; // sequence process this direction
                        for (Pt pt = new Pt(xx, xy); sp < seq.Length && MapGet(pt) == seq[sp]; sp++)
                        {
                            if (!OkGo(pt, dir))
                                break;

                            pt = pt.Go(dir);
                        }

                        if (sp >= seq.Length)
                            success++;

                        // also look for success2
                        if (crossingCount < 2 
                            && IsDiagonal(dir)) // pain: Empirically, I found that they mean ONLY X-shapes. Like diagonals. Sigh.
                            do // just for easy breaks
                            {
                                Pt pt = new Pt(xx, xy);
                                if (MapGet(pt) != 'A') // crossing point for "MAS"
                                    break;

                                if (!OkGo(pt, dir))
                                    break;

                                Direction opposite = Opposite(dir);
                                if (!OkGo(pt, opposite))
                                    break;

                                if (MapGet(pt.Go(opposite)) == 'M' && MapGet(pt.Go(dir)) == 'S')
                                    crossingCount++;
                            } while (false);
                    }

                    if (crossingCount >= 2)
                        success2++;
                }
            }

            Console.WriteLine($"I found {success} \"{seq}\"es!\n");
            Console.WriteLine($"I found {success2} \"X-MAS\"es!\n");
        }


        internal struct Pt
        {
            public readonly int X, Y;
            public Pt(int x, int y) { X = x; Y = y; }

            public Pt Up() { return new Pt(X, Y - 1); }
            public Pt Dn() { return new Pt(X, Y + 1); }
            public Pt UpLf() { return new Pt(X - 1, Y - 1); }
            public Pt DnRt() { return new Pt(X + 1, Y + 1); }
            public Pt Lf() { return new Pt(X - 1, Y); }
            public Pt Rt() { return new Pt(X + 1, Y); }
            public Pt UpRt() { return new Pt(X + 1, Y - 1); }
            public Pt DnLf() { return new Pt(X - 1, Y + 1); }

            public Pt Go(Direction dir) => _go(dir, this);
            private static Pt _go(Direction dir, Pt pt)
            {
                switch (dir)
                {
                    case Direction.Up:
                        pt = pt.Up();
                        break;
                    case Direction.UpRt:
                        pt = pt.UpRt();
                        break;
                    case Direction.Rt:
                        pt = pt.Rt();
                        break;
                    case Direction.DnRt:
                        pt = pt.DnRt();
                        break;
                    case Direction.Dn:
                        pt = pt.Dn();
                        break;
                    case Direction.DnLf:
                        pt = pt.DnLf();
                        break;
                    case Direction.Lf:
                        pt = pt.Lf();
                        break;
                    case Direction.UpLf:
                        pt = pt.UpLf();
                        break;
                }
                return pt;
            }
        }
    }
}

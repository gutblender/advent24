namespace day6_guardPattern
{
    using MapType = List<char[]>;
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Problem #6 Here.\n");
            Console.WriteLine("Paste your input below, and hit Enter a couple times to input a blank line to trigger processing:\n");

            void MapCopy(ref MapType dest, MapType src)
            {
                if (dest == null)
                    dest = new();

                dest.Clear();
                foreach(var line in src)
                    dest.Add(line.ToArray());
            }

            MapType map = new(), orig = new(); // back up the map also
            int width = 0;
            Pt? start = null;
            for (string? line; !string.IsNullOrWhiteSpace(line = Console.ReadLine());)
            {
                int i = line!.IndexOf('^');
                if (0 <= i)
                    start = new Pt(i, orig.Count);

                orig.Add(line!.Trim().ToCharArray());
            }

            MapCopy(ref map, orig);
            width = map.First().Length;

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
            char GetDirChar(Direction dir)
            {
                switch (dir)
                {
                    case Direction.Up: return '^'; // this handles the corner case to end up where I started
                    case Direction.Rt: return 'R';
                    case Direction.Dn: return 'D';
                    case Direction.Lf: return 'L';
                    default: return dir.ToString()[0];
                }
            }

            uint CountTraversals(Pt start, Direction dir)
            {
                uint cnt = 1; // count the original square
                //Direction dir = Direction.Up;
                Pt pos = start; // !.Value;

                for (cnt = 1; ;)
                {
                    // turn in circles lol
                    int turns = 0;
                    Pt blockage;
                    for (Pt next
                        ; turns < 3 && OkGo(pos, dir) && '#' == MapGet(next = pos.Go(dir))
                        ; dir = TurnRight(dir), turns++)
                        blockage = next; // save where the blockage was

                    if (!OkGo(pos, dir))
                        break;

                    char dc = GetDirChar(dir);
                    pos = pos.Go(dir);
                    char now = MapGet(pos);

                    if (dc == now)
                        return 0xFFFFFFFF; // perfect circle

                    switch (now)
                    {
                        default:
                            if (now == '.')
                                cnt++; // only count places I ain't been before
                            map[pos.Y][pos.X] = dc;
                            break;

                        case '#':
                        case 'O':
                            throw new Exception();
                    }
                }

                return cnt;
            }

            uint cnt = CountTraversals(start!.Value, Direction.Up);
            Console.WriteLine($"The guard went to {cnt} unique positions!");

            // now start over. 
            {
                MapCopy(ref map, orig);

                Direction dir = Direction.Up;
                Pt pos = start!.Value;

                uint nb = 0;
                for (; ; )
                {
                    // turn in circles lol, but not really, <= 3 turns
                    int turns = 0;
                    Pt next = pos.Go(dir);
                    for (
                        ; turns < 3 && OkGo(pos, dir) && '#' == MapGet(next)
                        ; dir = TurnRight(dir), next = pos.Go(dir), turns++)
                        ; // nothing to do

                    if (!OkGo(pos, dir)) // going off the map
                        break; // done at last

                    char dc = GetDirChar(dir);
                    orig[pos.Y][pos.X] = dc; // mark my direction

                    if ('.' == orig[next.Y][next.X])
                    { 
                        // ONLY if the guard hasn't been here before. We can't put one down "behind" him.

                        MapCopy(ref map, orig); // clean it up for analysis
                        // now what if this were a blockage tho??
                        map[next.Y][next.X] = '#';
                        if (0xFFFFFFFF == CountTraversals(pos, dir))
                            nb++;
                    }

                    pos = next;
                }
                Console.WriteLine($"I could find {nb} unique positions for a blockage!");
            }

        }

        static Direction TurnRight(Direction dir)
        {
            return (Direction)(((int)dir + 2) % (int)Direction.END);
        }
    }

    
    public enum Direction
    {
        Up, UpRt, Rt, DnRt, Dn, DnLf, Lf, UpLf, END,
        Start = 0
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
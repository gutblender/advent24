namespace day25_keys;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World! Problem 25 here.\nPaste input, then something invalid at the end:");

        Span<uint> pieces = stackalloc uint[LockKeyBase.NumCols];
        List<Lock> locks = [];
        List<Key> keys = [];
        for (bool ok = true; ok;)
        {
            // read lines and get key / lock
            string? line = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                break;

            bool key = line == LockKeyBase.KeyBlank;
            pieces.Clear();

            for (uint linect = 1; linect <= LockKeyBase.NumRows && ok; linect++)
            {
                line = Console.ReadLine();
                if (line == null)
                {
                    ok = false;
                    break;
                }
                for (int c = 0; c < LockKeyBase.NumCols; c++)
                {
                    if (line[c] == '.') 
                        continue; // nothing
                    else if (line[c] != '#')
                    {
                        ok = false;
                        break;
                    }
                    uint height = key ? LockKeyBase.NumRows - linect : linect;
                    if (pieces[c] < height)
                        pieces[c] = height;
                }

                if (!ok) break;
            }

            var arr = pieces.ToArray();
            if (key)
                keys.Add(new(arr));
            else
                locks.Add(new(arr));

            Console.ReadLine(); // blank line
        }

        int fits = 0;
        foreach(var l in locks)
        {
            foreach(var k in keys)
            {
                if (l.Fits(k))
                    fits++;
            }
        }

        Console.WriteLine($"Number of fitting key-lock pairs: {fits}");
    }
}

abstract class LockKeyBase(uint[] pcs)
{
    public const int NumRows = 6;
    public const int NumCols = 5;
    public static readonly string KeyBlank = new ('.', NumCols);
    private readonly uint[] pieces = pcs;

    protected bool Matches(LockKeyBase other)
    {
        for (int c = 0; c < NumCols; c++)
        {
            if ((NumRows-1) != (this.pieces[c] + other.pieces[c]))
                return false;
        }
        return true;
    }
    protected bool Fits(LockKeyBase other)
    {
        for (int c = 0; c < NumCols; c++)
        {
            if ((NumRows-1) < (this.pieces[c] + other.pieces[c]))
                return false;
        }
        return true;
    }
}
class Lock(uint[] pcs) : LockKeyBase(pcs)
{
    public bool Matches(Key key) => base.Matches(key);
    public bool Fits(Key key) => base.Fits(key);
}
class Key(uint[] pcs) : LockKeyBase(pcs)
{
    public bool Matches(Lock key) => base.Matches(key);
    public bool Fits(Lock key) => base.Fits(key);
}
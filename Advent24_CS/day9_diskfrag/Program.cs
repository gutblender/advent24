using System.Reflection.Metadata.Ecma335;

namespace day9_diskfrag
{
    internal class Program
    {
        enum Mode { File, Blank }
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World! Problem #9 Here.\n");
            Console.WriteLine("Paste your input below, and hit Enter a couple times to input a blank line to trigger processing:\n");

            Stream inputStream = Console.OpenStandardInput(32768);
            Console.SetIn(new StreamReader(inputStream, Console.InputEncoding, false, 32768));
            if (Console.ReadLine() is string line)
                ; // good
            else return;

            // let's take it easy on the ol' heap shall we
            uint cap = 0;
            foreach (char c in line)
                cap += (uint)(c - '0');

            uint[] disk = new uint[cap];
            Mode mode = Mode.File;
            uint id = 0, pos = 0;
            uint nf = 0, nb = 0; // double-check.
            uint uniqueFiles = 0;
            foreach (char c in line)
            {
                uint num, extent = (uint)(c - '0');
                if (mode == Mode.File)
                {
                    num = uniqueFiles = ++id;
                    mode = Mode.Blank;
                    nf += extent;
                }
                else
                {
                    num = 0;
                    mode = Mode.File;
                    nb += extent;
                }
                for ( ; 0 < extent && pos < disk.Length; extent--, pos++)
                    disk[pos] = num;
            }


            Console.WriteLine($"Ok, got a disk map of length {line.Length}.");
            Console.WriteLine($"Operation complete for disk sized {cap} with {nf} file blocks and {nb} blank.");
            Console.WriteLine($"Unique files: {uniqueFiles}.");

            // disk2 copy
            uint[] disk2 = new uint[cap];
            Array.Copy(disk, disk2, cap);

            var crc = Solution1(disk);

            var contig = disk.TakeWhile(x => x != 0).Count();
            Console.WriteLine($"The checksum is {crc}.");
            Console.WriteLine($"From the bottom of the disk, {contig} blocks are packed contiguously.");


            var crc2 = Solution2(disk2);

            Console.WriteLine($"Disk2 crc = {crc2}");
        }

        static ulong Solution1(uint[] disk)
        {
            ulong crc = 0;
            uint leftwatermark = 0, highestFile = 0;
            uint left, right;
            for (left = 0, right = (uint)disk.Length - 1; left < right;)
            {
                // now start seeking blanks on the left, files on the right.
                for (; left < right && disk[left] != 0; left++)
                    crc += left * (disk[left] - 1); // add up CRC while you're add it.

                for (; left < right && disk[right] == 0; right--) ;

                for (; left < right && disk[left] == 0 && disk[right] != 0; left++, right--)
                {
                    disk[left] = disk[right];
                    disk[right] = 0;
                    crc += left * (disk[left] - 1); // add up CRC while you're add it.

                    if (highestFile < disk[left])
                        highestFile = disk[left];
                }

                if (leftwatermark < left)
                    leftwatermark = left;
            }
            for (; disk[left] != 0; crc += left * (disk[left] - 1), left++) ;

            Console.WriteLine($"Last File ID: {highestFile}, left off after {leftwatermark} blocks.");
            return crc;
        }

        static ulong Solution2(uint[] disk)
        { // code part-2 solution
            uint right; // init the right bound, find the highest file ID
            for (right = (uint)disk.Length - 1; right > 0 && disk[right] == 0; right--) ;

            if (right <= 0)
                return 0; // empty disk

            uint id = disk[right]; // this is the ID you must move next.
            for (uint left; id > 0; id--)
            {
                uint ll, rl; // left/right length

                // find the highest-ID file, already identified
                for (left = 0; left < right && disk[right] != id; right--) ;
                if (left >= right)
                    break; // no more files

                // get file length - count the whole thing
                for (rl = 1; rl < right && disk[right - rl] == id; rl++) ;

                // look for a space of at least that length:
                // while not long enough blank, pick up where we 'left' off hahaha
                uint end = right + 1 - 2 * rl;
                for (ll = 0; ll < rl && left <= end; left += ll)
                {
                    // find the next blank 
                    for (; left <= end && disk[left] != 0; left++) ;
                    if (left > end)
                        break; // no blank left of it

                    // count its length - but count it all, so left skips it all if it's not enough.
                    for (ll = 1; disk[left + ll] == 0; ll++) ;

                    if (ll >= rl)
                    { // found a good gap.
                        for (; rl > 0; rl--)
                        {
                            disk[left++] = id; // write
                            disk[right--] = 0; // erase
                        }
                        break;
                    }
                }
            }

            ulong crc2 = 0;
            for (uint i = 0; i < disk.Length; i++)
                if (disk[i] > 0)
                    crc2 += i * (disk[i] - 1);

            return crc2;
        }
    }
}

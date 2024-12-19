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
            string line = Console.ReadLine();

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

            ulong crc = 0;
            uint leftwatermark = 0, highestFile = 0;
            for (uint left = 0, right = cap-1; left < right; )
            {
                // now start seeking blanks on the left, files on the right.
                for (; left < cap && disk[left] != 0; left++)
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

            var contig = disk.TakeWhile(x => x != 0).Count();

            Console.WriteLine($"Ok, got a disk map of length {line.Length}.");
            Console.WriteLine($"Operation complete for disk sized {cap} with {nf} file blocks and {nb} blank.");
            Console.WriteLine($"Number of unique files: {uniqueFiles} and the last ID touched was {highestFile}.");
            Console.WriteLine($"Leaving off after {leftwatermark} file blocks, The checksum is { crc }.");
            Console.WriteLine($"From the bottom of the disk, {contig} blocks are packed contiguously.");
        }
    }
}

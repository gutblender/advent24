using System.Net.Quic;
using System.Text.RegularExpressions;

namespace day14_robots;

internal class Program
{
    const int NumX = 101, NumY = 103, NumSteps = 100;
    static readonly Regex regLine = new(@"p=(?<px>[0-9]+),(?<py>[0-9]+) v=(?<vx>-?[0-9]+),(?<vy>-?[0-9]+)");


    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World! Day 14 here.\nPaste Input:");

        //List<string> lines = [];
        List<Robot> robots = [];
        int[] quadrants = new int[4];
        for (string? line; !string.IsNullOrEmpty(line = Console.ReadLine()); )
        {
            var match = regLine.Match(line);
            if (!match.Success)
                break;

            int px = int.Parse(match.Groups["px"].Value);
            int py = int.Parse(match.Groups["py"].Value);
            int vx = int.Parse(match.Groups["vx"].Value);
            int vy = int.Parse(match.Groups["vy"].Value);

            // collect robots for part2
            robots.Add(new(px, py, vx, vy));

            // move
            px += NumSteps * vx;
            py += NumSteps * vy;

            if (px < 0) px += (1 - px / NumX) * NumX;
            if (py < 0) py += (1 - py / NumY) * NumY;

            px %= NumX; 
            py %= NumY;

            // quadrant
            if (0 != (NumX % 2) && px == (NumX / 2))
                ; // do nothing, ignore, you're in the vertical center
            else if (0 != (NumY % 2) && py == (NumY / 2))
                ; // do nothing, ignore, you're in the horizontal center
            else
            {
                int ixq = 0;
                if (px > (NumX / 2))
                    ixq |= 1;
                if (py > (NumY / 2))
                    ixq |= 2;
                quadrants[ixq]++;
            }
        }

        int product = 1;
        foreach (var quad in quadrants)
            product *= quad;
        Console.WriteLine($"Part 1 solution: {product}");

        char[,] grid = new char[NumY, NumX];
        const char BLANK = '.';
        void ClearGrid()
        {
            for (int x = 0; x < NumX; x++)
                for (int y = 0; y < NumY; y++)
                    grid[y, x] = BLANK;
        }
        void MarkGrid()
        {
            ClearGrid();
            foreach (var robot in robots)
                grid[robot.Py, robot.Px] = '%';
        }
        void PrintGrid(Stream str)
        {
            for (int y = 0; y < NumY; y++)
            {
                for (int x = 0; x < NumX; x++)
                    str.WriteByte((byte)grid[y, x]);
                str.WriteByte((byte)'\n');
            }
        }

        // console and file
        using var console = Console.OpenStandardOutput();


        //int bottomYStart = NumY - NumX / 2;
        for (int n = 0; ; n++)
        { // n seconds
            bool print = false;

            MarkGrid();
            // for for a big fat brick 15 high and 5 wide - see comment below
            const int height = 15, width = 5;
            for (int ys = 0; !print && ys < NumY - height; ys++)
                for (int xs = 0; !print && xs < NumX - width; xs++)
                {
                    print = true; // start optimistic, then faultfind.
                    for (int y = 0; print && y < height; y++)
                        for (int x = 0; print && x < width; x++)
                            if (grid[ys + y, xs + x] == BLANK)
                                print = false;
                }

            if (print)
            {
                PrintGrid(console);
                Console.WriteLine($"This was after {n} seconds. Press enter to continue...");
                Console.ReadLine();
            }

            foreach (var robot in robots)
            { // move
                robot.Px += robot.Vx + NumX;
                robot.Py += robot.Vy + NumY;

                robot.Px %= NumX;
                robot.Py %= NumY;
            }
        }
    }
}

class Robot(int px, int py, int vx, int vy)
{
    public int Px = px;
    public int Py = py;
    public readonly int Vx = vx;
    public readonly int Vy = vy;


}

/* Part 2 christmas tree looks like the below.
 * So I look for the "block" of non-dots amongst the robots '%'
 * For something like that block of hashes '#', emphasis my own
 * Just print it, with the time that took, and let the human decide.
 * This worked first try (well, after debugging I mean).
...................................%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%...................................
...................................%.............................%..........%........................
...................................%.............................%...................................
...................................%.............................%...................................
...................................%.............................%...................................
...................................%..............%..............%.....................%.............
...................................%.............%%%.............%...................................
....%..............................%............%%%%%............%...................................
...................................%...........%%%%%%%...........%...................................
..............................%....%..........%%%%%%%%%..........%...................................
...................................%............#####............%...................%...............
................%..................%...........%#####%...........%...................................
...................................%..........%%#####%%..........%...................................
....................%..............%.........%%%#####%%%.........%...................................
...................................%........%%%%#####%%%%........%...................................
..%..........%..........%..........%..........%%#####%%..........%......................%............
...................................%.........%%%#####%%%.........%...............%...................
...................................%........%%%%#####%%%%........%...................................
...................................%.......%%%%%#####%%%%%.......%..............%....................
...................................%......%%%%%%#####%%%%%%......%..................%.............%..
...............................%...%........%%%%#####%%%%........%..%................................
........................%..........%.......%%%%%#####%%%%%.......%......%............................
.................%.....%...........%......%%%%%%#####%%%%%%......%...................................
...................................%.....%%%%%%%#####%%%%%%%.....%......%............................
...............%...................%....%%%%%%%%#####%%%%%%%%....%..........%........................
.....................%.....%.......%.............%%%.............%.........................%.........
..........................%........%.............%%%.............%...................................
...................................%.............%%%.............%...................................
...................................%.............................%...................................
...................................%.............................%.....................%.............
.....%.............................%.............................%...................................
...................................%.............................%............%........%.............
...................................%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%...................................
*/

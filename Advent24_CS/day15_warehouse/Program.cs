using System.Drawing;
using System.Text;
using System.Diagnostics;

namespace day15_warehouse;

internal class Program
{
    const char BLANK = '.';
    const char BLOCK = '#';
    const char ROBOT = '@';
    const char BOX = 'O';
    const char LT = '<', RT = '>', UP = '^', DN = 'v';

    // part 2:
    const char BOXL = '[';
    const char BOXR = ']';
    static readonly char[] acBigBox = [BOXL, BOXR];

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World! Day 15 here.\nPaste Input: ");

        #region basic functions, generalized
        void MyAssert(bool b)
        {
            if (!b) throw new();
        }
        char MapGetCore(List<char[]> g, Point pt)
        {
            if (pt.Y >= 0 && pt.Y < g.Count
                && pt.X >= 0 && pt.X < g[pt.Y].Length)
                return g[pt.Y][pt.X];
            return BLOCK;
        }
        char MapSetCore(List<char[]> g, Point pt, char c) // returns the OLD value
        {
            char ret = BLOCK;
            if (pt.Y >= 0 && pt.Y < g.Count
                && pt.X >= 0 && pt.X < g[pt.Y].Length)
            {
                ret = g[pt.Y][pt.X];
                g[pt.Y][pt.X] = c;
            }
            return ret;
        }
        Point Move(Point pt, char move)
        {
            var moved = move switch
            {
                LT => new(pt.X - 1, pt.Y),
                RT => new(pt.X + 1, pt.Y),
                UP => new(pt.X, pt.Y - 1),
                DN => new(pt.X, pt.Y + 1),
                _ => pt
            };
            return moved;
        }
        Point BoxOther(Point me, char boxPiece) => boxPiece switch
        {
            BOXL => Move(me, RT),
            BOXR => Move(me, LT),
            _ => throw new()
        };

        bool IsHorz(char move) => move switch
        {
            LT or RT => true,
            _ => false
        };
        char Behind(char dir) => dir switch // works for boxes, or directions.
        {
            BOXL => BOXR,
            BOXR => BOXL,

            LT => RT,
            RT => LT,
            UP => DN,
            DN => UP,

            _ => throw new()
        };
        #endregion // basic functions, generalized

        #region problem input

        List<char[]> grid = [];
        List<char[]> grid2 = [];
        int bookends = 0;

        Point robot = new(), robot2 = new();

        string? line;
        for (StringBuilder sb = new(); !string.IsNullOrEmpty(line = Console.ReadLine());)
        {
            grid.Add(line.ToArray());
            int ix = line.IndexOf(ROBOT);

            // build part 2 map - everything's twice as  W I D E 
            foreach (char _c in line)
            {
                char c = ROBOT == _c ? BLANK : _c;

                if (BOX == c)
                    sb.Append(acBigBox);
                else
                    sb.Append(c, 2); // double everything up.
            }
            grid2.Add(sb.ToString().ToArray());
            sb.Clear();

            if (0 <= ix)
            { // write robot as blank so it's easier. "Invisible Robot!"
                robot = new() { X = ix, Y = grid.Count - 1 };
                grid[robot.Y][robot.X] = BLANK;

                // twice as  W I D E, robot on the left of its "two".
                robot2 = robot; robot2.X *= 2;
            }
            else if (!line.Any(c => c != BLOCK) && ++bookends >= 2)
                break; // found ends of map
        }

        //Console.WriteLine($"Part 1 Grid:");
        //PrintGrid(grid);
        //Console.WriteLine();
        //Console.WriteLine($"Part 2 Grid:");
        //PrintGrid(grid2);

        // skip blank lines
        while (string.IsNullOrEmpty(line = Console.ReadLine())) ;

        string instructions;
        { // assemble one instruction set
            StringBuilder sb = new();
            char[] whitespaceCharset = [' ', '\n', '\r', '\t'];
            do // do because line is now the first line of instructions
            {
                sb.Append(line.Trim(whitespaceCharset));
            } while (!string.IsNullOrEmpty(line = Console.ReadLine()));

            instructions = sb.ToString();
        }

        #endregion // problem input

        #region Part 2 solution functions, placed before Part 1 to ensure it can't call those fns

        char MapGet2(Point pt) => MapGetCore(grid2, pt);
        char MapSet2(Point pt, char c) => MapSetCore(grid2, pt, c);

        // recursive function: Given a move, pushing direction i, is it possible
        // when calling this, make the first move yourself.
        // not "the robot is here", it's "the robot wants to be here".
        bool CanMove(Point pt, char i)//, List<Point> affected)
        { // can the robot move here?
            char tile = MapGet2(pt);
            if (BLOCK == tile) return false;
            if (BLANK == tile) return true; // must be blank here, good to go!

            //if (IsBox(tile))
            { // oh, I must be a box. Can I move?
                var next = Move(pt, i);
                // check things in this direction straight ahead. For horizontal moves, that's all.
                bool ahead = CanMove(next, i); //, affected);
                if (IsHorz(i) || !ahead) // for verts, fail fast.
                    return ahead;

                // go in front of my buddy, can HE move?
                var buddyNext = Move(BoxOther(pt, tile), i);
                return ahead && CanMove(buddyNext, i);
            }
        }

        void Move2(Point pt, char i)
        { // move your own ass direction `i` from pt. Succeeds or throws.
            var next = Move(pt, i);

            char tile = MapGet2(pt);
            if (BLOCK == tile) throw new();
            if (BLANK == tile) return; // there's nothing here to move. You're all done.

            // oh, I must be a box. I better move things carefully.
            var buddy = BoxOther(pt, tile); // where's my other half?
            char buddyVal = Behind(tile); // what IS my other half?

            if (IsHorz(i))
            { // for horizontal moves:
                if ((LT == i) == (BOXL == tile)) // me first? leading the way
                {
                    Move2(next, i); // I have to tell the boxes `i` of me to move first.

                    // move me first then buddy behind me.
                    MyAssert(BLANK == MapSet2(next, tile)); // move me
                    MyAssert(tile == MapSet2(pt, buddyVal)); // move buddy to where I was.

                    // clean up behind buddy.
                    var ptBehind = Move(pt, Behind(i));
                    MyAssert(buddyVal == MapSet2(ptBehind, BLANK));
                }
                else
                { // move stuff on the other side of my box.
                    var ptOutside = Move(next, i);
                    Move2(ptOutside, i);

                    // move buddy, then me.
                    MyAssert(BLANK == MapSet2(ptOutside, buddyVal)); // move the buddy into the now empty space
                    MyAssert(buddyVal == MapSet2(next, tile)); // move me where buddy was.

                    // clean up behind me.
                    MyAssert(tile == MapSet2(pt, BLANK));
                }
            }
            else
            { // for vertical moves it's harder to test but simpler to move. Just move.
                Move2(next, i); // tell whoever's there to move.

                // clear the way for my buddy too.
                var buddyNext = Move(buddy, i);
                Move2(buddyNext, i);

                // move & cleanup, in no particular order.
                MyAssert(BLANK == MapSet2(buddyNext, buddyVal)); // move the buddy
                MyAssert(BLANK == MapSet2(next, tile)); // move me

                // clean up behind
                MyAssert(buddyVal == MapSet2(buddy, BLANK)); // move the buddy
                MyAssert(tile == MapSet2(pt, BLANK)); // move me
            }
        }
        #endregion // Part 2 solution functions


        char MapGet(Point pt) => MapGetCore(grid, pt);
        char MapSet(Point pt, char c) => MapSetCore(grid, pt, c);

        foreach (char i in instructions)
        {

            { // PART 1 SOLUTION
                var next = Move(robot, i);
                // push objects? Keep going in that direction until blockage or free.
                Point pt = next;
                char tile;
                while (BOX == (tile = MapGet(pt)))
                    pt = Move(pt, i);

                if (tile == BLANK) // there is space in this move
                {  // success! move the robot.
                    MapSet(robot, BLANK);
                    robot = next;
                    // are we moving boxes as a result?
                    if (BOX == MapSet(next, BLANK))
                        MapSet(pt, BOX);
                    MapSet(robot, ROBOT);
                }
                // "else" no move possible, don't do anything.
            }

            { // PART2 SOLUTION
                var next = Move(robot2, i);
                if (CanMove(next, i))
                {
                    MapSet2(robot2, BLANK);
                    Move2(next, i); // this moves stuff out the way.
                    MyAssert(BLANK == MapGet2(next)); // sanity-check that.
                    robot2 = next; // yay
                    MapSet2(robot2, ROBOT);
                }
                else MyAssert(BLANK != MapGet2(next)); // sanity check: better not be blank then.
            }
        }

        { // part 2 sum
            ulong sum = 0;
            for (int y = 0; y < grid.Count; y++)
                for (int x = 0; x < grid[y].Length; x++)
                    if (grid[y][x] == BOX)
                        sum += (ulong)(100 * y + x);

            Console.WriteLine($"Part 1 solution: {sum}");
        }

        { // part 2 sum
            ulong sum = 0;
            for (int y = 0; y < grid2.Count; y++)
                for (int x = 0; x < grid2[y].Length; x++)
                    if (grid2[y][x] == BOXL)
                        // find the minimum x distance - y unaffected.
                        // if x+1 == xLength-1, distance is zero, so distance = xLength-1 - (x+1)
                        sum += (ulong)(100 * y + x); // Math.Min(x, grid2[y].Length - 2 - x));

            Console.WriteLine($"Part 2 solution: {sum}");
        }

        void PrintGrid(List<char[]> g)
        {
            foreach (var arr in g)
                Console.WriteLine(arr);
        }

        Console.WriteLine($"Part 1 Grid:");
        PrintGrid(grid);
        Console.WriteLine();
        Console.WriteLine($"Part 2 Grid:");
        PrintGrid(grid2);
    }
}

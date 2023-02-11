using System.Text;

namespace Virgee.Sudoku;

/// <summary> 
/// A representation of a Sudoku board.
/// </summary>
public class Board : System.IEquatable<Board>
{
    public int[] Cells { get; private set; }
    public int[] GeneratedCells { get; private set; }
    public int[] CompletedCells { get; private set; } = default!;
    private int AllowedHints = 3;
    private int NextCellIndex = 0;
    private int CursorRow { get; set; }
    private int CursorColumn { get; set; }

    private int HistoryIndex = 0;
    private List<(int, int)> History { get; set; } = default!;
    private static readonly int[] Numbers = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    private static readonly Random rnd = new Random(DateTime.Now.Millisecond);
    private bool HighlightDuplicates { get; set; } = true;

    public Board() : this(new int[81])
    {
        // This space intentionally left blank.
    }

    public Board(int[] cells)
    {
        if (cells.Length != 81)
        {
            throw new System.ArgumentException("The number of cells does not match the dimensions of a Sudoku board");
        }

        Cells = (int[])cells.Clone();
        GeneratedCells = (int[])cells.Clone();
    }

    public void Reset()
    {
        Cells = (int[])GeneratedCells.Clone();
        History = new List<(int, int)>();
    }

    public void Generate()
    {
        Reset();
        Solve();
        CompletedCells = (int[])Cells.Clone();
        if (IsValid())
        {
            while (GetEmptyCellCount() < 45)
            {
                int index = rnd.Next(Cells.Length);
                if (Cells[index] != 0)
                {
                    Cells[index] = 0;
                }
            }
        }

        GeneratedCells = (int[])Cells.Clone();
    }

    public bool Solve()
    {
        NextCellIndex = 0;
        return SolveInternal();
    }

    private bool SolveInternal()
    {
        if (NextCellIndex >= Cells.Length)
        {
            return true;
        }

        var row = NextCellIndex / 9;
        var column = NextCellIndex % 9;
        var numbers = GetNumbers();

        foreach (var i in numbers)
        {
            if (!CanPutNumberInCell(row, column, i))
            {
                continue;
            }

            Cells[NextCellIndex++] = i;

            if (SolveInternal())
            {
                return true;
            }

            Cells[--NextCellIndex] = 0;
        }

        return false;
    }

    public bool IsValid()
    {
        var visits = new HashSet<string>();
        for (var i = 0; i < 9; i++)
        {
            for (var j = 0; j < 9; j++)
            {
                var value = Cells[i * 9 + j];
                if (value == 0)
                    continue;

                if (
                    !visits.Add($"{value}r({i})") ||
                    !visits.Add($"{value}c({j})") ||
                    !visits.Add($"{value}b({i / 3},{j / 3})")
                    )
                {
                    return false;
                }

            }
        }

        return true;
    }

    public void GoBack()
    {
        if (HistoryIndex - 2 < 0) return;

        HistoryIndex -= 2;
        var (index, number) = History[HistoryIndex];
        Cells[index] = number;
        CursorRow = index / 9;
        CursorColumn = index % 9;
    }

    public void GoForward()
    {
        if (HistoryIndex >= History.Count) return;

        var (index, number) = History[HistoryIndex + 1];
        Cells[index] = number;
        CursorRow = index / 9;
        CursorColumn = index % 9;

        HistoryIndex += 2;
    }

    public void PlaceNextNumber()
    {
        if (AllowedHints <= 0)
        {
            return;
        }

        AllowedHints--;
        var emptyCellIndexes = Cells.Select((v, i) => new { v, i })
            .Where(x => x.v == 0)
            .ToArray();

        if (emptyCellIndexes.Any())
        {

            var index = emptyCellIndexes[rnd.Next(emptyCellIndexes.Length)].i;

            Cells[index] = CompletedCells[index];
        }
    }

    public void HighlightErrors()
    {
        HighlightDuplicates = !HighlightDuplicates;
    }

    private void UpdateHistory((int, int) tuple)
    {
        if (History.Count > 0 && HistoryIndex < History.Count)
        {
            History.RemoveRange(HistoryIndex, History.Count - HistoryIndex);
        }

        History.Add(tuple);
        HistoryIndex = History.Count;
    }

    public void SetCell(int number)
    {

        int index = CursorRow * 9 + CursorColumn;
        if (GeneratedCells[index] == 0)
        {
            UpdateHistory((index, Cells[index]));
            Cells[index] = number;
            UpdateHistory((index, Cells[index]));
        }
    }

    public void PrintBoard()
    {
        Console.WriteLine(this);
    }

    public void PrintRemainingNumbers()
    {
        Console.SetCursorPosition(0, 19);
        for (var n = 1; n <= 9; n++)
        {
            Console.Write(GetRemainingNumberCount(n) < 9 ? $"  \x1b[0;37m{n}\x1b[0m " : $"  \x1b[2;37m{n}\x1b[0m ");
        }
    }

    public void PrintLegend()
    {
        int left = 37;
        var legend = new List<string>(new string[] {
            "\t \x1b[1;37mR\x1b[0m\t\t\tReset the game",
            "\t \x1b[1;37m0\x1b[0m\t\t\tReset the cell",
            "\t \x1b[1;37m1-9\x1b[0m\t\t\tPlace a number under cursor",
            "\t \x1b[1;37mW\x1b[0m or \x1b[1;37mUp Arrow\x1b[0m\t\tMove the cursor up",
            "\t \x1b[1;37mA\x1b[0m or \x1b[1;37mLeft Arrow\x1b[0m\tMove the cursor left",
            "\t \x1b[1;37mS\x1b[0m or \x1b[1;37mDown Arrow\x1b[0m\tMove the cursor down",
            "\t \x1b[1;37mW\x1b[0m or \x1b[1;37mRight Arrow\x1b[0m\tMove the cursor right",
            $"\t \x1b[{(HistoryIndex - 1 > 0 ? 1 : 2)};37m,\x1b[0m\t\t\t\x1b[{(HistoryIndex - 1 > 0 ? 0 : 2)};37mGo back in history\x1b[0m",
            $"\t \x1b[{(HistoryIndex < History.Count ? 1 : 2)};37m.\x1b[0m\t\t\t\x1b[{(HistoryIndex < History.Count ? 0 : 2)};37mGo forward in history\x1b[0m",
            $"\t \x1b[1;37mE\x1b[0m\t\t\tHighlight erros ({(!HighlightDuplicates ? "off" : "on")})  ",
            $"\t \x1b[{(AllowedHints > 0 ? 1 : 2)};37mH\x1b[{(AllowedHints > 0 ? 0 : 2)}m\t\t\tHint ({AllowedHints} remaining)\x1b[0m",
            "\t \x1b[1;37mN\x1b[0m\t\t\tNext game",
            "\t \x1b[1;37mQ\x1b[0m or \x1b[1;37mESC\x1b[0m\t\tExit",
            "",
            $"\t {(GetEmptyCellCount() == 0 && IsValid() ? "YOU FOUND THE SOLUTION!!!!" : new String(' ', 26))}"
        });

        for (var top = 0; top < legend.Count; top++)
        {
            Console.SetCursorPosition(left, top);
            Console.WriteLine(legend[top]);
        }
    }

    public void PrintCursor()
    {
        var left = CursorColumn * 4 + 2;
        var top = (CursorRow * 2) + 1;


        Console.SetCursorPosition(left - 2, top - 1);
        Console.Write("\x1b[1;33m╔═══╗");
        Console.SetCursorPosition(left - 2, top);
        Console.Write("║");
        Console.SetCursorPosition(left + 2, top);
        Console.Write("║");
        Console.SetCursorPosition(left - 2, top + 1);
        Console.Write("╚═══╝\x1b[0m");
    }

    public void MoveCursorUp()
    {
        CursorRow--;
        if (CursorRow < 0) CursorRow = 8;
    }

    public void MoveCursorDown()
    {
        CursorRow++;
        if (CursorRow > 8) CursorRow = 0;
    }

    public void MoveCursorLeft()
    {
        CursorColumn--;
        if (CursorColumn < 0) CursorColumn = 8;
    }

    public void MoveCursorRight()
    {
        CursorColumn++;
        if (CursorColumn > 8) CursorColumn = 0;
    }

    public bool Equals(Board? other)
    {
        if (other is null) return false;
        return Cells.SequenceEqual(other.Cells);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.AppendLine("\x1b[2;37m╔═══╤═══╤═══╦═══╤═══╤═══╦═══╤═══╤═══╗");


        for (int i = 0; i < Cells.Length; i++)
        {
            var row = i / 9;
            var column = i % 9;

            if (row > 0 && column == 0)
            {
                sb.AppendLine(row % 3 == 0 ? "\n╠═══╪═══╪═══╬═══╪═══╪═══╬═══╪═══╪═══╣" : "\n╟───┼───┼───╫───┼───┼───╫───┼───┼───╢");
            }

            if (column == 0)
            {
                sb.Append("║");
            }

            var divider = ((column + 1) % 3) == 0 ? "║" : "│";
            var number = Cells[i];
            var color = 32;

            if (HighlightDuplicates && IsDuplicate(row, column, number))
            {
                color = 31;
            }
            else
            {
                if (Cells[i] != 0 && Cells[i] == Cells[CursorRow * 9 + CursorColumn])
                {
                    color = 35;
                }
            }


            sb.Append(Cells[i] == 0 ? $"   {divider}" : $" \x1b[0;{color}m{number}\x1b[2;37m {divider}");
        }


        sb.AppendLine("\n╚═══╧═══╧═══╩═══╧═══╧═══╩═══╧═══╧═══╝\x1b[0m");

        return sb.ToString();
    }

    private bool IsDuplicate(int row, int column, int number)
    {
        return
            IsDuplicateInRow(row, number) ||
            IsDuplicateInColumn(column, number) ||
            IsDuplicateInBlock(row, column, number);
    }

    private bool IsDuplicateInRow(int row, int number)
    {
        int count = 0;
        for (var column = 0; column < 9; column++)
        {
            if (Cells[row * 9 + column] == number)
            {
                count++;
            }
        }

        return count > 1;
    }

    private bool IsDuplicateInColumn(int column, int number)
    {
        int count = 0;
        for (var row = 0; row < 9; row++)
        {
            if (Cells[row * 9 + column] == number)
            {
                count++;
            }
        }

        return count > 1;
    }

    private bool IsDuplicateInBlock(int row, int column, int number)
    {
        int count = 0;
        var blockRow = row / 3;
        var blockColumn = column / 3;
        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                if (Cells[(blockRow * 3 + i) * 9 + (blockColumn * 3 + j)] == number)
                {
                    count++;
                }
            }
        }

        return count > 1;
    }



    private int GetEmptyCellCount()
    {
        return Cells.Count(c => c == 0);
    }

    private int GetRemainingNumberCount(int number)
    {
        return Cells.Count(c => c == number);
    }

    private static IEnumerable<int> GetNumbers()
    {
        return Numbers.OrderBy(x => rnd.Next(1, 1000));
    }

    private bool CanPutNumberInCell(int drow, int dcolumn, int dnumber)
    {
        var startingBoxIndex = (drow / 3 * 3) * 9 + (dcolumn / 3 * 3);

        for (var i = 0; i < 9; i++)
        {
            var boxRow = i / 3;
            var boxColumn = i % 3;
            var checkRowIndex = drow * 9 + i;
            var checkColumnIndex = i * 9 + dcolumn;
            var checkBoxIndex = startingBoxIndex + boxRow * 9 + boxColumn;

            if ((checkRowIndex <= NextCellIndex && Cells[checkRowIndex] == dnumber) ||
                (checkColumnIndex <= NextCellIndex && Cells[checkColumnIndex] == dnumber) ||
                (checkBoxIndex <= NextCellIndex && Cells[checkBoxIndex] == dnumber))
            {
                return false;
            }
        }

        return true;
    }

}


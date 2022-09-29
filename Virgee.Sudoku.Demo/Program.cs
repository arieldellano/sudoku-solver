using Virgee.Sudoku;

Console.Clear();
Console.CursorVisible = false;

var quit = false;
var board = new Board();
board.Generate();

do
{
    Console.SetCursorPosition(0, 0);

    board.PrintBoard();
    board.PrintLegend();
    board.PrintRemainingNumbers();
    board.PrintCursor();

    var keyInfo = Console.ReadKey(true);

    switch (keyInfo.Key)
    {
        case ConsoleKey.Q:
        case ConsoleKey.Escape:
            quit = true;
            break;
        case ConsoleKey.W:
        case ConsoleKey.UpArrow:
            board.MoveCursorUp();
            break;
        case ConsoleKey.S:
        case ConsoleKey.DownArrow:
            board.MoveCursorDown();
            break;
        case ConsoleKey.A:
        case ConsoleKey.LeftArrow:
            board.MoveCursorLeft();
            break;
        case ConsoleKey.D:
        case ConsoleKey.RightArrow:
            board.MoveCursorRight();
            break;
        case ConsoleKey.R:
            board.Reset();
            break;
        case ConsoleKey.E:
            board.HighlightErrors();
            break;
        case ConsoleKey.N:
            board = new Board();
            board.Generate();
            break;

        case ConsoleKey.H:
            board.PlaceNextNumber();
            break;
        default:
            if (keyInfo.KeyChar == ',') {
                board.GoBack();
            } else if (keyInfo.KeyChar == '.')  {
                board.GoForward();
            } else if ((keyInfo.KeyChar >= '0' && keyInfo.KeyChar <= '9'))
            {
                var value = (int)keyInfo.KeyChar - (int)'0';
                board.SetCell(value);
            }
            break;

    }
} while (!quit);

Console.Clear();
Console.CursorVisible = true;
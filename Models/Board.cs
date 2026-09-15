using System.Diagnostics;

namespace SudokuCLI;

public class Board
{
    // CONSTANTS ==========
    public const int DisplayWidth = 37;
    public const int DisplayHeight = 19;


    // PROPERTIES ==========
    public Cell[,] Cells { get; set; }
    public int CursorRow { get; set; }
    public int CursorColumn { get; set; }
    public Game GameInstance { get; set; }

    // CONSTRUCTOR ==========
    public Board(int[,] solution, int[,] puzzle, Game instance)
    {
        GameInstance = instance;
        Cells = new Cell[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                Cells[row, column] = new Cell(solution[row, column], puzzle[row, column] != 0);
            }
        }
    }


    // METHODS ==========
    public bool ValidateWin()
    {
        List<Cell> correctCells = new List<Cell>();
        int givenCount = 0;
        foreach (Cell cell in Cells)
        {
            if (cell.Value == cell.Guess)
                correctCells.Add(cell);

            if (cell.IsGiven)
                givenCount++;
        }

        if (correctCells.Count == Cells.Length - givenCount)
            return true;

        return false;
    }

    public void SetGuess(int value)
    {
        if (Cells[CursorRow, CursorColumn].IsGiven)
            return;

        Cells[CursorRow, CursorColumn].IsGuess = true;
        Cells[CursorRow, CursorColumn].Guess = value;
    }

    public void ClearGuess()
    {
        if (Cells[CursorRow, CursorColumn].IsGiven)
            return;

        Cells[CursorRow, CursorColumn].IsGuess = false;
        Cells[CursorRow, CursorColumn].Guess = 0;
    }

    public void MoveCursor(int rowOffset, int columnOffset)
    {
        int row = CursorRow + rowOffset;
        int column = CursorColumn + columnOffset;

        if (row >= 0 && row <= 8) CursorRow = row;
        if (column >= 0 && column <= 8) CursorColumn = column;
    }

    public void DrawHeader()
    {
        TimeSpan elapsed = GameInstance.Stopwatch.Elapsed;

        string left = $"DIFFICULTY: {GameInstance.Difficulty}";
        string right = $"TIME: {(int)elapsed.TotalMinutes:00}:{elapsed.Seconds:00}";

        int gap = Math.Max(1, DisplayWidth - left.Length - right.Length);
        string header = (left + new string(' ', gap) + right).PadRight(DisplayWidth);

        lock (Terminal.DrawLock)
        {
            Console.SetCursorPosition(OriginX(), OriginY());
            Console.Write(header);
        }
    }

    public void Display()
    {
        int originX = OriginX();
        int y = OriginY();

        DrawHeader();
        y++;

        lock (Terminal.DrawLock)
        {
            Console.SetCursorPosition(originX, y++);
            Console.Write("╔═══╤═══╤═══╦═══╤═══╤═══╦═══╤═══╤═══╗");
            for (int i = 0; i < 9; i++)
            {
                if (i > 0)
                {
                    Console.SetCursorPosition(originX, y++);
                    if (i % 3 == 0)
                        Console.Write("╠═══╪═══╪═══╬═══╪═══╪═══╬═══╪═══╪═══╣");
                    else
                        Console.Write("╟───┼───┼───╫───┼───┼───╫───┼───┼───╢");
                }

                Console.SetCursorPosition(originX, y++);

                for (int j = 0; j < 9; j++)
                {
                    if (j % 3 == 0)
                        Console.Write("║");
                    else
                        Console.Write("│");

                    if (Cells[i, j].IsGiven)
                        Console.Write($" {Cells[i, j].ToString()} ");
                    else if (Cells[i, j].IsGuess)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write($" {Cells[i, j].ToString()} ");
                        Console.ResetColor();
                    }
                    else
                        Console.Write("   ");
                }

                Console.Write("║");
            }

            Console.SetCursorPosition(originX, y);
            Console.WriteLine("╚═══╧═══╧═══╩═══╧═══╧═══╩═══╧═══╧═══╝");
            Console.SetCursorPosition(originX, y + 1);
            Console.WriteLine();
            Console.SetCursorPosition(originX, y + 2);
            Console.WriteLine("▲ UP     ▼ DOWN    [1 TO 9] SET VALUE");
            Console.SetCursorPosition(originX, y + 3);
            Console.WriteLine("◀ LEFT   ▶ RIGHT   [DEL] DELETE VALUE");
            //TODO: quando cursor estiver em "ⓘ", printar instruções abaixo
        }
    }

    public void DrawCursorOverlay()
    {
        int x = OriginX() + CursorColumn * 4;
        int y = OriginY() + 1 + CursorRow * 2;

        lock (Terminal.DrawLock)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.SetCursorPosition(x, y);
            Console.Write("╔═══╗");

            Console.SetCursorPosition(x, y + 1);
            Console.Write("║");

            Console.SetCursorPosition(x + 4, y + 1);
            Console.Write("║");

            Console.SetCursorPosition(x, y + 2);
            Console.Write("╚═══╝");

            Console.ResetColor();
        }
    }

    public static int OriginX()
    {
        return Math.Max(0, (Console.WindowWidth - DisplayWidth) / 2);
    }

    public static int OriginY()
    {
        return Math.Max(0, (Console.WindowHeight - DisplayHeight) / 2);
    }
}

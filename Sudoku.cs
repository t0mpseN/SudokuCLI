using System.Diagnostics;

namespace SudokuCLI;

public class Sudoku
{
    // PROPERTIES ==========
    private static readonly Random Rng = new Random();
    public Cell[,] Board { get; set; }
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;
    public int CursorRow { get; set; }
    public int CursorColumn { get; set; }


    // CONSTRUCTOR ==========
    public Sudoku()
    {
        Difficulty = PickDifficulty();

        Console.Clear();
        Console.CursorVisible = false;

        int[,] solution = new int[9, 9];
        Solve(solution);

        int[,] puzzle = (int[,])solution.Clone();
        Carve(puzzle, Difficulty);

        int givens = 0;
        foreach (int v in puzzle) if (v != 0) givens++;
        Console.WriteLine($"{Difficulty}: {givens} pistas, técnica {LogicSolve(puzzle)}");
        Console.ReadKey(true);

        Board = new Cell[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                Board[row, column] = new Cell(solution[row, column], puzzle[row, column] != 0);
            }
        }


        Stopwatch stopwatch = Stopwatch.StartNew();
        bool haveWon = false;
        while (!haveWon)
        {
            Console.SetCursorPosition(0,0);
            DisplayBoard(Board);
            DrawCursorOverlay();
            GetPlayerInput();
            haveWon = ValidateWin();
        }

        double timeElapsed = stopwatch.Elapsed.TotalMinutes;
        if (DisplayEndScreen(timeElapsed))
            new Sudoku();
        else
            Environment.Exit(0);
    }



    // METHODS ==========
    private bool ValidateWin()
    {
        List<Cell> correctCells = new List<Cell>();
        int givenCount = 0;
        foreach (Cell cell in Board)
        {
            if (cell.Value == cell.Guess)
                correctCells.Add(cell);

            if (cell.IsGiven)
                givenCount++;
        }

        if (correctCells.Count == Board.Length - givenCount)
            return true;

        return false;
    }

    private void GetPlayerInput()
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        if (keyInfo.KeyChar >= '1' && keyInfo.KeyChar <= '9')
        {
            if (!Board[CursorRow, CursorColumn].IsGiven)
            {
                Board[CursorRow, CursorColumn].IsGuess = true;
                Board[CursorRow, CursorColumn].Guess = keyInfo.KeyChar - '0';
            }
        }
        else if (keyInfo.KeyChar == '0' || keyInfo.Key == ConsoleKey.Backspace || keyInfo.Key == ConsoleKey.Delete)
        {
            if (!Board[CursorRow, CursorColumn].IsGiven)
            {
                Board[CursorRow, CursorColumn].IsGuess = false;
                Board[CursorRow, CursorColumn].Guess = 0;
            }
        }
        else
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    if (CursorRow > 0) CursorRow--;
                    break;
                case ConsoleKey.DownArrow:
                    if (CursorRow < 8) CursorRow++;
                    break;
                case ConsoleKey.LeftArrow:
                    if (CursorColumn > 0) CursorColumn--;
                    break;
                case ConsoleKey.RightArrow:
                    if (CursorColumn < 8) CursorColumn++;
                    break;
                case ConsoleKey.Escape:
                    return;
            }
        }
    }

    private Difficulty PickDifficulty()
    {
        Console.Clear();
        Console.Write("Pick difficulty:\n1. Easy\n2. Medium\n3. Hard\n4. Expert\n");
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);

        switch (keyInfo.KeyChar)
        {
            case '1':
                return Difficulty.Easy;
            case '2':
                return Difficulty.Medium;
            case '3':
                return Difficulty.Hard;
            case '4':
                return Difficulty.Expert;
            default:
                return PickDifficulty();
        }
    }

    private bool DisplayEndScreen(double timeElapsed)
    {
        Console.Clear();
        Console.WriteLine("CONGRATULATIONS!");
        Console.WriteLine($"You have solved this board in {timeElapsed:F2}");
        Console.WriteLine("Play again?\n1. Yes\n2.");

        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        switch (keyInfo.KeyChar)
        {
            case '1':
                return true;
            case '2':
                return false;
            default:
                return DisplayEndScreen(timeElapsed);
        }
    }

    private bool Solve(int[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                if (board[row, column] == 0)
                {
                    List<int> values = Enumerable.Range(1, 9).OrderBy(x => Rng.Next()).ToList();
                    foreach (int value in values)
                    {
                        if (IsValid(board, row, column, value))
                        {
                            board[row, column] = value;
                            if (Solve(board))
                                return true;

                            board[row, column] = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsValid(int[,] board, int row, int column, int value)
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[row, i] == value ||
                board[i, column] == value ||
                board[row / 3 * 3 + i / 3, column / 3 * 3 + i % 3] == value)
                return false;
        }
        return true;
    }

    private int CountSolutions(int[,] board, int limit = 2)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                if (board[row, column] == 0)
                {
                    int found = 0;
                    for (int value = 1; value <= 9; value++)
                    {
                        if (IsValid(board, row, column, value))
                        {
                            board[row, column] = value;
                            found += CountSolutions(board, limit);
                            board[row, column] = 0;
                            if (found >= limit)
                                return found;
                        }
                    }
                    return found;
                }
            }
        }
        return 1;
    }

    private Technique MaxTechnique(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.Easy: return Technique.NakedSingle;
            case Difficulty.Medium: return Technique.HiddenSingle;
            default: return Technique.Guess;
        }
    }

    private void Carve(int[,] puzzle, Difficulty difficulty)
    {
        Technique cap = MaxTechnique(difficulty);

        List<(int, int)> cells = new List<(int, int)>();
        for (int row = 0; row < 9; row++)
            for (int column = 0; column < 9; column++)
                cells.Add((row, column));

        cells = cells.OrderBy(x => Rng.Next()).ToList();

        foreach ((int row, int column) in cells)
        {
            int backup = puzzle[row, column];
            puzzle[row, column] = 0;

            bool accept;
            if (cap == Technique.Guess)
                accept = CountSolutions(puzzle) == 1;
            else
                accept = LogicSolve(puzzle) <= cap;

            if (!accept)
                puzzle[row, column] = backup;
        }
    }

    private List<int> GetCandidates(int[,] board, int row, int column)
    {
        List<int> candidates = new List<int>();
        for (int value = 1; value <= 9; value++)
        {
            if (IsValid(board, row, column, value))
                candidates.Add(value);
        }
        return candidates;
    }

    private bool NakedSingle(int[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                if (board[row, column] != 0) continue;

                List<int> candidates = GetCandidates(board, row, column);
                if (candidates.Count == 1)
                {
                    board[row, column] = candidates[0];
                    return true;
                }
            }
        }
        return false;
    }

    private (int row, int column) CellOfUnit(int unit, int index)
    {
        if (unit < 9) return (unit, index);      
        if (unit < 18) return (index, unit - 9);  

        int block = unit - 18;                   
        return (block / 3 * 3 + index / 3, block % 3 * 3 + index % 3);
    }

    private bool HiddenSingle(int[,] board)
    {
        for (int unit = 0; unit < 27; unit++)
        {
            for (int value = 1; value <= 9; value++)
            {
                int count = 0;
                int foundRow = -1, foundColumn = -1;

                for (int i = 0; i < 9; i++)
                {
                    (int row, int column) = CellOfUnit(unit, i);
                    if (board[row, column] != 0) continue;
                    if (!IsValid(board, row, column, value)) continue;

                    count++;
                    foundRow = row;
                    foundColumn = column;
                }

                if (count == 1)
                {
                    board[foundRow, foundColumn] = value;
                    return true;
                }
            }
        }
        return false;
    }

    private Technique LogicSolve(int[,] board)
    {
        int[,] working = (int[,])board.Clone();
        Technique hardest = Technique.None;

        while (true)
        {
            bool complete = true;
            foreach (int value in working)
                if (value == 0) complete = false;

            if (complete) return hardest;

            if (NakedSingle(working))
            {
                if (hardest < Technique.NakedSingle) hardest = Technique.NakedSingle;
            }
            else if (HiddenSingle(working))
            {
                if (hardest < Technique.HiddenSingle) hardest = Technique.HiddenSingle;
            }
            else
            {
                return Technique.Guess;
            }
        }
    }

    private void DisplayBoard(Cell[,] board)
    {
        Console.WriteLine("╔═══╤═══╤═══╦═══╤═══╤═══╦═══╤═══╤═══╗");
        for (int i = 0; i < 9; i++)
        {
            if (i > 0)
            {
                if (i % 3 == 0)
                    Console.WriteLine("╠═══╪═══╪═══╬═══╪═══╪═══╬═══╪═══╪═══╣");
                else
                    Console.WriteLine("╟───┼───┼───╫───┼───┼───╫───┼───┼───╢");
            }

            for (int j = 0; j < 9; j++)
            {
                if (j % 3 == 0)
                    Console.Write("║");
                else
                    Console.Write("│");

                if (board[i, j].IsGiven)
                    Console.Write($" {board[i, j].ToString()} ");
                else if (board[i, j].IsGuess)
                {
                    Console.ForegroundColor= ConsoleColor.Cyan;
                    Console.Write($" {board[i, j].ToString()} ");
                    Console.ResetColor();
                }
                else
                    Console.Write("   ");
            }

            Console.WriteLine("║");
        }

        Console.WriteLine("╚═══╧═══╧═══╩═══╧═══╧═══╩═══╧═══╧═══╝");
    }

    private void DrawCursorOverlay()
    {
        int x = CursorColumn * 4;
        int y = CursorRow * 2;

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

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    Expert,
}

public enum Technique
{
    None = 0,
    NakedSingle = 1,
    HiddenSingle = 2,
    Guess = 99,   
}

public class Cell
{
    public int Value { get; set; } = 0;
    public int Guess { get; set; }
    public bool IsGiven { get; set; }
    public bool IsGuess { get; set; }

    public Cell(int value, bool isGiven)
    {
        Value = value;
        IsGiven = isGiven;
    }

    public override string ToString() => IsGiven ? Value.ToString() : Guess.ToString();
}

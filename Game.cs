using System.Diagnostics;

namespace SudokuCLI;

public class Game
{
    // PROPERTIES ==========
    private static readonly Random Rng = new Random();
    public Board Board { get; set; }
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;
    public Stopwatch Stopwatch { get; set; } = new Stopwatch();


    // CONSTRUCTOR ==========
    public Game(Difficulty difficulty)
    {
        Difficulty = difficulty;

        int[,] solution = new int[9, 9];
        Solve(solution);

        int[,] puzzle = (int[,])solution.Clone();
        Carve(puzzle, Difficulty);

        int givens = 0;
        foreach (int v in puzzle) if (v != 0) givens++;
        //Console.WriteLine($"{Difficulty}: {givens} pistas, técnica {LogicSolve(puzzle)}");
        //Console.ReadKey(true);

        Board = new Board(solution, puzzle, this);

        Stopwatch = Stopwatch.StartNew();

        bool haveWon = false;
        bool quit = false;

        // The main loop blocks on Console.ReadKey, so it cannot refresh the
        // clock on its own. This ticks the header once a second from a
        // background thread; Terminal.DrawLock keeps it from colliding with
        // the board redraw.
        using (Timer clock = new Timer(
            _ => Board.DrawHeader(),
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1)))
        {
            while (!haveWon && !quit)
            {
                Board.Display();
                Board.DrawCursorOverlay();
                quit = !Input.HandleBoardInput(Board);
                haveWon = Board.ValidateWin();
            }
        }

        Stopwatch.Stop();

        // Leaving the board is not finishing it: no conclusion screen, and the
        // clock timer is already disposed by the using above, so ClearAll in
        // the menu stays clear.
        if (quit)
        {
            Terminal.ClearAll();
            return;
        }

        double timeElapsed = Stopwatch.Elapsed.TotalMinutes;
        if (Navigation.Conclusion(timeElapsed))
            new Game(Difficulty);
        else
            Terminal.End();
    }



    // METHODS ==========
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

}

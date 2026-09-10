namespace SudokuCLI;

public class Sudoku
{
    // PROPERTIES ==========
    public Cell[,] Board { get; set; }
    public Difficulty Difficulty { get; set; } = Difficulty.Easy;


    // CONSTRUCTOR ==========
    public Sudoku()
    {
        Difficulty = PickDifficulty();
        Console.Clear();
        Board = new Cell[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                Board[row, column] = new Cell(Difficulty);
            }
        }

        Solve(Board);
        DisplayBoard(Board);
    }



    // METHODS ==========
    private Difficulty PickDifficulty()
    {
        Console.Write("Pick difficulty:\n1. Easy\n2. Medium\n3. Hard\n4. Expert\n");
        ConsoleKeyInfo key = Console.ReadKey();
        switch (key.KeyChar)
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
                Console.WriteLine("\nInvalid option. Setting difficulty to Easy.");
                return Difficulty.Easy;
        }
    }

    private bool Solve(Cell[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                if (board[row, column].Value == 0)
                {
                    Random rng = new Random();
                    List<int> values = Enumerable.Range(1, 9).OrderBy(x => rng.Next()).ToList();
                    foreach (int value in values)
                    {
                        if (IsValid(board, row, column, value))
                        {
                            board[row, column].Value = value;
                            if (Solve(board))
                                return true;
                            board[row, column].Value = 0;
                        }
                    }
                    return false;
                }
            }
        }
        return true;
    }

    private bool IsValid(Cell[,] board, int row, int column, int value)
    {
        for (int i = 0; i < 9; i++)
        {
            if (board[row, i].Value == value ||
                board[i, column].Value == value ||
                board[row / 3 * 3 + i / 3, column / 3 * 3 + i % 3].Value == value)
                return false;
        }

        return true;
    }

    private void DisplayBoard(Cell[,] board)
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (board[i, j].IsGiven)
                    Console.Write(board[i, j].ToString() + " ");
                else
                    Console.Write("  ");
            }
            Console.WriteLine();
        }
    }
}

public enum Difficulty
{
    Easy,
    Medium,
    Hard,
    Expert,
}

public class Cell
{
    public int Value { get; set; } = 0;
    public bool IsGiven { get; set; }
    public Cell(Difficulty difficulty)
    {
        // TODO: adicionar dificuldade
        IsGiven = RollTheDice(difficulty);
    }

    private bool RollTheDice(Difficulty difficulty)
    {
        Random rng = new Random();
        int roll = rng.Next(1, 101); 
        switch (difficulty)
        {
            case Difficulty.Easy:
                return roll <= 50;
            case Difficulty.Medium:
                return roll <= 40;
            case Difficulty.Hard:
                return roll <= 30;
            case Difficulty.Expert:
                return roll <= 20;
            default:
                return false;
        }
    }

    public override string ToString()
    {
        return Value.ToString();
        //┌───┐
        //│ X │
        //└───┘
    }
}

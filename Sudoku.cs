namespace SudokuCLI;

public class Sudoku
{
    // PROPERTIES ==========
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

        Board = new Cell[9, 9];
        for (int row = 0; row < 9; row++)
        {
            for (int column = 0; column < 9; column++)
            {
                Board[row, column] = new Cell(Difficulty);
            }
        }

        Solve(Board);

        while (true)
        {
            Console.SetCursorPosition(0,0);
            DisplayBoard(Board);
            DrawCursorOverlay();
            GetPlayerInput();
        }
    }

    private void GetPlayerInput()
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);

        if (keyInfo.KeyChar >= '1' && keyInfo.KeyChar <= '9')
        {
            if (!Board[CursorRow, CursorColumn].IsGiven)
            {
                Board[CursorRow, CursorColumn].IsGuess = true;
                Board[CursorRow, CursorColumn].Value = keyInfo.KeyChar - '0';
            }
        }
        else if (keyInfo.KeyChar == '0' || keyInfo.Key == ConsoleKey.Backspace || keyInfo.Key == ConsoleKey.Delete)
        {
            if (!Board[CursorRow, CursorColumn].IsGiven)
            {
                Board[CursorRow, CursorColumn].IsGuess = false;
                Board[CursorRow, CursorColumn].Value = 0;
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


    // METHODS ==========
    private Difficulty PickDifficulty()
    {
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
        // Calcula as posições absolutas na tela
        int x = CursorColumn * 4;
        int y = CursorRow * 2;

        Console.ForegroundColor = ConsoleColor.Red; // Escolha a cor do seu cursor

        // Desenha o teto da célula
        Console.SetCursorPosition(x, y);
        Console.Write("╔═══╗");

        // Desenha as paredes laterais (y + 1 é a linha do número)
        Console.SetCursorPosition(x, y + 1);
        Console.Write("║"); // Parede esquerda

        Console.SetCursorPosition(x + 4, y + 1);
        Console.Write("║"); // Parede direita

        // Desenha o chão da célula (y + 2 é a linha divisória de baixo)
        Console.SetCursorPosition(x, y + 2);
        Console.Write("╚═══╝");

        Console.ResetColor(); // Muito importante para não deixar o terminal vermelho
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
    public bool IsGuess { get; set; }

    public Cell(Difficulty difficulty)
    {
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

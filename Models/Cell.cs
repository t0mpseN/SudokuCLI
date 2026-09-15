namespace SudokuCLI;

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

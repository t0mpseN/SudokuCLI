using System.Diagnostics;
using System.Text;

namespace SudokuCLI;

public static class Terminal
{
    // CONSTANTS ===========
    public const int MinWidth = 25;
    public const int MinHeight = 30;


    // PROPERTIES ===========
    public static readonly object DrawLock = new object();


    // METHODS ===========
    public static void Setup()
    {
        Console.Title = "SudokuCLI";
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        Console.Out.Flush();
        ClearAll();
        Console.Out.Flush();

        WaitForValidSize();
    }

    public static void WaitForValidSize()
    {
        while (!IsLargeEnough())
        {
            ClearAll();

            Console.WriteLine();
            Console.WriteLine("Terminal too small.");
            Console.WriteLine();
            Console.WriteLine(
                $"Please resize your terminal to at least {MinWidth}x{MinHeight}."
            );
            Console.WriteLine();
            Console.WriteLine(
                $"Current size: {Console.WindowWidth}x{Console.WindowHeight}"
            );

            Thread.Sleep(100);
        }

        // Wait until the terminal dimensions stop changing
        int lastWidth = -1;
        int lastHeight = -1;

        while (Console.WindowWidth != lastWidth ||
               Console.WindowHeight != lastHeight)
        {
            lastWidth = Console.WindowWidth;
            lastHeight = Console.WindowHeight;

            Thread.Sleep(100);
        }

        ClearAll();
        Console.SetCursorPosition(0, 0);
    }

    public static void WriteCentered(string text, int y)
    {
        string[] lines = text.Replace("\r\n", "\n").Split('\n');

        foreach (string line in lines)
        {
            int x = Math.Max(
                0,
                (Console.WindowWidth - line.Length) / 2
            );

            Console.SetCursorPosition(x, y);
            Console.Write(line);

            y++;
        }
    }

    public static void WriteAtCenter(string text)
    {
        int y = Console.WindowHeight / 2;

        WriteCentered(text, y);
    }

    public static bool IsLargeEnough()
    {
        return Console.WindowWidth >= MinWidth &&
               Console.WindowHeight >= MinHeight;
    }

    public static int GetTextHeight(string text)
    {
        return text.Split('\n').Length;
    }

    public static void End()
    {
        ClearAll();
        Console.WriteLine("Thank you for playing SudokuCLI!");
        Thread.Sleep(500);
        Process.GetCurrentProcess().Kill();
    }

    public static void ClearAll()
    {
        Console.Write("\x1b[3J");  
        Console.Clear();           
        Console.Out.Flush();
        Console.SetCursorPosition(0, 0);
    }
}

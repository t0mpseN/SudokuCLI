namespace SudokuCLI;

public static class Input
{
    public static ConsoleKey ReadKey()
    {
        return Console.ReadKey(true).Key;
    }

    public static int VerticalSelection(string title, List<string> options)
    {
        int selected = 0;

        while (true)
        {
            Terminal.ClearAll();
            Console.SetCursorPosition(0, 0);

            int titleHeight = Terminal.GetTextHeight(title);
            int spacing = 2;

            int totalHeight = titleHeight + spacing + options.Count;
            int startY = Math.Max(0, (Console.WindowHeight - totalHeight) / 2);

            Terminal.WriteCentered(title, startY);

            int optionsY = startY + titleHeight + spacing;

            for (int i = 0; i < options.Count; i++)
            {
                int x = Math.Max(0, (Console.WindowWidth - options[i].Length) / 2);

                Console.SetCursorPosition(x, optionsY + i);
                Console.Write(options[i]);

                if (i == selected)
                {
                    Console.SetCursorPosition(Math.Max(0, x - 2), optionsY + i);
                    Console.Write("▶");
                }
            }

            ConsoleKey key = ReadKey();

            switch (key)
            {
                case ConsoleKey.UpArrow:
                    selected = Math.Max(0, selected - 1);
                    break;

                case ConsoleKey.DownArrow:
                    selected = Math.Min(options.Count - 1, selected + 1);
                    break;

                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    return selected;

                case ConsoleKey.Escape:
                    return -1;
            }
        }
    }

    // Returns false when the player asked to leave the board (Esc), so the
    // game loop can stop instead of only ending this one keypress.
    public static bool HandleBoardInput(Board board)
    {
        ConsoleKeyInfo keyInfo = Console.ReadKey(true);
        if (keyInfo.KeyChar >= '1' && keyInfo.KeyChar <= '9')
        {
            board.SetGuess(keyInfo.KeyChar - '0');
        }
        else if (keyInfo.KeyChar == '0' || keyInfo.Key == ConsoleKey.Backspace || keyInfo.Key == ConsoleKey.Delete)
        {
            board.ClearGuess();
        }
        else
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    board.MoveCursor(-1, 0);
                    break;
                case ConsoleKey.DownArrow:
                    board.MoveCursor(1, 0);
                    break;
                case ConsoleKey.LeftArrow:
                    board.MoveCursor(0, -1);
                    break;
                case ConsoleKey.RightArrow:
                    board.MoveCursor(0, 1);
                    break;
                case ConsoleKey.Escape:
                    return false;
            }
        }

        return true;
    }
}

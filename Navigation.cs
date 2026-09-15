
namespace SudokuCLI;

public static class Navigation
{
    public static void MainMenu()
    {
        Terminal.ClearAll();
        string title = "┏━━━┓╋╋╋╋┏┓╋╋┏┓╋╋╋╋┏━━━┳┓╋╋┏━━┓\n" +
                       "┃┏━┓┃╋╋╋╋┃┃╋╋┃┃╋╋╋╋┃┏━┓┃┃╋╋┗┫┣┛\n" +
                       "┃┗━━┳┓┏┳━┛┣━━┫┃┏┳┓┏┫┃╋┗┫┃╋╋╋┃┃ \n" +
                       "┗━━┓┃┃┃┃┏┓┃┏┓┃┗┛┫┃┃┃┃╋┏┫┃╋┏┓┃┃ \n" +
                       "┃┗━┛┃┗┛┃┗┛┃┗┛┃┏┓┫┗┛┃┗━┛┃┗━┛┣┫┣┓\n" +
                       "┗━━━┻━━┻━━┻━━┻┛┗┻━━┻━━━┻━━━┻━━┛";
        List<string> options = new List<string>
        {
            "PLAY",
            "SETTINGS",
            "STATISTICS",
            "EXIT"
        };

        int selection = Input.VerticalSelection(title, options);

        switch (selection)
        {
            case 0:
                Board();
                break;
            case 1:
                Settings();
                break;
            case 2:
                MainMenu();
                break;
            case 3:
                Terminal.End();
                break;
            default:
                MainMenu();
                break;
        }
    }

    private static Difficulty PickDifficulty()
    {
        Console.Clear();
        string title = "PICK DIFFICULTY";

        int difficulty = Input.VerticalSelection(title, new List<string> { "Easy", "Medium", "Hard", "Expert" });

        switch (difficulty)
        {
            case 0:
                return Difficulty.Easy;
            case 1:
                return Difficulty.Medium;
            case 2:
                return Difficulty.Hard;
            case 3:
                return Difficulty.Expert;
            default:
                return PickDifficulty();
        }
    }

    public static void Board()
    {
        Terminal.ClearAll();
        Game game = new Game(PickDifficulty());

        // The game returns here when the player leaves with Esc.
        MainMenu();
    }

    public static bool Conclusion(double timeElapsed)
    {
        Terminal.ClearAll();
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
                return Conclusion(timeElapsed);
        }
    }

    public static void Settings()
    {

    }
}

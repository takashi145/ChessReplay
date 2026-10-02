namespace ChessReplay.Ui;

internal static class Terminal
{
    public static int Width
    {
        get
        {
            try
            {
                return Console.WindowWidth;
            }
            catch (IOException)
            {
                return 80;
            }
        }
    }

    public static int Height
    {
        get
        {
            try
            {
                return Console.WindowHeight;
            }
            catch (IOException)
            {
                return 24;
            }
        }
    }

    public static IDisposable HideCursor()
    {
        SetCursorVisible(false);
        return new CursorRestorer();
    }

    private static void SetCursorVisible(bool visible)
    {
        try
        {
            Console.CursorVisible = visible;
        }
        catch (IOException)
        {
        }
    }

    private sealed class CursorRestorer : IDisposable
    {
        public void Dispose() => SetCursorVisible(true);
    }
}

namespace ChessReplay.Diagnostics;

public sealed class VerboseLog(bool enabled)
{
    public void Info(string message)
    {
        if (enabled)
            Console.Error.WriteLine($"[verbose] {message}");
    }
}

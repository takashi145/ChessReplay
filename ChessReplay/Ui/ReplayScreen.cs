using ChessReplay.Chess;
using ChessReplay.Models;
using Spectre.Console;

namespace ChessReplay.Ui;

public enum ReplayExit
{
    Quit,
    Back,
}

public sealed class ReplayScreen(
    ChessGame game,
    IReadOnlyList<BoardSnapshot> snapshots,
    string focusUsername,
    bool canGoBack = false)
{
    private const int ProgressBarWidth = 24;

    private int index;
    private bool flipped;

    public ReplayExit Run()
    {
        using var _ = Terminal.HideCursor();

        Draw();

        while (true)
        {
            var exit = HandleKey(Console.ReadKey(true).Key);
            if (exit is { } result)
                return result;

            Draw();
        }
    }

    private ReplayExit? HandleKey(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.RightArrow:
                if (index < snapshots.Count - 1) index++;
                break;
            case ConsoleKey.LeftArrow:
                if (index > 0) index--;
                break;
            case ConsoleKey.F:
                flipped = !flipped;
                break;
            case ConsoleKey.Home:
                index = 0;
                break;
            case ConsoleKey.End:
                index = snapshots.Count - 1;
                break;
            case ConsoleKey.B when canGoBack:
                return ReplayExit.Back;
            case ConsoleKey.Q or ConsoleKey.Escape:
                return ReplayExit.Quit;
        }

        return null;
    }

    private void Draw()
    {
        var snapshot = snapshots[index];
        var top = flipped ? game.White : game.Black;
        var bottom = flipped ? game.Black : game.White;

        AnsiConsole.Clear();
        AnsiConsole.MarkupLine($" {FormatPlayer(top)}");
        AnsiConsole.MarkupLine("          vs");
        AnsiConsole.MarkupLine($"   {FormatPlayer(bottom)}");
        AnsiConsole.WriteLine();
        AnsiConsole.Markup(BoardRenderer.Render(snapshot, flipped));
        AnsiConsole.WriteLine();
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(FormatMove(snapshot));
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(RenderProgressBar(index, snapshots.Count - 1));
        AnsiConsole.WriteLine();
        var backHint = canGoBack ? "B Back to list   " : "";
        AnsiConsole.MarkupLine($"[grey]← Previous   → Next   F Flip   Home/End   {backHint}Q Quit[/]");
    }

    private string FormatPlayer(Player player)
    {
        var label = player.Rating is { } rating ? $"{player.Username} ({rating})" : player.Username;
        var isFocusPlayer = string.Equals(player.Username, focusUsername, StringComparison.OrdinalIgnoreCase);

        return isFocusPlayer ? $"[bold yellow]{Markup.Escape(label)}[/]" : $"[bold]{Markup.Escape(label)}[/]";
    }

    private static string FormatMove(BoardSnapshot snapshot)
    {
        if (snapshot.San is null)
            return "Starting position";

        var fullMoveNumber = (snapshot.MoveNumber + 1) / 2;
        var isWhiteMove = snapshot.MoveNumber % 2 == 1;
        var label = isWhiteMove ? $"{fullMoveNumber}. {snapshot.San}" : $"{fullMoveNumber}... {snapshot.San}";

        return Markup.Escape(label);
    }

    private static string RenderProgressBar(int index, int total)
    {
        var filled = total == 0 ? ProgressBarWidth : (int)Math.Round(ProgressBarWidth * (double)index / total);
        var bar = new string('━', filled) + new string('─', ProgressBarWidth - filled);
        return $"{bar} {index} / {total}";
    }
}

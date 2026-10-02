using ChessReplay.Models;
using Spectre.Console;

namespace ChessReplay.Ui;

public static class GameSelector
{
    private const int ReservedLines = 4;
    private const int MinPageSize = 3;
    private const int QuitChoice = -1;

    public readonly record struct Entry(ChessGame Game, int? MoveCount);

    public static ChessGame? Select(string title, string focusUsername, IReadOnlyList<Entry> entries)
    {
        if (entries.Count == 0)
            return null;

        var width = Terminal.Width;
        var showTimeClass = width >= 70;
        var showMoveCount = width >= 85;
        var showDate = width >= 100;
        var pageSize = Math.Clamp(Terminal.Height - ReservedLines, MinPageSize, entries.Count + 1);

        string FormatChoice(int index)
        {
            if (index == QuitChoice)
                return "[grey]Q Quit[/]";

            var entry = entries[index];
            var opponent = entry.Game.GetOpponent(focusUsername);
            var (result, _) = GameOutcome.Describe(entry.Game.GetPlayer(focusUsername), opponent);

            var line = $"vs {Markup.Escape(opponent.Username),-16} {result,-6}";
            if (showTimeClass) line += $" {Markup.Escape(entry.Game.TimeClass),-8}";
            if (showMoveCount) line += $" {(entry.MoveCount is { } m ? $"{m} moves" : "? moves"),-10}";
            if (showDate) line += $" {entry.Game.EndTime.LocalDateTime:yyyy-MM-dd HH:mm}";

            return $"{index + 1,3}. {line}";
        }

        var prompt = new SelectionPrompt<int>()
            .Title($"[bold]{Markup.Escape(title)}[/] [grey]({entries.Count} games)[/]")
            .PageSize(pageSize)
            .WrapAround(true)
            .UseConverter(FormatChoice);

        prompt.AddChoices(Enumerable.Range(0, entries.Count));
        prompt.AddChoice(QuitChoice);

        using var _ = Terminal.HideCursor();
        var selected = AnsiConsole.Prompt(prompt);

        return selected == QuitChoice ? null : entries[selected].Game;
    }
}

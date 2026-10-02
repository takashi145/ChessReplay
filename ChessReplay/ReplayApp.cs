using ChessReplay.Api;
using ChessReplay.Chess;
using ChessReplay.Diagnostics;
using ChessReplay.Models;
using ChessReplay.Ui;

namespace ChessReplay;

internal sealed class ReplayApp(ChessComClient client, VerboseLog log)
{
    public async Task RunLatestAsync(string username, CancellationToken ct)
    {
        var archives = await client.GetArchivesAsync(username, ct);
        if (archives.Count == 0)
            throw new NoGamesFoundException(username);

        ChessGame? latest = null;
        for (var i = archives.Count - 1; i >= 0 && latest is null; i--)
        {
            var games = await client.GetGamesAsync(username, archives[i], ct);
            log.Info($"Archive {archives[i]}: {games.Count} games");

            if (games.Count > 0)
                latest = games.OrderByDescending(g => g.EndTime).First();
        }

        Replay(latest ?? throw new NoGamesFoundException(username), username);
    }

    public async Task RunRandomAsync(string username, CancellationToken ct)
    {
        var archives = await client.GetArchivesAsync(username, ct);
        if (archives.Count == 0)
            throw new NoGamesFoundException(username);

        var shuffled = archives.OrderBy(_ => Random.Shared.Next()).Take(6);

        foreach (var archive in shuffled)
        {
            var games = await client.GetGamesAsync(username, archive, ct);
            log.Info($"Archive {archive}: {games.Count} games");

            if (games.Count > 0)
            {
                Replay(games[Random.Shared.Next(games.Count)], username);
                return;
            }
        }

        throw new NoGamesFoundException(username);
    }

    public async Task RunLastAsync(string username, int count, CancellationToken ct)
    {
        count = Math.Max(1, count);

        var archives = await client.GetArchivesAsync(username, ct);
        if (archives.Count == 0)
            throw new NoGamesFoundException(username);

        var collected = new List<ChessGame>();
        for (var i = archives.Count - 1; i >= 0 && collected.Count < count; i--)
        {
            var games = await client.GetGamesAsync(username, archives[i], ct);
            log.Info($"Archive {archives[i]}: {games.Count} games");
            collected.AddRange(games);
        }

        var recent = collected.OrderByDescending(g => g.EndTime).Take(count).ToList();
        if (recent.Count == 0)
            throw new NoGamesFoundException(username);

        SelectAndReplay($"{username} — Recent games", username, recent);
    }

    public async Task RunMonthAsync(string username, GameArchive archive, CancellationToken ct)
    {
        var games = await client.GetGamesAsync(username, archive, ct);
        log.Info($"Archive {archive}: {games.Count} games");

        if (games.Count == 0)
            throw new NoGamesFoundException(username, archive);

        SelectAndReplay($"{username} — Games in {archive}", username, games);
    }

    private void SelectAndReplay(string title, string username, IReadOnlyList<ChessGame> games)
    {
        var entries = games
            .OrderByDescending(g => g.EndTime)
            .Select(g => new GameSelector.Entry(g, TryCountMoves(g.Pgn)))
            .ToList();

        while (true)
        {
            var selected = GameSelector.Select(title, username, entries);
            if (selected is null)
                return;

            if (Replay(selected, username, canGoBack: true) == ReplayExit.Quit)
                return;
        }
    }

    private static int? TryCountMoves(string pgn)
    {
        try
        {
            return PgnParser.Parse(pgn).ExecutedMoves.Count;
        }
        catch (PgnParseException)
        {
            return null;
        }
    }

    private ReplayExit Replay(ChessGame game, string username, bool canGoBack = false)
    {
        var board = PgnParser.Parse(game.Pgn);
        var snapshots = ReplayBuilder.Build(board);
        log.Info($"Built {snapshots.Count} board snapshots");

        return new ReplayScreen(game, snapshots, username, canGoBack).Run();
    }
}

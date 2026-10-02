using ChessReplay.Api;
using ChessReplay.Chess;
using ChessReplay.Diagnostics;
using ChessReplay.Models;
using ChessReplay.Ui;

namespace ChessReplay;

internal sealed class ReplayApp(ChessComClient client, VerboseLog log)
{
    private const string Chess960Message = "Chess960 games are not supported yet.";

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
                latest = games.Where(g => !g.IsChess960).OrderByDescending(g => g.EndTime).FirstOrDefault();
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

            var playable = games.Where(g => !g.IsChess960).ToList();
            if (playable.Count > 0)
            {
                Replay(playable[Random.Shared.Next(playable.Count)], username);
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
        var playableCount = 0;
        for (var i = archives.Count - 1; i >= 0 && playableCount < count; i--)
        {
            var games = await client.GetGamesAsync(username, archives[i], ct);
            log.Info($"Archive {archives[i]}: {games.Count} games");
            collected.AddRange(games);
            playableCount += games.Count(g => !g.IsChess960);
        }

        var recent = new List<ChessGame>();
        var hidden = 0;
        foreach (var game in collected.OrderByDescending(g => g.EndTime))
        {
            if (game.IsChess960)
                hidden++;
            else if (recent.Count < count)
                recent.Add(game);

            if (recent.Count == count)
                break;
        }

        if (recent.Count == 0 && hidden == 0)
            throw new NoGamesFoundException(username);

        SelectAndReplay($"{username} — Recent games (UTC)", username, recent, hidden);
    }

    public async Task RunMonthAsync(string username, GameArchive archive, CancellationToken ct)
    {
        var games = await client.GetGamesAsync(username, archive, ct);
        log.Info($"Archive {archive}: {games.Count} games");

        if (games.Count == 0)
            throw new NoGamesFoundException(username, archive);

        var playable = games.Where(g => !g.IsChess960).ToList();
        SelectAndReplay($"{username} — Games in {archive} (UTC)", username, playable, games.Count - playable.Count);
    }

    private void SelectAndReplay(string title, string username, IReadOnlyList<ChessGame> games, int hiddenChess960)
    {
        if (games.Count == 0)
            throw new PgnParseException($"All {hiddenChess960} games are Chess960, which is not supported yet.");

        var entries = games
            .OrderByDescending(g => g.EndTime)
            .Select(g => new GameSelector.Entry(g, TryCountMoves(g.Pgn)))
            .ToList();

        while (true)
        {
            var selected = GameSelector.Select(title, username, entries, hiddenChess960);
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
        if (game.IsChess960)
            throw new PgnParseException(Chess960Message);

        var board = PgnParser.Parse(game.Pgn);
        var snapshots = ReplayBuilder.Build(board);
        log.Info($"Built {snapshots.Count} board snapshots");

        return new ReplayScreen(game, snapshots, username, canGoBack).Run();
    }
}

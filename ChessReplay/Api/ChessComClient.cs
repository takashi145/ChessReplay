using System.Net;
using System.Net.Http.Json;
using ChessReplay.Diagnostics;
using ChessReplay.Models;

namespace ChessReplay.Api;

public sealed class ChessComClient(VerboseLog log) : IDisposable
{
    private const string BaseUrl = "https://api.chess.com/pub";

    private readonly HttpClient httpClient = CreateHttpClient();

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "chess-replay/0.1 (+https://github.com/takashi145/ChessReplay)");
        return client;
    }

    public async Task<IReadOnlyList<GameArchive>> GetArchivesAsync(string username, CancellationToken ct)
    {
        var url = $"{BaseUrl}/player/{Uri.EscapeDataString(username)}/games/archives";
        var dto = await GetAsync<ArchivesResponseDto>(url, () => new PlayerNotFoundException(username), ct);

        return ParseArchives(dto.Archives)
            .OrderBy(a => a.Year)
            .ThenBy(a => a.Month)
            .ToList();
    }

    private List<GameArchive> ParseArchives(IEnumerable<string> archiveUrls)
    {
        var archives = new List<GameArchive>();
        foreach (var archiveUrl in archiveUrls)
        {
            if (GameArchive.TryParseFromUrl(archiveUrl, out var archive))
                archives.Add(archive);
            else
                log.Info($"Could not parse archive URL: {archiveUrl}");
        }

        return archives;
    }

    public async Task<IReadOnlyList<ChessGame>> GetGamesAsync(string username, GameArchive archive, CancellationToken ct)
    {
        var url = $"{BaseUrl}/player/{Uri.EscapeDataString(username)}/games/{archive.Year:D4}/{archive.Month:D2}";
        GamesResponseDto dto;
        try
        {
            dto = await GetAsync<GamesResponseDto>(url, () => new NoGamesFoundException(username, archive), ct);
        }
        catch (NoGamesFoundException)
        {
            await GetArchivesAsync(username, ct);
            throw;
        }

        return dto.Games
            .Where(g => !string.IsNullOrWhiteSpace(g.Pgn))
            .Select(ToChessGame)
            .ToList();
    }

    private async Task<T> GetAsync<T>(string url, Func<Exception> onNotFound, CancellationToken ct)
    {
        log.Info($"GET {url}");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.GetAsync(url, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // The caller cancelled (e.g. Ctrl+C) - let it propagate as-is.
        }
        catch (TaskCanceledException ex)
        {
            throw new ChessComConnectionException("The request to Chess.com timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ChessComConnectionException("Could not connect to Chess.com.", ex);
        }

        log.Info($"HTTP {(int)response.StatusCode} {response.StatusCode}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            throw onNotFound();

        if (!response.IsSuccessStatusCode)
            throw new ChessComApiException(
                $"Chess.com API returned {(int)response.StatusCode} {response.StatusCode}.",
                response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
        return result ?? throw new ChessComApiException("Chess.com API returned an empty response.", response.StatusCode);
    }

    private static ChessGame ToChessGame(GameDto dto) => new(
        Pgn: dto.Pgn!,
        EndTime: DateTimeOffset.FromUnixTimeSeconds(dto.EndTime),
        TimeControl: dto.TimeControl,
        TimeClass: dto.TimeClass,
        Rated: dto.Rated,
        White: new Player(dto.White.Username, dto.White.Rating, dto.White.Result),
        Black: new Player(dto.Black.Username, dto.Black.Rating, dto.Black.Result),
        Url: dto.Url,
        Rules: dto.Rules);

    public void Dispose() => httpClient.Dispose();
}

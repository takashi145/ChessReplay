using System.Text.Json.Serialization;

namespace ChessReplay.Api;

internal sealed class ArchivesResponseDto
{
    [JsonPropertyName("archives")]
    public List<string> Archives { get; init; } = [];
}

internal sealed class GamesResponseDto
{
    [JsonPropertyName("games")]
    public List<GameDto> Games { get; init; } = [];
}

internal sealed class GameDto
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = "";

    [JsonPropertyName("pgn")]
    public string? Pgn { get; init; }

    [JsonPropertyName("time_control")]
    public string TimeControl { get; init; } = "";

    [JsonPropertyName("end_time")]
    public long EndTime { get; init; }

    [JsonPropertyName("rated")]
    public bool Rated { get; init; }

    [JsonPropertyName("time_class")]
    public string TimeClass { get; init; } = "";

    [JsonPropertyName("rules")]
    public string Rules { get; init; } = "chess";

    [JsonPropertyName("white")]
    public PlayerDto White { get; init; } = new();

    [JsonPropertyName("black")]
    public PlayerDto Black { get; init; } = new();
}

internal sealed class PlayerDto
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = "";

    [JsonPropertyName("rating")]
    public int? Rating { get; init; }

    [JsonPropertyName("result")]
    public string Result { get; init; } = "";
}

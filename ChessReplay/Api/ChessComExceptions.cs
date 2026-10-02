using System.Net;
using ChessReplay.Models;

namespace ChessReplay.Api;

public sealed class PlayerNotFoundException(string username)
    : Exception($"Player \"{username}\" was not found on Chess.com.");

public sealed class NoGamesFoundException : Exception
{
    public NoGamesFoundException(string username)
        : base($"No public games found for \"{username}\".")
    {
    }

    public NoGamesFoundException(string username, GameArchive archive)
        : base($"No public games found for \"{username}\" in {archive}.")
    {
    }
}

public sealed class ChessComApiException(string message, HttpStatusCode? statusCode) : Exception(message)
{
    public HttpStatusCode? StatusCode { get; } = statusCode;
}

public sealed class ChessComConnectionException(string message, Exception inner) : Exception(message, inner);

using System.CommandLine;
using ChessReplay;
using ChessReplay.Api;
using ChessReplay.Chess;
using ChessReplay.Diagnostics;
using ChessReplay.Models;
using Spectre.Console;

var usernameArgument = new Argument<string>("username")
{
    Description = "Chess.com username",
};

var lastOption = new Option<int?>("--last")
{
    Description = "List the most recent N games to choose from",
};

var randomOption = new Option<bool>("--random")
{
    Description = "Replay a random past game",
};

var monthOption = new Option<string?>("--month")
{
    Description = "List games from a specific month (YYYY-MM)",
};

var verboseOption = new Option<bool>("--verbose")
{
    Description = "Print diagnostic information",
};

var rootCommand = new RootCommand("Replay Chess.com public games in the terminal")
{
    usernameArgument,
    lastOption,
    randomOption,
    monthOption,
    verboseOption,
};

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    if (Console.IsInputRedirected)
    {
        AnsiConsole.MarkupLine("[red]chess-replay needs an interactive terminal.[/]");
        return 1;
    }

    var username = parseResult.GetValue(usernameArgument)!;
    var last = parseResult.GetValue(lastOption);
    var random = parseResult.GetValue(randomOption);
    var month = parseResult.GetValue(monthOption);
    var verbose = parseResult.GetValue(verboseOption);

    var log = new VerboseLog(verbose);
    using var client = new ChessComClient(log);
    var app = new ReplayApp(client, log);

    try
    {
        if (month is not null)
        {
            if (!GameArchive.TryParse(month, out var archive))
            {
                AnsiConsole.MarkupLine("[red]--month expects the format YYYY-MM.[/]");
                return 1;
            }

            await app.RunMonthAsync(username, archive, cancellationToken);
        }
        else if (last is { } count)
            await app.RunLastAsync(username, count, cancellationToken);
        else if (random)
            await app.RunRandomAsync(username, cancellationToken);
        else
            await app.RunLatestAsync(username, cancellationToken);

        return 0;
    }
    catch (OperationCanceledException)
    {
        return 130;
    }
    catch (ChessComConnectionException ex)
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
        AnsiConsole.MarkupLine("[red]Check your internet connection and try again.[/]");
        return 1;
    }
    catch (Exception ex) when (ex is PlayerNotFoundException or NoGamesFoundException
                                     or ChessComApiException or PgnParseException)
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
        return 1;
    }
});

var parseResult = rootCommand.Parse(args);
return await parseResult.InvokeAsync();

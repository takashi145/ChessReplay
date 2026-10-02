# ChessReplay

A tool to replay Chess.com games in the terminal.

## Requirements

- .NET 10 SDK

## Usage

```
dotnet run --project ChessReplay -- <username> [options]
```

| Argument / Option | Description |
|---|---|
| `<username>` | Chess.com username (required) |
| `--last <N>` | List the most recent N games to choose from |
| `--random` | Replay a random past game |
| `--month <YYYY-MM>` | List games from a specific month |
| `--verbose` | Print diagnostic information to stderr |

With no options, it replays your single most recent game.

### Examples

```
# Replay the single most recent game
dotnet run --project ChessReplay -- <username>

# List the 20 most recent games to pick from
dotnet run --project ChessReplay -- <username> --last 20

# List games played in August 2026
dotnet run --project ChessReplay -- <username> --month 2026-08

# Replay a random past game
dotnet run --project ChessReplay -- <username> --random
```

## Controls

**Game list** (`--last` / `--month`)

| Key | Action |
|---|---|
| `↑` / `↓` | Move selection |
| `Enter` | Replay selected game |
| `Q` / `Esc` | Quit |

**Replay screen**

| Key | Action |
|---|---|
| `←` / `→` | Previous / next move |
| `Home` / `End` | Jump to start / end of game |
| `F` | Flip board |
| `B` | Back to game list (when opened from a list) |
| `Q` / `Esc` | Quit |

## License

MIT

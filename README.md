# WMR.Monads

Zero-overhead `Result` and `Maybe` monads for .NET 10.

## Features

- **`Result` / `Result<T>`** — railway-oriented error handling with `Error`, `Warning`, `Information` messages.
- **`Maybe<T>`** — explicit optional values.
- Rich extension API: `Map`, `Bind`, `Ensure`, `Tap`, `Match`, `Filter`, `Or`, plus async variants.
- Built-in `BadRequest`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Server` error types.

## Projects

| Project | Purpose |
|---------|---------|
| `Monads` | Library (`MWR.Monads`) |
| `Monads.Tests` | xUnit suite (346 tests) |

## Build & Test

```bash
dotnet build
dotnet test
```

## Status

Pre-release.

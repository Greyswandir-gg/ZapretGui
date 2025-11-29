## zapret-gui

WPF mini-Discord UI plus CLI adapter for `zapret-discord-youtube`.

- `ZapretCli` — JSON-only adapter with commands: `status`, `list-strategies`, `run-strategy`, `stop`.
- `ZapretGui` — MVVM WPF front-end that calls `zapret-cli.exe` only.
- Config example: `config/zapret-adapter.example.json`.

### Quick start

```powershell
# build
dotnet build zapret-gui.sln

# run CLI
dotnet run --project src/ZapretCli -- list-strategies --config config/zapret-adapter.example.json

# run GUI
dotnet run --project src/ZapretGui
```

State/config files live under `%AppData%\zapret-gui`. The zapret folder is never modified by this repo.

## zapret-gui architecture

- Solution: `zapret-gui.sln`
  - `ZapretCli` — console adapter around `zapret-discord-youtube` BATs. Only process execution + stdout/stderr JSON. Never writes to the zapret folder.
  - `ZapretGui` — WPF mini-Discord UI (MVVM + CommunityToolkit.Mvvm). Talks to `zapret-cli.exe` only.
  - Tests: `ZapretCli.Tests`, `ZapretGui.Tests`.

### ZapretCli

- Config discovery (in order): `--config path`, env `ZAPRET_ADAPTER_CONFIG`, `zapret-adapter.json` next to `zapret-cli.exe`.
- Models: `ZapretConfig`, `StrategyItem`, `StatusResult`, etc. Plain JSON with camelCase.
- Services:
  - `ConfigLoader` — reads JSON config, validates `zapretPath` (else `{ok:false, error:"invalid_zapret_path"}`).
  - `StrategyRepository` — lists BAT files by mask, derives `displayName` from filename.
  - `ZapretProcessRunner` — detects/kills `winws*` processes, starts BAT via `cmd /c "<file>"`.
  - `FileLastStateStore` — stores `last-state.json` in `%AppData%\zapret-gui` (fallback: exe folder).
  - `JsonPrinter` — prints only JSON to stdout.
- CLI surface (System.CommandLine): `status`, `list-strategies`, `run-strategy <file>`, `stop`.
  - All errors: non-zero exit, `{ "ok": false, "error": "..." }`.
  - No writes to zapret folder. All local state stays in `%AppData%\zapret-gui`.

### ZapretGui

- MVVM:
  - `MainViewModel` wires tabs: `Strategies`, `Resources`, `Diagnostics`, `Settings`.
  - `StrategiesViewModel` — async CLI calls for list/status/run/stop.
  - `SettingsViewModel` — stores zapret/CLI paths, writes `%AppData%\zapret-gui\gui-settings.json` and `zapret-adapter.json`.
  - `DiagnosticsViewModel` — log of CLI calls (timestamp, command, raw result).
- Services:
  - `GuiSettingsStore` — load/save GUI settings; writes CLI config to `%AppData%\zapret-gui\zapret-adapter.json`.
  - `ZapretCliClient` — runs `zapret-cli.exe ... --config <appdata>/zapret-adapter.json`, parses JSON, logs diagnostics.
- UI:
  - Discord-like palette (`#202225` sidebar, `#2f3136` content, accent `#5865f2`).
  - Sidebar channels: `# strategies`, `# resources`, `# diagnostics`, `# settings`.
  - Strategies: list BATs left, status + run/stop buttons right.
  - Resources: placeholder text.
  - Diagnostics: scrollable log.
  - Settings: pick zapret folder, CLI path, save & test connection.

### Rules enforced

- GUI never calls BATs directly; only `zapret-cli.exe`.
- No writes inside `zapret-discord-youtube`.
- Active strategy persisted only in `%AppData%\zapret-gui\last-state.json`.

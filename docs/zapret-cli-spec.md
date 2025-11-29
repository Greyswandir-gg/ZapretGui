## zapret-cli specification

### Config

- JSON file fields:
  - `zapretPath` (required, must exist)
  - `generalMask` (e.g. `general (*.bat)`)
  - `serviceScript` (reserved)
- Discovery order: `--config "<path>"` > env `ZAPRET_ADAPTER_CONFIG` > `zapret-adapter.json` beside `zapret-cli.exe`.
- Invalid or missing `zapretPath` → exit code != 0, `{ "ok": false, "error": "invalid_zapret_path" }`.

### Commands

`status`
```json
{
  "ok": true,
  "state": {
    "isRunning": true,
    "activeStrategy": "general (ALT3).bat",
    "gameFilter": "unknown",
    "ipsetMode": "unknown",
    "processes": [ { "name": "winws.exe", "pid": 1234 } ]
  }
}
```

`list-strategies`
```json
{
  "ok": true,
  "strategies": [
    { "fileName": "general (ALT).bat", "displayName": "ALT", "path": "C:\\Tools\\zapret-discord-youtube\\general (ALT).bat" }
  ]
}
```

`run-strategy <fileName>`
```json
{ "ok": true, "started": true, "strategy": "general (ALT3).bat" }
```

`stop`
```json
{ "ok": true, "stoppedProcesses": [ { "name": "winws.exe", "pid": 1234 } ] }
```

### Process behavior

- Detect running zapret by process names `winws.exe`, `winws64.exe` (case-insensitive).
- Starting strategy: `cmd.exe /c "<file>"`, working directory = `zapretPath`, `UseShellExecute=false`, `CreateNoWindow=true`.
- `stop` kills running zapret processes before returning.
- `run-strategy` also issues a stop before start.

### State and safety

- Active strategy stored only in `%AppData%\\zapret-gui\\last-state.json` (fallback: alongside CLI).
- CLI outputs JSON to stdout only; stderr unused for UX.
- Never write into `zapretPath` or modify files inside `zapret-discord-youtube`.

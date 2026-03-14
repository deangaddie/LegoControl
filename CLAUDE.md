# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                                  # build all projects
dotnet run --project src/LegoControl.UI       # run the Blazor app (opens in browser)
```

## Project Overview

LegoControl is a browser-based Blazor WebAssembly app for controlling Lego motorised devices via Bluetooth. It runs entirely client-side in the browser — no backend server.

## Architecture

Two projects in `src/`:

- **[LegoControl.Core](src/LegoControl.Core/)** — class library; Lego device models, Bluetooth logic, settings. No UI dependencies.
- **[LegoControl.UI](src/LegoControl.UI/)** — Blazor WebAssembly app; all pages and components. References Core. Uses **MudBlazor** (Material Design) for UI components.

**Connectivity:** Web Bluetooth API (Chromium only) — called from JS interop in UI, abstracted behind interfaces in Core.
**Persistence:** Browser storage (localStorage/IndexedDB); JSON export/import for portability.
**Target hardware:** Lego Boost (set 17101) — in progress.

## Conventions

- Keep [README.md](README.md) updated as the project evolves — supported sets, architecture changes, new features, etc.

## Key Design Constraints

- No server-side component — everything runs locally in the user's browser
- Must use Web Bluetooth API (only available in Chromium-based browsers)
- Data portability via JSON export/import (no cloud sync)

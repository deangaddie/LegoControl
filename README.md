# LegoControl
Control Lego motorised devices by Bluetooth in the browser

Uses the Web Bluetooth API to control connected Lego devices. The app is a Blazor WebAssembly app that runs entirely locally in the user's browser — no backend server.

Settings, devices, and configurations are stored in browser storage. Data can be exported to a JSON file and imported into another browser.

The app is a PWA — it can be installed from the browser and works offline.

## Features

- **Touch controls** — dual on-screen joysticks for throttle and steering (touch-capable devices)
- **Keyboard controls** — customisable key bindings (arrow keys by default)
- **Gamepad / controller support** — configurable axis mapping, dead zone, and inversion
- **Visual programming** — Blockly-based editor with Lego-specific blocks (drive, steer, stop, wait, loops, conditions, variables)
- **Data portability** — JSON export/import for moving configs between browsers

## Solution Structure

```text
LegoControl.sln
src/
  LegoControl.Core/   # Device models, Bluetooth logic, settings (class library)
  LegoControl.UI/     # Blazor WebAssembly frontend
  LegoControl.Tests/  # Unit tests
```

## Building and Running

```bash
# Build
dotnet build

# Run (opens in browser)
dotnet run --project src/LegoControl.UI
```

## Deployment

The app is a static Blazor WebAssembly site and can be hosted on any static host. It is configured for [Netlify](https://netlify.com) via [netlify.toml](netlify.toml).

**To deploy on Netlify:**

1. Push this repo to GitHub/GitLab
2. Connect the repo in Netlify — it will detect `netlify.toml` automatically
3. Deploy — Netlify runs `dotnet publish` and serves `publish/wwwroot`

> **Note:** .NET SDK must be available on the Netlify build image. If the build fails with a missing SDK error, add `DOTNET_VERSION = 10.0` as an environment variable in Netlify's site settings.

### Alternative: manual drag-and-drop deploy

1. Build locally: `dotnet publish src/LegoControl.UI -c Release -o publish`
2. In Netlify, go to your site's **Deploys** tab and drag the `publish/wwwroot` folder into the drop zone
3. Go to **Site configuration → Redirects** and add a rule: `/* → /index.html` with status `200` (required for client-side routing)

## Supported Platforms

Web Bluetooth is required and is only available in Chromium-based browsers. In production, HTTPS is required (localhost works without it during development).

| Platform | Browser | Supported |
| --- | --- | --- |
| Windows / macOS / Linux | Chrome, Edge | Yes |
| Android | Chrome | Yes |
| ChromeOS | Chrome | Yes |
| iOS | Any | No — WebKit does not support Web Bluetooth |
| Any | Firefox, Safari | No |

## Supported Lego Sets

| Set | Name | Hub | Notes |
| --- | --- | --- | --- |
| #42160 | Audi RS Q e-tron | Technic Hub | Rear + front drive motors (linked), steering motor |
| #17101 | Boost | Move Hub | Ports A–D, color/distance sensor, tilt sensor |

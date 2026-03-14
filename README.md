# LegoControl
Control Lego motorised devices by Bluetooth in the browser

This will use the browser Bluetooth to control connected Lego devices.

The front end for the user will be a Blazor app which runs entirely locally on the users browser.

The app will store settings and used connections/sets/setups in the browser data. There will be an option for the user export the data to a JSON file, and then import into another browser.

## Solution Structure

```text
LegoControl.sln
src/
  LegoControl.Core/   # Device models, Bluetooth logic, settings (class library)
  LegoControl.UI/     # Blazor WebAssembly frontend
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

## Supported Lego sets

None yet.

## Lego sets in progress

- Audi RS Q e-tron (#42160)
- Boost (#17101)
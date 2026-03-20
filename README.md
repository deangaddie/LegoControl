# LegoControl

Control Lego motorised devices by Bluetooth in the browser.

Uses the Web Bluetooth API to connect to Lego hubs and control motors and read sensors. The app is a Blazor WebAssembly app that runs entirely locally in the user's browser — no backend server, no account required.

Settings, devices, and configurations are stored in browser storage. Data can be exported to a JSON file and imported into another browser. The app is a PWA — it can be installed from the browser and works offline.

## Features

- **Touch controls** — full-screen dual joystick interface (left stick = throttle, right stick = steering)
- **Keyboard controls** — customisable key bindings (arrow keys by default)
- **Gamepad / controller support** — configurable axis mapping, dead zone, and inversion
- **Visual programming** — Blockly-based drag-and-drop editor with Lego-specific blocks (drive, steer, stop, wait, loops, conditions, variables)
- **Motor configuration** — set roles, invert direction, link motors, configure speed limits and steering range
- **Steering calibration** — automatic homing sequence detects mechanical limits and sets centre
- **Real-time feedback** — live motor speed and position display during control and programming
- **Sensor support** — colour, distance, reflection, and ambient light sensors
- **Custom models** — add your own Lego set configurations beyond the built-in ones
- **Data portability** — JSON export/import for moving configs between browsers

## Getting Started

### Requirements

- A Chromium-based browser (Chrome or Edge) — Web Bluetooth is not supported in Firefox, Safari, or any iOS browser
- HTTPS is required for Web Bluetooth in production; `localhost` works without it during development
- A supported Lego set with a wireless hub (see [Supported Lego Sets](#supported-lego-sets))
- .NET SDK (for local development)

### Run Locally

```bash
# Build
dotnet build

# Run — opens in browser at https://localhost:5XXX
dotnet run --project src/LegoControl.UI
```

## How to Use

### 1. Add a Device

Go to **Devices → Add Device**. Choose a name for your device (e.g. "My Audi") and select the model that matches your Lego set. The device is saved with the default motor and sensor configuration for that model.

### 2. Configure the Device (optional)

Go to **Devices → Configure** for your device. Here you can:

- **Label motors** — give each port a meaningful name (e.g. "Left Drive", "Steering")
- **Set motor roles** — Drive, Steering, or Auxiliary
- **Invert direction** — if a motor runs backwards, tick Invert
- **Link motors** — pair two drive motors (e.g. front and rear) so they move as one
- **Set speed limits** — cap drive motors to a maximum speed
- **Steering range** — set the maximum left/right angles, or run the automatic homing sequence to detect mechanical limits
- **Test motors** — use the sliders to test each motor before connecting to a vehicle

#### Steering Homing

Click **Run Homing** on a steering motor to start the automatic calibration sequence:

1. The motor sweeps left at low power until it stalls against the mechanical limit
2. It then sweeps right to the opposite limit
3. The midpoint is calculated and the encoder is reset to zero
4. The motor returns to centre

After homing, the left/right extents are stored and used to keep the steering within its physical range.

### 3. Control the Device

Go to **Devices → Play** for your device. Click **Connect Bluetooth** — the browser will show a picker listing nearby Lego devices. Select yours.

Then choose a control mode:

#### Keyboard

Use arrow keys by default. The control area must have focus (click it first). Key bindings can be customised — click each binding and press the new key.

#### Gamepad

Plug in a USB controller or pair a Bluetooth gamepad via your OS. Once detected, click **Configure Gamepad** to assign axes:

1. Move your throttle axis — click **Assign** to record it
2. Move your steering axis — click **Assign** to record it
3. Set the dead zone (default 0.10) and invert axes if needed

#### Touch

Tap **Touch Controls** to enter full-screen mode with two on-screen joysticks:

- **Left joystick** — throttle (up = forward, down = reverse)
- **Right joystick** — steering (left/right)

Works on phones and tablets. The interface hides the browser UI for an immersive experience.

### 4. Program the Device

Go to **Devices → Program** for your device. The Blockly visual editor lets you build programs by dragging and connecting blocks:

| Block | Description |
| --- | --- |
| Drive | Set drive motor speed (-100 to 100) |
| Steer | Set steering motor position (degrees) |
| Stop | Stop all motors |
| Wait | Pause for a number of seconds |
| Repeat | Run blocks a fixed number of times |
| If / If-Else | Conditional branching |
| While | Loop while a condition is true |
| Wait Until | Wait for a condition to become true |
| Set Variable | Store a value in a named variable |
| Sensor Value | Read the current sensor reading |

Click **Run** to execute the program on the connected device. Click **Stop** to cancel. The program auto-saves and persists between sessions. Programs can also be exported and imported as JSON.

### 5. Manage Models

Go to **Models** to see all available Lego set configurations. Built-in models can be edited and reverted. Custom models can be added manually or imported from a JSON file. Models define the available motor ports, sensor ports, and their default configuration.

### 6. Settings

Go to **Settings** to customise the app appearance:

- **Dark / Light mode** — manual toggle or auto-detect system preference
- **Colour theme** — choose from preset themes or set custom Primary, Secondary, and Tertiary colours

### Data Export / Import

Device configurations, models, and programs can all be exported as JSON from their respective pages. Import JSON to restore a setup or copy it to another browser.

To do a full backup, export each device and model individually.

## Solution Structure

```text
LegoControl.sln
src/
  LegoControl.Core/     # Device models, Bluetooth logic, settings (class library, no UI dependencies)
  LegoControl.UI/       # Blazor WebAssembly frontend
  LegoControl.Tests/    # Unit tests
```

## Building and Running

```bash
# Build all projects
dotnet build

# Run the Blazor app
dotnet run --project src/LegoControl.UI

# Run tests
dotnet test
```

## Deployment

The app is a static Blazor WebAssembly site and can be hosted on any static host. It is configured for [Netlify](https://netlify.com) via [netlify.toml](netlify.toml).

### Deploy to Netlify (manual drag-and-drop)

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

Custom models can be added for any Lego hub that uses the Lego Wireless Protocol (LWP) over Bluetooth GATT. This includes most Lego Powered Up and Technic hubs.

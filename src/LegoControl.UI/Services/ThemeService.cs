using LegoControl.Core.Models;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;

namespace LegoControl.UI.Services;

public class ThemeService : IThemeService
{
    private const string StorageKey = "lc_theme";

    private readonly IJSRuntime _js;

    public ThemeSettings Settings { get; private set; } = new();
    public MudTheme Theme { get; private set; } = new();
    public bool IsDarkMode => Settings.IsDarkMode;
    public event Action? OnChanged;

    public IReadOnlyList<ThemePreset> Presets { get; } =
    [
        new("Default",   "#594ae2", "#ff4081", "#00bcd4"),
        new("Lego Red",  "#e63946", "#ffb703", "#457b9d"),
        new("Ocean",     "#0077b6", "#00b4d8", "#90e0ef"),
        new("Forest",    "#2d6a4f", "#74c69d", "#95d5b2"),
        new("Sunset",    "#f4813f", "#9b2335", "#f4a261"),
    ];

    public ThemeService(IJSRuntime js) => _js = js;

    public async Task InitializeAsync()
    {
        var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (json is not null)
        {
            try { Settings = JsonSerializer.Deserialize<ThemeSettings>(json) ?? new(); }
            catch { Settings = new(); }
        }

        if (Settings.AutoDarkMode)
        {
            await DetectSystemPreferenceAsync();
        }

        RebuildTheme();
    }

    public async Task SetPresetAsync(string presetName)
    {
        Settings.PresetName = presetName;
        await SaveAsync();
    }

    public async Task SetDarkModeAsync(bool isDark)
    {
        Settings.IsDarkMode = isDark;
        await SaveAsync();
    }

    public async Task SetCustomColorsAsync(string? primary, string? secondary, string? tertiary)
    {
        Settings.CustomPrimary = primary;
        Settings.CustomSecondary = secondary;
        Settings.CustomTertiary = tertiary;
        await SaveAsync();
    }

    public async Task SetAutoDarkModeAsync(bool auto)
    {
        Settings.AutoDarkMode = auto;
        if (auto)
        {
            await DetectSystemPreferenceAsync();
        }
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        RebuildTheme();
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(Settings));
        OnChanged?.Invoke();
    }

    private async Task DetectSystemPreferenceAsync()
    {
        try
        {
            var prefersDark = await _js.InvokeAsync<bool>("window.matchMedia('(prefers-color-scheme: dark)').matches");
            Settings.IsDarkMode = prefersDark;
        }
        catch
        {
            // Fallback to light mode if detection fails
            Settings.IsDarkMode = false;
        }
    }

    private void RebuildTheme()
    {
        var preset = Presets.FirstOrDefault(p => p.Name == Settings.PresetName) ?? Presets[0];
        var primary = Settings.CustomPrimary ?? preset.Primary;
        var secondary = Settings.CustomSecondary ?? preset.Secondary;
        var tertiary = Settings.CustomTertiary ?? preset.Tertiary;

        Theme = new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = primary,
                Secondary = secondary,
                Tertiary = tertiary,
                Success = "#4caf50",
                Error = "#f44336",
                Warning = "#ff9800",
                Info = "#2196f3",
                Surface = "#ffffff",
                Background = "#fafafa",
                AppbarBackground = primary,
                AppbarText = "#ffffff",
                DrawerBackground = "#ffffff",
                DrawerText = "#000000",
                TextPrimary = "#000000",
                TextSecondary = "#666666",
                ActionDefault = "#666666",
                ActionDisabled = "#cccccc",
                Divider = "#e0e0e0",
                LinesDefault = "#e0e0e0",
                LinesInputs = "#e0e0e0",
                TableLines = "#e0e0e0",
                TableStriped = "#f5f5f5",
                OverlayLight = "rgba(255,255,255,0.8)",
                OverlayDark = "rgba(0,0,0,0.4)",
            },
            PaletteDark = new PaletteDark
            {
                Primary = primary,
                Secondary = secondary,
                Tertiary = tertiary,
                Success = "#81c784",
                Error = "#ef5350",
                Warning = "#ffb74d",
                Info = "#64b5f6",
                Surface = "#1e1e1e",
                Background = "#121212",
                AppbarBackground = "#1e1e2e",
                AppbarText = "#ffffff",
                DrawerBackground = "#1e1e1e",
                DrawerText = "#ffffff",
                TextPrimary = "#ffffff",
                TextSecondary = "#b0b0b0",
                ActionDefault = "#b0b0b0",
                ActionDisabled = "#666666",
                Divider = "#333333",
                LinesDefault = "#333333",
                LinesInputs = "#333333",
                TableLines = "#333333",
                TableStriped = "#2a2a2a",
                OverlayLight = "rgba(255,255,255,0.1)",
                OverlayDark = "rgba(0,0,0,0.6)",
            },
        };
    }
}

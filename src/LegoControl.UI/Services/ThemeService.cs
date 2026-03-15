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
        new("Default",   "#594ae2", "#ff4081"),
        new("Lego Red",  "#e63946", "#ffb703"),
        new("Ocean",     "#0077b6", "#00b4d8"),
        new("Forest",    "#2d6a4f", "#74c69d"),
        new("Sunset",    "#f4813f", "#9b2335"),
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

    public async Task SetCustomColorsAsync(string? primary, string? secondary)
    {
        Settings.CustomPrimary = primary;
        Settings.CustomSecondary = secondary;
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        RebuildTheme();
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(Settings));
        OnChanged?.Invoke();
    }

    private void RebuildTheme()
    {
        var preset = Presets.FirstOrDefault(p => p.Name == Settings.PresetName) ?? Presets[0];
        var primary = Settings.CustomPrimary ?? preset.Primary;
        var secondary = Settings.CustomSecondary ?? preset.Secondary;

        Theme = new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = primary,
                Secondary = secondary,
                AppbarBackground = primary,
                AppbarText = "#ffffff",
            },
            PaletteDark = new PaletteDark
            {
                Primary = primary,
                Secondary = secondary,
                AppbarBackground = "#1e1e2e",
                AppbarText = "#ffffff",
            },
        };
    }
}

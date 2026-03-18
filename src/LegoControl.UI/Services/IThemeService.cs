using LegoControl.Core.Models;
using MudBlazor;

namespace LegoControl.UI.Services;

public record ThemePreset(string Name, string Primary, string Secondary, string Tertiary);

public interface IThemeService
{
    ThemeSettings Settings { get; }
    MudTheme Theme { get; }
    bool IsDarkMode { get; }
    IReadOnlyList<ThemePreset> Presets { get; }
    event Action? OnChanged;

    Task InitializeAsync();
    Task SetPresetAsync(string presetName);
    Task SetDarkModeAsync(bool isDark);
    Task SetCustomColorsAsync(string? primary, string? secondary, string? tertiary);
    Task SetAutoDarkModeAsync(bool auto);
}

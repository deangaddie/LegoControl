namespace LegoControl.Core.Models;

public class ThemeSettings
{
    public string PresetName { get; set; } = "Default";
    public bool IsDarkMode { get; set; } = false;
    public string? CustomPrimary { get; set; }
    public string? CustomSecondary { get; set; }
    public string? CustomTertiary { get; set; }
    public bool AutoDarkMode { get; set; } = true;
}

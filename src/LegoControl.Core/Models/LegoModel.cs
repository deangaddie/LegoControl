using System.Text.Json.Serialization;

namespace LegoControl.Core.Models;

public class LegoModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string SetNumber { get; set; } = "";
    public List<string> ConnectionInstructions { get; set; } = [];
    public DeviceConfig DefaultConfig { get; set; } = new();
    [JsonIgnore] public bool IsBuiltIn { get; set; }
    [JsonIgnore] public bool IsEdited { get; set; }
}

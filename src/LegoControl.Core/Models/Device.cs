namespace LegoControl.Core.Models;

public class Device
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string ModelId { get; set; }
    public DeviceConfig Config { get; set; } = new();
}

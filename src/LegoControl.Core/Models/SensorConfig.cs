namespace LegoControl.Core.Models;

public class SensorConfig
{
    public string PortId { get; set; } = "";
    public string Label { get; set; } = "";
    public SensorMode Mode { get; set; } = SensorMode.ColorAndDistance;
}

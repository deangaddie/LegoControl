namespace LegoControl.Core.Models;

public class DeviceConfig
{
    public List<MotorConfig> Motors { get; set; } = [];
    public List<SensorConfig> Sensors { get; set; } = [];
}

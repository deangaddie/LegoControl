using LegoControl.Core.Models;

namespace LegoControl.Core.Services;

public static class DeviceConfigFactory
{
    public static DeviceConfig CreateDefault(LegoSet set) => set switch
    {
        LegoSet.Boost => new DeviceConfig
        {
            Motors =
            [
                new() { PortId = "A", Label = "Motor A", Role = MotorRole.Drive },
                new() { PortId = "B", Label = "Motor B", Role = MotorRole.Drive },
                new() { PortId = "C", Label = "Port C",  Role = MotorRole.Auxiliary },
                new() { PortId = "D", Label = "Port D",  Role = MotorRole.Auxiliary },
            ],
            Sensors =
            [
                new() { PortId = "C_SENSOR", Label = "Color / Distance", Mode = SensorMode.ColorAndDistance },
                new() { PortId = "TILT",     Label = "Tilt Sensor" },
            ]
        },
        LegoSet.AudiRSQetron => new DeviceConfig
        {
            Motors =
            [
                new() { PortId = "A", Label = "Rear Drive Motor",  Role = MotorRole.Drive },
                new() { PortId = "B", Label = "Front Drive Motor", Role = MotorRole.Drive },
                new() { PortId = "D", Label = "Steering Motor",    Role = MotorRole.Steering },
            ],
            Sensors = []
        },
        _ => new DeviceConfig()
    };
}

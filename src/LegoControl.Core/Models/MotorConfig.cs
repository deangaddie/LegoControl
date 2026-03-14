namespace LegoControl.Core.Models;

public class MotorConfig
{
    public string PortId { get; set; } = "";
    public string Label { get; set; } = "";
    public MotorRole Role { get; set; } = MotorRole.Auxiliary;
    public bool InvertDirection { get; set; }

    // Drive / Auxiliary
    public int MinSpeed { get; set; } = 0;
    public int MaxSpeed { get; set; } = 100;
    public bool HomePositionSet { get; set; }

    // Steering
    public int SteeringMaxLeft { get; set; } = -90;
    public int SteeringMaxRight { get; set; } = 90;
    public bool SteeringCenterSet { get; set; }
    public bool HomingEnabled { get; set; }
}

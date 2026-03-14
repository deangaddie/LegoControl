namespace LegoControl.Core.Models;

public record GamepadBindings
{
    public int   ThrottleAxis   { get; init; } = 1;    // left stick Y (standard gamepad layout)
    public int   SteerAxis      { get; init; } = 2;    // right stick X
    public bool  InvertThrottle { get; init; } = true; // Y-axis up = negative on most controllers
    public bool  InvertSteer    { get; init; } = false;
    public float DeadZone       { get; init; } = 0.10f;
}

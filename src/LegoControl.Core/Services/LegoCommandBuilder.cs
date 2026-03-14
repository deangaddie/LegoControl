namespace LegoControl.Core.Services;

public static class LegoCommandBuilder
{
    private static byte PortByte(string portId) => portId switch
    {
        "A" => 0x00, "B" => 0x01, "C" => 0x02, "D" => 0x03,
        _ => throw new ArgumentException($"Unknown port: {portId}")
    };

    // LWP Port Output: Write Direct Mode Data (PWM power)
    // Range: -100 (full reverse) to 100 (full forward), 0 = coast/stop
    public static byte[] StartPower(string portId, int power)
    {
        int p = Math.Clamp(power, -100, 100);
        return [0x08, 0x00, 0x81, PortByte(portId), 0x11, 0x51, 0x00, (byte)(sbyte)p];
    }

    // LWP Port Output: Go to Absolute Position (degrees from zero/home)
    public static byte[] GoToAbsolutePosition(string portId, int degrees, int speed = 50, int maxPower = 100)
    {
        var pos = BitConverter.GetBytes(degrees); // int32 LE
        return [0x0E, 0x00, 0x81, PortByte(portId), 0x11, 0x0D,
                pos[0], pos[1], pos[2], pos[3],
                (byte)Math.Clamp(speed, 0, 100),
                (byte)Math.Clamp(maxPower, 0, 100),
                126,   // end state: HOLD
                0x00]; // profile: none
    }

    // LWP Port Input Format Setup (Single): subscribe to motor speed updates.
    // Mode 1 = SPEED (current speed as int8, -100 to 100). Delta = 1 minimum change.
    public static byte[] SubscribeMotorSpeed(string portId)
        => [0x0A, 0x00, 0x41, PortByte(portId), 0x01, 0x01, 0x00, 0x00, 0x00, 0x01];

    // LWP Port Input Format Setup (Single): subscribe to motor position updates.
    // Mode 2 = POS (relative position, int32 degrees). Delta = 1 degree minimum change.
    public static byte[] SubscribeMotorPosition(string portId)
        => [0x0A, 0x00, 0x41, PortByte(portId), 0x02, 0x01, 0x00, 0x00, 0x00, 0x01];

    // LWP Port Output: Write Direct Mode Data, mode 2 (POS) — resets the motor's
    // position counter to the given value at its current physical position.
    // Call with degrees=0 to mark the current position as the new zero/center.
    public static byte[] PresetEncoder(string portId, int degrees = 0)
    {
        var pos = BitConverter.GetBytes(degrees); // int32 LE
        return [0x0B, 0x00, 0x81, PortByte(portId), 0x11, 0x51, 0x02,
                pos[0], pos[1], pos[2], pos[3]];
    }

    // LWP Port Input Format Setup (Single): subscribe to sensor updates.
    // Boost Color/Distance Sensor (88007) modes:
    //   0 = COLOR  (color index 0-10, 1 byte)
    //   1 = PROX   (proximity distance 0-10, 1 byte)
    //   3 = REFLECT (reflection %, 1 byte)
    //   4 = AMBIL  (ambient light %, 1 byte)
    public static byte[] SubscribeSensor(string portId, int mode, int delta = 1)
        => [0x0A, 0x00, 0x41, PortByte(portId), (byte)mode, (byte)delta, 0x00, 0x00, 0x00, 0x01];
}

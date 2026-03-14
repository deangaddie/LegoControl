namespace LegoControl.Core.Services;

public class LegoHubService : ILegoHubService
{
    private readonly IBluetoothService _bt;
    private readonly Dictionary<string, int> _motorPositions = new();

    public IReadOnlyDictionary<string, int> MotorPositions => _motorPositions;
    public event Action<string, int>? MotorPositionChanged;

    public LegoHubService(IBluetoothService bt)
    {
        _bt = bt;
        _bt.NotificationReceived += OnNotificationReceived;
    }

    public Task SubscribeMotorPositionAsync(string portId)
        => _bt.SendCommandAsync(LegoCommandBuilder.SubscribeMotorPosition(portId));

    public async Task<SteeringHomingResult> RunSteeringHomingAsync(string portId, CancellationToken ct = default)
    {
        try
        {
            await SubscribeMotorPositionAsync(portId);
            await Task.Delay(150, ct); // Let subscription settle

            // Sweep left (negative power) until the motor stalls against the end stop
            await _bt.SendCommandAsync(LegoCommandBuilder.StartPower(portId, -30));
            int leftExtent = await WaitForStallAsync(portId, ct);
            await _bt.SendCommandAsync(LegoCommandBuilder.StartPower(portId, 0));
            await Task.Delay(200, ct);

            // Sweep right (positive power) until the motor stalls
            await _bt.SendCommandAsync(LegoCommandBuilder.StartPower(portId, 30));
            int rightExtent = await WaitForStallAsync(portId, ct);
            await _bt.SendCommandAsync(LegoCommandBuilder.StartPower(portId, 0));
            await Task.Delay(200, ct);

            // Calculate mechanical centre and drive to it
            int center = (leftExtent + rightExtent) / 2;
            await _bt.SendCommandAsync(LegoCommandBuilder.GoToAbsolutePosition(portId, center, speed: 50));
            await Task.Delay(600, ct); // Allow time to reach centre

            // Reset encoder so centre = 0°
            await _bt.SendCommandAsync(LegoCommandBuilder.PresetEncoder(portId, 0));
            _motorPositions[portId] = 0;

            // Return extents relative to the new zero
            return new SteeringHomingResult(
                Success: true,
                LeftExtent: leftExtent - center,
                RightExtent: rightExtent - center,
                Center: 0);
        }
        catch (OperationCanceledException)
        {
            await _bt.SendCommandAsync(LegoCommandBuilder.StartPower(portId, 0));
            return new SteeringHomingResult(false, 0, 0, 0, "Homing cancelled");
        }
        catch (Exception ex)
        {
            try { await _bt.SendCommandAsync(LegoCommandBuilder.StartPower(portId, 0)); } catch { }
            return new SteeringHomingResult(false, 0, 0, 0, ex.Message);
        }
    }

    // Polls the last-known position every 100 ms; returns when it hasn't moved
    // more than 2 degrees across 4 consecutive checks (~400 ms stable).
    private async Task<int> WaitForStallAsync(string portId, CancellationToken ct)
    {
        int lastPos = _motorPositions.GetValueOrDefault(portId, 0);
        int stableCount = 0;
        while (stableCount < 4)
        {
            await Task.Delay(100, ct);
            int pos = _motorPositions.GetValueOrDefault(portId, 0);
            stableCount = Math.Abs(pos - lastPos) <= 2 ? stableCount + 1 : 0;
            lastPos = pos;
        }
        return lastPos;
    }

    // Parse LWP Port Value Single (0x45) messages to extract motor position.
    // Format: [length, 0x00, 0x45, port_byte, b0, b1, b2, b3]  (int32 LE position)
    private void OnNotificationReceived(byte[] bytes)
    {
        if (bytes.Length < 8 || bytes[2] != 0x45) return;

        string portId = bytes[3] switch
        {
            0x00 => "A", 0x01 => "B", 0x02 => "C", 0x03 => "D",
            _ => ""
        };
        if (portId == "") return;

        int pos = BitConverter.ToInt32(bytes, 4);
        _motorPositions[portId] = pos;
        MotorPositionChanged?.Invoke(portId, pos);
    }
}

namespace LegoControl.Core.Services;

public record SteeringHomingResult(
    bool Success,
    int LeftExtent,
    int RightExtent,
    int Center,
    string? Error = null);

public interface ILegoHubService
{
    /// <summary>Last known motor positions keyed by port ID (e.g. "A").</summary>
    IReadOnlyDictionary<string, int> MotorPositions { get; }

    /// <summary>Fired whenever a motor position notification arrives from the hub.</summary>
    event Action<string, int>? MotorPositionChanged;

    /// <summary>
    /// Sends a subscription command so the hub sends position updates for the given port.
    /// Call once per session after connecting.
    /// </summary>
    Task SubscribeMotorPositionAsync(string portId);

    /// <summary>
    /// Runs a full steering homing sequence:
    /// 1. Sweep left until stall → record left extent
    /// 2. Sweep right until stall → record right extent
    /// 3. Move to mechanical centre
    /// 4. Preset encoder to 0 at that position
    /// Returns measured extents (relative to the new zero centre).
    /// </summary>
    Task<SteeringHomingResult> RunSteeringHomingAsync(string portId, CancellationToken ct = default);
}

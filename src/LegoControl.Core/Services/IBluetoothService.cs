namespace LegoControl.Core.Services;

public interface IBluetoothService
{
    bool IsConnected { get; }
    string? ConnectedDeviceName { get; }
    event Action? ConnectionStateChanged;
    event Action<byte[]>? NotificationReceived;
    Task<bool> IsSupportedAsync();
    Task ConnectAsync();
    Task DisconnectAsync();
    Task SendCommandAsync(byte[] data);
}

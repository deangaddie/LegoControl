using LegoControl.Core.Services;
using Microsoft.JSInterop;

namespace LegoControl.UI.Services;

public class WebBluetoothService(IJSRuntime js) : IBluetoothService, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<WebBluetoothService>? _dotNetRef;

    public bool IsConnected { get; private set; }
    public string? ConnectedDeviceName { get; private set; }
    public event Action? ConnectionStateChanged;
    public event Action<byte[]>? NotificationReceived;

    public async Task<bool> IsSupportedAsync()
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("isSupported");
    }

    public async Task ConnectAsync()
    {
        var module = await GetModuleAsync();
        _dotNetRef ??= DotNetObjectReference.Create(this);
        var name = await module.InvokeAsync<string>("connect", _dotNetRef);
        IsConnected = true;
        ConnectedDeviceName = name;
        ConnectionStateChanged?.Invoke();
    }

    public async Task DisconnectAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("disconnect");
        IsConnected = false;
        ConnectedDeviceName = null;
        ConnectionStateChanged?.Invoke();
    }

    public async Task SendCommandAsync(byte[] data)
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("sendCommand", data);
    }

    [JSInvokable]
    public void OnDisconnected()
    {
        IsConnected = false;
        ConnectedDeviceName = null;
        ConnectionStateChanged?.Invoke();
    }

    [JSInvokable]
    public void OnNotification(int[] bytes)
    {
        NotificationReceived?.Invoke(bytes.Select(b => (byte)b).ToArray());
    }

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./js/lego-bluetooth.js");
        return _module;
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        if (_module is not null)
            await _module.DisposeAsync();
    }
}

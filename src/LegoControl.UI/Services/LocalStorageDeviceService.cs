using System.Text.Json;
using LegoControl.Core.Models;
using LegoControl.Core.Services;
using Microsoft.JSInterop;

namespace LegoControl.UI.Services;

public class LocalStorageDeviceService(IJSRuntime js) : IDeviceService
{
    private const string StorageKey = "lc_devices";
    private List<Device> _devices = [];

    public IReadOnlyList<Device> Devices => _devices.AsReadOnly();

    public async Task InitializeAsync()
    {
        var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (json is not null)
            _devices = JsonSerializer.Deserialize<List<Device>>(json) ?? [];
    }

    public async Task AddAsync(Device device)
    {
        if (device.Config.Motors.Count == 0)
            device.Config = DeviceConfigFactory.CreateDefault(device.Set);
        _devices.Add(device);
        await SaveAsync();
    }

    public async Task UpdateAsync(Guid id, string name, LegoSet set)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device is not null)
        {
            device.Name = name;
            device.Set = set;
            await SaveAsync();
        }
    }

    public async Task RemoveAsync(Guid id)
    {
        _devices.RemoveAll(d => d.Id == id);
        await SaveAsync();
    }

    public async Task UpdateConfigAsync(Guid id, DeviceConfig config)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device is not null)
        {
            device.Config = config;
            await SaveAsync();
        }
    }

    private async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_devices);
        await js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }
}

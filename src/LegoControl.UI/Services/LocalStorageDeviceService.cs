using System.Text.Json;
using LegoControl.Core.Models;
using LegoControl.Core.Services;
using Microsoft.JSInterop;

namespace LegoControl.UI.Services;

public class LocalStorageDeviceService(IJSRuntime js, ILegoModelService modelService) : IDeviceService
{
    private const string StorageKey = "lc_devices";
    private List<Device> _devices = [];

    public IReadOnlyList<Device> Devices => _devices.AsReadOnly();

    public async Task InitializeAsync()
    {
        var json = await js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
        if (json is not null)
        {
            try
            {
                _devices = JsonSerializer.Deserialize<List<Device>>(json) ?? [];
            }
            catch
            {
                // Old data format incompatible (e.g. contained LegoSet enum); start fresh.
                await js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
            }
        }
    }

    public async Task AddAsync(Device device)
    {
        if (device.Config.Motors.Count == 0)
        {
            var model = modelService.Models.FirstOrDefault(m => m.Id == device.ModelId);
            if (model is not null)
                device.Config = DeepCopy(model.DefaultConfig);
        }
        _devices.Add(device);
        await SaveAsync();
    }

    public async Task UpdateAsync(Guid id, string name, string modelId)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device is not null)
        {
            device.Name = name;
            device.ModelId = modelId;
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

    private static DeviceConfig DeepCopy(DeviceConfig config)
    {
        var json = JsonSerializer.Serialize(config);
        return JsonSerializer.Deserialize<DeviceConfig>(json)!;
    }
}

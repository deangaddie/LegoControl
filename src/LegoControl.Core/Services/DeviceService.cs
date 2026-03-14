using LegoControl.Core.Models;

namespace LegoControl.Core.Services;

public class DeviceService : IDeviceService
{
    private readonly List<Device> _devices = [];

    public IReadOnlyList<Device> Devices => _devices.AsReadOnly();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task AddAsync(Device device)
    {
        _devices.Add(device);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Guid id, string name, string modelId)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device is not null)
        {
            device.Name = name;
            device.ModelId = modelId;
        }
        return Task.CompletedTask;
    }

    public Task RemoveAsync(Guid id)
    {
        _devices.RemoveAll(d => d.Id == id);
        return Task.CompletedTask;
    }

    public Task UpdateConfigAsync(Guid id, DeviceConfig config)
    {
        var device = _devices.FirstOrDefault(d => d.Id == id);
        if (device is not null)
            device.Config = config;
        return Task.CompletedTask;
    }
}

using LegoControl.Core.Models;

namespace LegoControl.Core.Services;

public interface IDeviceService
{
    IReadOnlyList<Device> Devices { get; }
    Task InitializeAsync();
    Task AddAsync(Device device);
    Task UpdateAsync(Guid id, string name, LegoSet set);
    Task RemoveAsync(Guid id);
    Task UpdateConfigAsync(Guid id, DeviceConfig config);
}

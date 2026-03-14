using FluentAssertions;
using LegoControl.Core.Models;
using LegoControl.Core.Services;

namespace LegoControl.Tests.Services;

public class DeviceServiceTests
{
    private readonly DeviceService _service = new();

    [Fact]
    public async Task InitializeAsync_CompletesSuccessfully()
    {
        // Act
        await _service.InitializeAsync();

        // Assert - should not throw
    }

    [Fact]
    public async Task Devices_ReturnsReadOnlyList()
    {
        // Act
        var devices = _service.Devices;

        // Assert
        devices.Should().BeEmpty();
        devices.Should().BeAssignableTo<IReadOnlyList<Device>>();
    }

    [Fact]
    public async Task AddAsync_AddsDevice()
    {
        // Arrange
        var device = new Device
        {
            Name = "Test Device",
            ModelId = "boost-17101",
            Config = new DeviceConfig()
        };

        // Act
        await _service.AddAsync(device);

        // Assert
        _service.Devices.Should().HaveCount(1);
        _service.Devices[0].Name.Should().Be("Test Device");
        _service.Devices[0].ModelId.Should().Be("boost-17101");
    }

    [Fact]
    public async Task AddAsync_WithExistingConfig_KeepsConfig()
    {
        // Arrange
        var customConfig = new DeviceConfig
        {
            Motors = [new MotorConfig { PortId = "A", Label = "Custom Motor" }],
            Sensors = [new SensorConfig { PortId = "S1", Label = "Custom Sensor" }]
        };
        var device = new Device
        {
            Name = "Test Device",
            ModelId = "boost-17101",
            Config = customConfig
        };

        // Act
        await _service.AddAsync(device);

        // Assert
        _service.Devices.Should().HaveCount(1);
        var addedDevice = _service.Devices[0];
        addedDevice.Config.Should().BeSameAs(customConfig);
        addedDevice.Config.Motors.Should().HaveCount(1);
        addedDevice.Config.Sensors.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ExistingDevice_UpdatesNameAndModelId()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            ModelId = "boost-17101"
        };
        await _service.AddAsync(device);
        var deviceId = device.Id;

        // Act
        await _service.UpdateAsync(deviceId, "New Name", "audi-42160");

        // Assert
        var updatedDevice = _service.Devices.Single();
        updatedDevice.Id.Should().Be(deviceId);
        updatedDevice.Name.Should().Be("New Name");
        updatedDevice.ModelId.Should().Be("audi-42160");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentDevice_DoesNothing()
    {
        // Arrange
        var device = new Device { Name = "Test", ModelId = "boost-17101" };
        await _service.AddAsync(device);

        // Act
        await _service.UpdateAsync(Guid.NewGuid(), "New Name", "audi-42160");

        // Assert
        var unchangedDevice = _service.Devices.Single();
        unchangedDevice.Name.Should().Be("Test");
        unchangedDevice.ModelId.Should().Be("boost-17101");
    }

    [Fact]
    public async Task RemoveAsync_ExistingDevice_RemovesDevice()
    {
        // Arrange
        var device = new Device { Name = "Test", ModelId = "boost-17101" };
        await _service.AddAsync(device);
        var deviceId = device.Id;

        // Act
        await _service.RemoveAsync(deviceId);

        // Assert
        _service.Devices.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveAsync_NonExistentDevice_DoesNothing()
    {
        // Arrange
        var device = new Device { Name = "Test", ModelId = "boost-17101" };
        await _service.AddAsync(device);

        // Act
        await _service.RemoveAsync(Guid.NewGuid());

        // Assert
        _service.Devices.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateConfigAsync_ExistingDevice_UpdatesConfig()
    {
        // Arrange
        var device = new Device { Name = "Test", ModelId = "boost-17101" };
        await _service.AddAsync(device);
        var deviceId = device.Id;

        var newConfig = new DeviceConfig
        {
            Motors = [new MotorConfig { PortId = "X", Label = "New Motor" }]
        };

        // Act
        await _service.UpdateConfigAsync(deviceId, newConfig);

        // Assert
        var updatedDevice = _service.Devices.Single();
        updatedDevice.Config.Should().BeSameAs(newConfig);
    }

    [Fact]
    public async Task UpdateConfigAsync_NonExistentDevice_DoesNothing()
    {
        // Arrange
        var device = new Device { Name = "Test", ModelId = "boost-17101" };
        await _service.AddAsync(device);

        var originalConfig = device.Config;
        var newConfig = new DeviceConfig();

        // Act
        await _service.UpdateConfigAsync(Guid.NewGuid(), newConfig);

        // Assert
        var unchangedDevice = _service.Devices.Single();
        unchangedDevice.Config.Should().BeSameAs(originalConfig);
    }
}

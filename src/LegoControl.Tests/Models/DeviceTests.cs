using FluentAssertions;
using LegoControl.Core.Models;

namespace LegoControl.Tests.Models;

public class DeviceTests
{
    [Fact]
    public void Constructor_GeneratesUniqueId()
    {
        // Act
        var device1 = new Device { Name = "Device 1", ModelId = "boost-17101" };
        var device2 = new Device { Name = "Device 2", ModelId = "audi-42160" };

        // Assert
        device1.Id.Should().NotBeEmpty();
        device2.Id.Should().NotBeEmpty();
        device1.Id.Should().NotBe(device2.Id);
    }

    [Fact]
    public void Constructor_InitializesConfig()
    {
        // Act
        var device = new Device { Name = "Test", ModelId = "boost-17101" };

        // Assert
        device.Config.Should().NotBeNull();
        device.Config.Motors.Should().NotBeNull();
        device.Config.Sensors.Should().NotBeNull();
    }

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var config = new DeviceConfig();

        // Act
        var device = new Device
        {
            Id = id,
            Name = "Test Device",
            ModelId = "boost-17101",
            Config = config
        };

        // Assert
        device.Id.Should().Be(id);
        device.Name.Should().Be("Test Device");
        device.ModelId.Should().Be("boost-17101");
        device.Config.Should().BeSameAs(config);
    }
}

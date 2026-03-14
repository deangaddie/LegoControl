using FluentAssertions;
using LegoControl.Core.Models;

namespace LegoControl.Tests.Models;

public class DeviceTests
{
    [Fact]
    public void Constructor_GeneratesUniqueId()
    {
        // Act
        var device1 = new Device { Name = "Device 1", Set = LegoSet.Boost };
        var device2 = new Device { Name = "Device 2", Set = LegoSet.AudiRSQetron };

        // Assert
        device1.Id.Should().NotBeEmpty();
        device2.Id.Should().NotBeEmpty();
        device1.Id.Should().NotBe(device2.Id);
    }

    [Fact]
    public void Constructor_InitializesConfig()
    {
        // Act
        var device = new Device { Name = "Test", Set = LegoSet.Boost };

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
            Set = LegoSet.Boost,
            Config = config
        };

        // Assert
        device.Id.Should().Be(id);
        device.Name.Should().Be("Test Device");
        device.Set.Should().Be(LegoSet.Boost);
        device.Config.Should().BeSameAs(config);
    }
}
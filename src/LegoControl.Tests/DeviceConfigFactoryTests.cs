using FluentAssertions;
using LegoControl.Core.Models;
using LegoControl.Core.Services;

namespace LegoControl.Tests.Services;

public class DeviceConfigFactoryTests
{
    [Fact]
    public void CreateDefault_Boost_ReturnsCorrectConfig()
    {
        // Act
        var result = DeviceConfigFactory.CreateDefault(LegoSet.Boost);

        // Assert
        result.Should().NotBeNull();
        result.Motors.Should().HaveCount(4);

        // Motor A
        result.Motors[0].PortId.Should().Be("A");
        result.Motors[0].Label.Should().Be("Motor A");
        result.Motors[0].Role.Should().Be(MotorRole.Drive);

        // Motor B
        result.Motors[1].PortId.Should().Be("B");
        result.Motors[1].Label.Should().Be("Motor B");
        result.Motors[1].Role.Should().Be(MotorRole.Drive);

        // Port C
        result.Motors[2].PortId.Should().Be("C");
        result.Motors[2].Label.Should().Be("Port C");
        result.Motors[2].Role.Should().Be(MotorRole.Auxiliary);

        // Port D
        result.Motors[3].PortId.Should().Be("D");
        result.Motors[3].Label.Should().Be("Port D");
        result.Motors[3].Role.Should().Be(MotorRole.Auxiliary);

        // Sensors
        result.Sensors.Should().HaveCount(2);
        result.Sensors[0].PortId.Should().Be("C_SENSOR");
        result.Sensors[0].Label.Should().Be("Color / Distance");
        result.Sensors[0].Mode.Should().Be(SensorMode.ColorAndDistance);

        result.Sensors[1].PortId.Should().Be("TILT");
        result.Sensors[1].Label.Should().Be("Tilt Sensor");
        result.Sensors[1].Mode.Should().Be(SensorMode.ColorAndDistance); // default
    }

    [Fact]
    public void CreateDefault_AudiRSQetron_ReturnsCorrectConfig()
    {
        // Act
        var result = DeviceConfigFactory.CreateDefault(LegoSet.AudiRSQetron);

        // Assert
        result.Should().NotBeNull();
        result.Motors.Should().HaveCount(3);

        // Rear Drive Motor
        result.Motors[0].PortId.Should().Be("A");
        result.Motors[0].Label.Should().Be("Rear Drive Motor");
        result.Motors[0].Role.Should().Be(MotorRole.Drive);

        // Front Drive Motor
        result.Motors[1].PortId.Should().Be("B");
        result.Motors[1].Label.Should().Be("Front Drive Motor");
        result.Motors[1].Role.Should().Be(MotorRole.Drive);

        // Steering Motor
        result.Motors[2].PortId.Should().Be("D");
        result.Motors[2].Label.Should().Be("Steering Motor");
        result.Motors[2].Role.Should().Be(MotorRole.Steering);

        // No sensors
        result.Sensors.Should().BeEmpty();
    }

    [Fact]
    public void CreateDefault_UnknownSet_ReturnsEmptyConfig()
    {
        // Act
        var result = DeviceConfigFactory.CreateDefault((LegoSet)999);

        // Assert
        result.Should().NotBeNull();
        result.Motors.Should().BeEmpty();
        result.Sensors.Should().BeEmpty();
    }
}
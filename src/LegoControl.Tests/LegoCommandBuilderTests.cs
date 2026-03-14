using FluentAssertions;
using LegoControl.Core.Services;
using System.Reflection;

namespace LegoControl.Tests.Services;

public class LegoCommandBuilderTests
{
    [Theory]
    [InlineData("A", 50, new byte[] { 0x08, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 50 })]
    [InlineData("B", -25, new byte[] { 0x08, 0x00, 0x81, 0x01, 0x11, 0x51, 0x00, 231 })] // 231 = (sbyte)-25
    [InlineData("C", 100, new byte[] { 0x08, 0x00, 0x81, 0x02, 0x11, 0x51, 0x00, 100 })]
    [InlineData("D", -100, new byte[] { 0x08, 0x00, 0x81, 0x03, 0x11, 0x51, 0x00, 156 })] // 156 = (sbyte)-100
    public void StartPower_ValidInputs_ReturnsCorrectByteArray(string portId, int power, byte[] expected)
    {
        // Act
        var result = LegoCommandBuilder.StartPower(portId, power);

        // Assert
        result.Should().Equal(expected);
    }

    [Theory]
    [InlineData(150, new byte[] { 0x08, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 100 })] // clamped to 100
    [InlineData(-150, new byte[] { 0x08, 0x00, 0x81, 0x00, 0x11, 0x51, 0x00, 156 })] // clamped to -100
    public void StartPower_PowerOutOfRange_ClampsToValidRange(int power, byte[] expected)
    {
        // Act
        var result = LegoCommandBuilder.StartPower("A", power);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void StartPower_InvalidPort_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => LegoCommandBuilder.StartPower("X", 50);
        action.Should().Throw<ArgumentException>().WithMessage("Unknown port: X");
    }

    [Fact]
    public void GoToAbsolutePosition_DefaultParameters_ReturnsCorrectByteArray()
    {
        // Act
        var result = LegoCommandBuilder.GoToAbsolutePosition("A", 180);

        // Assert
        result.Should().HaveCount(14);
        result[0].Should().Be(0x0E);
        result[1].Should().Be(0x00);
        result[2].Should().Be(0x81);
        result[3].Should().Be(0x00); // port A
        result[4].Should().Be(0x11);
        result[5].Should().Be(0x0D);
        // degrees: 180 = 0xB4 0x00 0x00 0x00 (little endian)
        result[6].Should().Be(0xB4);
        result[7].Should().Be(0x00);
        result[8].Should().Be(0x00);
        result[9].Should().Be(0x00);
        result[10].Should().Be(50); // default speed
        result[11].Should().Be(100); // default maxPower
        result[12].Should().Be(126); // end state HOLD
        result[13].Should().Be(0x00); // profile none
    }

    [Theory]
    [InlineData("B", 120, 80, new byte[] { 0x0E, 0x00, 0x81, 0x01, 0x11, 0x0D, 0x78, 0x00, 0x00, 0x00, 80, 100, 126, 0x00 })]
    public void GoToAbsolutePosition_CustomParameters_ReturnsCorrectByteArray(string portId, int degrees, int speed, byte[] expected)
    {
        // Act
        var result = LegoCommandBuilder.GoToAbsolutePosition(portId, degrees, speed);

        // Assert
        result.Should().Equal(expected);
    }

    [Theory]
    [InlineData(-10, 100, new byte[] { 0x0E, 0x00, 0x81, 0x00, 0x11, 0x0D, 0x32, 0x00, 0x00, 0x00, 0, 100, 126, 0x00 })] // speed clamped to 0
    [InlineData(50, 150, new byte[] { 0x0E, 0x00, 0x81, 0x00, 0x11, 0x0D, 0x32, 0x00, 0x00, 0x00, 50, 100, 126, 0x00 })] // maxPower clamped to 100
    public void GoToAbsolutePosition_ParametersOutOfRange_ClampsToValidRange(int speed, int maxPower, byte[] expected)
    {
        // Act
        var result = LegoCommandBuilder.GoToAbsolutePosition("A", 50, speed, maxPower);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void GoToAbsolutePosition_InvalidPort_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => LegoCommandBuilder.GoToAbsolutePosition("X", 180);
        action.Should().Throw<ArgumentException>().WithMessage("Unknown port: X");
    }

    [Theory]
    [InlineData("A", new byte[] { 0x0A, 0x00, 0x41, 0x00, 0x02, 0x01, 0x00, 0x00, 0x00, 0x01 })]
    [InlineData("B", new byte[] { 0x0A, 0x00, 0x41, 0x01, 0x02, 0x01, 0x00, 0x00, 0x00, 0x01 })]
    public void SubscribeMotorPosition_ValidPort_ReturnsCorrectByteArray(string portId, byte[] expected)
    {
        // Act
        var result = LegoCommandBuilder.SubscribeMotorPosition(portId);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void SubscribeMotorPosition_InvalidPort_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => LegoCommandBuilder.SubscribeMotorPosition("X");
        action.Should().Throw<ArgumentException>().WithMessage("Unknown port: X");
    }

    [Fact]
    public void PresetEncoder_DefaultDegrees_ReturnsCorrectByteArray()
    {
        // Act
        var result = LegoCommandBuilder.PresetEncoder("A");

        // Assert
        result.Should().Equal(new byte[] { 0x0B, 0x00, 0x81, 0x00, 0x11, 0x51, 0x02, 0x00, 0x00, 0x00, 0x00 });
    }

    [Fact]
    public void PresetEncoder_CustomDegrees_ReturnsCorrectByteArray()
    {
        // Act
        var result = LegoCommandBuilder.PresetEncoder("B", 90);

        // Assert
        result.Should().Equal(new byte[] { 0x0B, 0x00, 0x81, 0x01, 0x11, 0x51, 0x02, 0x5A, 0x00, 0x00, 0x00 }); // 90 = 0x5A 0x00 0x00 0x00
    }

    [Fact]
    public void PresetEncoder_InvalidPort_ThrowsArgumentException()
    {
        // Act & Assert
        var action = () => LegoCommandBuilder.PresetEncoder("X");
        action.Should().Throw<ArgumentException>().WithMessage("Unknown port: X");
    }
}
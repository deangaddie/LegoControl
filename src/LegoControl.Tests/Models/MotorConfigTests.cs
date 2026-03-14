using System.Text.Json;
using FluentAssertions;
using LegoControl.Core.Models;

namespace LegoControl.Tests.Models;

public class MotorConfigTests
{
    [Fact]
    public void LinkedPortId_DefaultsToNull()
    {
        var motor = new MotorConfig();
        motor.LinkedPortId.Should().BeNull();
    }

    [Fact]
    public void InvertLink_DefaultsToFalse()
    {
        var motor = new MotorConfig();
        motor.InvertLink.Should().BeFalse();
    }

    [Fact]
    public void LinkedPortId_RoundTripsViaJson()
    {
        var motor = new MotorConfig { PortId = "A", LinkedPortId = "B", InvertLink = true };

        var json = JsonSerializer.Serialize(motor);
        var result = JsonSerializer.Deserialize<MotorConfig>(json)!;

        result.LinkedPortId.Should().Be("B");
        result.InvertLink.Should().BeTrue();
    }

    [Fact]
    public void OldJson_WithoutLinkFields_DeserializesToDefaults()
    {
        var json = """{"portId":"A","label":"Motor A","role":"Drive","invertDirection":false}""";
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
        };

        var result = JsonSerializer.Deserialize<MotorConfig>(json, options)!;

        result.LinkedPortId.Should().BeNull();
        result.InvertLink.Should().BeFalse();
    }
}

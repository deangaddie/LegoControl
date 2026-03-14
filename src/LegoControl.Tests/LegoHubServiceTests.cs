using FluentAssertions;
using LegoControl.Core.Services;
using Moq;

namespace LegoControl.Tests.Services;

public class LegoHubServiceTests
{
    private readonly Mock<IBluetoothService> _mockBt = new();
    private readonly LegoHubService _service;

    public LegoHubServiceTests()
    {
        _service = new LegoHubService(_mockBt.Object);
    }

    private void FireNotification(byte[] bytes)
        => _mockBt.Raise(bt => bt.NotificationReceived += null, bytes);

    // ---- MotorPositions ----

    [Fact]
    public void MotorPositions_InitiallyEmpty()
    {
        _service.MotorPositions.Should().BeEmpty();
    }

    // ---- OnNotificationReceived ----

    [Theory]
    [InlineData(0x00, "A")]
    [InlineData(0x01, "B")]
    [InlineData(0x02, "C")]
    [InlineData(0x03, "D")]
    public void OnNotificationReceived_ValidMessage_UpdatesCorrectPort(byte portByte, string expectedPortId)
    {
        // 100 in little-endian int32
        var bytes = new byte[] { 0x08, 0x00, 0x45, portByte, 100, 0x00, 0x00, 0x00 };

        FireNotification(bytes);

        _service.MotorPositions.Should().ContainKey(expectedPortId).WhoseValue.Should().Be(100);
    }

    [Fact]
    public void OnNotificationReceived_ValidMessage_FiresMotorPositionChangedEvent()
    {
        string? firedPort = null;
        int? firedPos = null;
        _service.MotorPositionChanged += (port, pos) => { firedPort = port; firedPos = pos; };

        FireNotification(new byte[] { 0x08, 0x00, 0x45, 0x00, 50, 0x00, 0x00, 0x00 });

        firedPort.Should().Be("A");
        firedPos.Should().Be(50);
    }

    [Fact]
    public void OnNotificationReceived_NegativePosition_ParsedCorrectly()
    {
        // -1 in little-endian int32: 0xFF 0xFF 0xFF 0xFF
        FireNotification(new byte[] { 0x08, 0x00, 0x45, 0x00, 0xFF, 0xFF, 0xFF, 0xFF });

        _service.MotorPositions["A"].Should().Be(-1);
    }

    [Fact]
    public void OnNotificationReceived_TooShort_Ignored()
    {
        FireNotification(new byte[] { 0x07, 0x00, 0x45, 0x00, 100, 0x00, 0x00 }); // 7 bytes

        _service.MotorPositions.Should().BeEmpty();
    }

    [Fact]
    public void OnNotificationReceived_WrongMessageType_Ignored()
    {
        FireNotification(new byte[] { 0x08, 0x00, 0x44, 0x00, 100, 0x00, 0x00, 0x00 }); // 0x44, not 0x45

        _service.MotorPositions.Should().BeEmpty();
    }

    [Fact]
    public void OnNotificationReceived_UnknownPortByte_Ignored()
    {
        FireNotification(new byte[] { 0x08, 0x00, 0x45, 0x04, 100, 0x00, 0x00, 0x00 }); // port 0x04

        _service.MotorPositions.Should().BeEmpty();
    }

    // ---- MotorSpeeds ----

    [Fact]
    public void MotorSpeeds_InitiallyEmpty()
    {
        _service.MotorSpeeds.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0x00, "A")]
    [InlineData(0x01, "B")]
    [InlineData(0x02, "C")]
    [InlineData(0x03, "D")]
    public void OnNotificationReceived_SpeedMessage_UpdatesCorrectPort(byte portByte, string expectedPortId)
    {
        // 5-byte speed notification: [0x05, 0x00, 0x45, port, speed]
        FireNotification(new byte[] { 0x05, 0x00, 0x45, portByte, 75 });

        _service.MotorSpeeds.Should().ContainKey(expectedPortId).WhoseValue.Should().Be(75);
    }

    [Fact]
    public void OnNotificationReceived_SpeedMessage_FiresMotorSpeedChangedEvent()
    {
        string? firedPort = null;
        int? firedSpeed = null;
        _service.MotorSpeedChanged += (port, speed) => { firedPort = port; firedSpeed = speed; };

        FireNotification(new byte[] { 0x05, 0x00, 0x45, 0x00, 60 });

        firedPort.Should().Be("A");
        firedSpeed.Should().Be(60);
    }

    [Fact]
    public void OnNotificationReceived_NegativeSpeed_ParsedCorrectly()
    {
        // -50 as sbyte = 206 as byte
        byte negFifty = unchecked((byte)-50);
        FireNotification(new byte[] { 0x05, 0x00, 0x45, 0x00, negFifty });

        _service.MotorSpeeds["A"].Should().Be(-50);
    }

    [Fact]
    public void OnNotificationReceived_SpeedMessage_DoesNotUpdatePositions()
    {
        FireNotification(new byte[] { 0x05, 0x00, 0x45, 0x00, 75 });

        _service.MotorPositions.Should().BeEmpty();
    }

    [Fact]
    public void OnNotificationReceived_PositionMessage_DoesNotUpdateSpeeds()
    {
        FireNotification(new byte[] { 0x08, 0x00, 0x45, 0x00, 100, 0x00, 0x00, 0x00 });

        _service.MotorSpeeds.Should().BeEmpty();
    }

    [Fact]
    public void OnNotificationReceived_SpeedThenPosition_BothTrackedSeparately()
    {
        FireNotification(new byte[] { 0x05, 0x00, 0x45, 0x00, 50 });    // speed = 50
        FireNotification(new byte[] { 0x08, 0x00, 0x45, 0x00, 90, 0x00, 0x00, 0x00 }); // pos = 90

        _service.MotorSpeeds["A"].Should().Be(50);
        _service.MotorPositions["A"].Should().Be(90);
    }

    // ---- SubscribeMotorSpeedAsync ----

    [Fact]
    public async Task SubscribeMotorSpeedAsync_SendsCorrectCommand()
    {
        _mockBt.Setup(bt => bt.SendCommandAsync(It.IsAny<byte[]>())).Returns(Task.CompletedTask);
        var expected = LegoCommandBuilder.SubscribeMotorSpeed("A");

        await _service.SubscribeMotorSpeedAsync("A");

        _mockBt.Verify(bt => bt.SendCommandAsync(It.Is<byte[]>(b => b.SequenceEqual(expected))), Times.Once);
    }

    // ---- SubscribeMotorPositionAsync ----

    [Fact]
    public async Task SubscribeMotorPositionAsync_SendsCorrectCommand()
    {
        _mockBt.Setup(bt => bt.SendCommandAsync(It.IsAny<byte[]>())).Returns(Task.CompletedTask);
        var expected = LegoCommandBuilder.SubscribeMotorPosition("A");

        await _service.SubscribeMotorPositionAsync("A");

        _mockBt.Verify(bt => bt.SendCommandAsync(It.Is<byte[]>(b => b.SequenceEqual(expected))), Times.Once);
    }

    // ---- RunSteeringHomingAsync ----

    [Fact]
    public async Task RunSteeringHomingAsync_Cancelled_ReturnsFailureResult()
    {
        _mockBt.Setup(bt => bt.SendCommandAsync(It.IsAny<byte[]>())).Returns(Task.CompletedTask);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _service.RunSteeringHomingAsync("D", cts.Token);

        result.Success.Should().BeFalse();
        result.Error.Should().Be("Homing cancelled");
    }

    [Fact]
    public async Task RunSteeringHomingAsync_BluetoothException_ReturnsFailureResult()
    {
        _mockBt.Setup(bt => bt.SendCommandAsync(It.IsAny<byte[]>()))
               .ThrowsAsync(new InvalidOperationException("BT disconnected"));

        var result = await _service.RunSteeringHomingAsync("D");

        result.Success.Should().BeFalse();
        result.Error.Should().Be("BT disconnected");
    }
}

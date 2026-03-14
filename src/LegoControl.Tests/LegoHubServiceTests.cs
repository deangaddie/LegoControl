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

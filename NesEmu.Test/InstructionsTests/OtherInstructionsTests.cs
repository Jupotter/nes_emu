namespace NesEmu.Test.InstructionsTests;

public class OtherInstructionsTests
{
    [Theory]
    [InlineData(0x00, CpuFlags.Zero)]
    [InlineData(0x01, CpuFlags.None)]
    [InlineData(0xFF, CpuFlags.Negative)]
    [InlineData(0x05, CpuFlags.None)]
    public void TAXTest(byte value, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA9, value, 0xAA };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(0x00, CpuFlags.Zero)]
    [InlineData(0x01, CpuFlags.None)]
    [InlineData(0xFF, CpuFlags.Negative)]
    [InlineData(0x05, CpuFlags.None)]
    public void TAYTest(byte value, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA9, value, 0xA8 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }

    [Theory]
    [InlineData(0xFF, CpuFlags.Zero)]
    [InlineData(0x01, CpuFlags.None)]
    [InlineData(0b01111111, CpuFlags.Negative)]
    [InlineData(0x05, CpuFlags.None)]
    public void INXTest(byte value, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA9, value, 0xAA, 0xE8 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterX.Should().Be((byte)(value + 1));
        tested.Status.Should().Be(expectedStatus);
    }
}

namespace NesEmu.Test.InstructionsTests;

public class LDXTests
{
    [Theory]
    [InlineData(0x00, CpuFlags.Zero)]
    [InlineData(0x01, CpuFlags.None)]
    [InlineData(0xFF, CpuFlags.Negative)]
    [InlineData(0x05, CpuFlags.None)]
    public void LDXImmTest(byte value, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA2, value };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
}

public class LDYTests
{
    [Theory]
    [InlineData(0x00, CpuFlags.Zero)]
    [InlineData(0x01, CpuFlags.None)]
    [InlineData(0xFF, CpuFlags.Negative)]
    [InlineData(0x05, CpuFlags.None)]
    public void LDYImmTest(byte value, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA0, value };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
}

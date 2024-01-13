namespace NesEmu.Test.InstructionsTests;

public class LDATests
{
    [Theory]
    [InlineData(0x00, CpuFlags.Zero)]
    [InlineData(0x01, CpuFlags.None)]
    [InlineData(0xFF, CpuFlags.Negative)]
    [InlineData(0x05, CpuFlags.None)]
    public void LDAImmTest(byte value, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA9, value };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
    }

    [Theory]
    [InlineData(0x00, 0x0, CpuFlags.Zero)]
    [InlineData(0x00, 0x1, CpuFlags.Zero)]
    [InlineData(0x00, 0xff, CpuFlags.Zero)]
    [InlineData(0x01, 0x0, CpuFlags.None)]
    [InlineData(0x01, 0x1, CpuFlags.None)]
    [InlineData(0x01, 0xff, CpuFlags.None)]
    [InlineData(0xFF, 0x0, CpuFlags.Negative)]
    [InlineData(0xFF, 0x1, CpuFlags.Negative)]
    [InlineData(0xFF, 0xff, CpuFlags.Negative)]
    [InlineData(0x05, 0x0, CpuFlags.None)]
    [InlineData(0x05, 0x1, CpuFlags.None)]
    [InlineData(0x05, 0xff, CpuFlags.None)]
    public void LDAZeroPageTest(byte value, byte address, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA5, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
    }

    [Theory]
    [InlineData(0x00, 0x0, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0x1, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0xff, 0x05, CpuFlags.Zero)]
    [InlineData(0x01, 0x0, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0x1, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0xff, 0x05, CpuFlags.None)]
    [InlineData(0xFF, 0x0, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0x1, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0xff, 0x05, CpuFlags.Negative)]
    [InlineData(0x05, 0x0, 0x05, CpuFlags.None)]
    [InlineData(0x05, 0x1, 0x05, CpuFlags.None)]
    [InlineData(0x05, 0xff, 0x05, CpuFlags.None)]
    public void LDAZeroPageXTest(byte value, byte address, byte offset, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA2, offset, 0xB5, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
    }

    [Theory]
    [InlineData(0x00, 0x0, CpuFlags.Zero)]
    [InlineData(0x00, 0x0101, CpuFlags.Zero)]
    [InlineData(0x00, 0x02ff, CpuFlags.Zero)]
    [InlineData(0x01, 0x0, CpuFlags.None)]
    [InlineData(0x01, 0x0101, CpuFlags.None)]
    [InlineData(0x01, 0x02ff, CpuFlags.None)]
    [InlineData(0xFF, 0x0, CpuFlags.Negative)]
    [InlineData(0xFF, 0x0101, CpuFlags.Negative)]
    [InlineData(0xFF, 0x02ff, CpuFlags.Negative)]
    [InlineData(0x05, 0x0, CpuFlags.None)]
    [InlineData(0x05, 0x0101, CpuFlags.None)]
    [InlineData(0x05, 0x02ff, CpuFlags.None)]
    [InlineData(0xAD, 0x8000, CpuFlags.Negative)]
    public void LDAAbsoluteTest(byte value, ushort address, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xAD, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
    }

    [Theory]
    [InlineData(0x00, 0x0300, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0x0301, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0x03ff, 0x05, CpuFlags.Zero)]
    [InlineData(0x01, 0x0300, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0x0301, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0x03ff, 0x05, CpuFlags.None)]
    [InlineData(0xFF, 0x0300, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0x0301, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0x03ff, 0x05, CpuFlags.Negative)]
    [InlineData(0x05, 0x0300, 0x05, CpuFlags.None)]
    [InlineData(0x05, 0x0301, 0x05, CpuFlags.None)]
    [InlineData(0x05, 0x03ff, 0x05, CpuFlags.None)]
    [InlineData(0xBD, 0x8000, 0x02, CpuFlags.Negative)]
    public void LDAAbsoluteXTest(byte value, ushort address, byte offset, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA2, offset, 0xBD, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
    }

    [Theory]
    [InlineData(0x00, 0x0300, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0x0301, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0x03ff, 0x05, CpuFlags.Zero)]
    [InlineData(0x01, 0x0300, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0x0301, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0x03ff, 0x05, CpuFlags.None)]
    [InlineData(0xFF, 0x0300, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0x0301, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0x03ff, 0x05, CpuFlags.Negative)]
    [InlineData(0x05, 0x0300, 0x05, CpuFlags.None)]
    [InlineData(0x05, 0x0301, 0x05, CpuFlags.None)]
    [InlineData(0x05, 0x03ff, 0x05, CpuFlags.None)]
    public void LDAAbsoluteYTest(byte value, ushort address, byte offset, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA0, offset, 0xB9, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
    }

    [Theory]
    [InlineData(0x00, 0x0300, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0x0301, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0x03ff, 0x05, CpuFlags.Zero)]
    [InlineData(0x01, 0x0300, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0x0301, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0x03ff, 0x05, CpuFlags.None)]
    [InlineData(0xFF, 0x0300, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0x0301, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0x03ff, 0x05, CpuFlags.Negative)]
    public void LDAIndirectXTest(byte value, ushort address, byte offset, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA2, offset, 0xA1, (byte)address };
        var tested = new Cpu();
        tested.MemWriteShort((byte)((byte)address + offset), 0x605);
        tested.MemWriteByte(0x605, value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
    }

    [Theory]
    [InlineData(0x00, 0x00, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0x01, 0x05, CpuFlags.Zero)]
    [InlineData(0x00, 0xff, 0x05, CpuFlags.Zero)]
    [InlineData(0x01, 0x00, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0x01, 0x05, CpuFlags.None)]
    [InlineData(0x01, 0xff, 0x05, CpuFlags.None)]
    [InlineData(0xFF, 0x00, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0x01, 0x05, CpuFlags.Negative)]
    [InlineData(0xFF, 0xff, 0x05, CpuFlags.Negative)]
    public void LDAIndirectYTest(byte value, ushort address, byte offset, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA0, offset, 0xB1, (byte)address };
        var tested = new Cpu();
        tested.MemWriteShort(address, 0x605);
        tested.MemWriteByte((ushort)(0x605+offset), value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
    }
}
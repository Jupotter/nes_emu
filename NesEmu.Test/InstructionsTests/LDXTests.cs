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
    public void LDXZeroPageTest(byte value, byte address, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA6, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
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
    public void LDXZeroPageYTest(byte value, byte address, byte offset, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA0, offset, 0xB6, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
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
    [InlineData(0xAE, 0x8000, CpuFlags.Negative)]
    public void LDXAbsoluteTest(byte value, ushort address, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xAE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
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
    [InlineData(0xBE, 0x8000, 0x02, CpuFlags.Negative)]
    public void LDXAbsoluteYTest(byte value, ushort address, byte offset, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA0, offset, 0xBE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
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
    public void LDYZeroPageTest(byte value, byte address, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA4, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
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
    public void LDYZeroPageXTest(byte value, byte address, byte offset, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA2, offset, 0xB4, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
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
    [InlineData(0xAC, 0x8000, CpuFlags.Negative)]
    public void LDYAbsoluteTest(byte value, ushort address, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xAC, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
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
    [InlineData(0xBC, 0x8000, 0x02, CpuFlags.Negative)]
    public void LDYAbsoluteXTest(byte value, ushort address, byte offset, CpuFlags expectedStatus)
    {
        var program = new byte[] { 0xA2, offset, 0xBC, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001+program.Length));
    }
}

namespace NesEmu.Test.InstructionsTests;

public class STATests
{
    [Theory]
    [InlineData(0x00)]
    [InlineData(0x01)]
    [InlineData(0xFF)]
    [InlineData(0x05)]
    public void STAZeroPageTest(byte address)
    {
        var program = new byte[] { 0xA9, 0xDE, 0x85, address };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [Theory]
    [InlineData(0x00, 0x00)]
    [InlineData(0x00, 0x01)]
    [InlineData(0x00, 0x05)]
    [InlineData(0x00, 0xFF)]
    [InlineData(0x01, 0x00)]
    [InlineData(0x01, 0x01)]
    [InlineData(0x01, 0x05)]
    [InlineData(0x01, 0xFF)]
    [InlineData(0xFF, 0x00)]
    [InlineData(0xFF, 0x01)]
    [InlineData(0xFF, 0x05)]
    [InlineData(0xFF, 0xFF)]
    public void STAZeroPageXTest(byte address, byte offset)
    {
        var program = new byte[] { 0xA2, offset, 0xA9, 0xDE, 0x95, address };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte((byte)(address + offset)).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [Theory]
    [InlineData(0x0000)]
    [InlineData(0x3101)]
    [InlineData(0xFFFF)]
    public void STAAbsoluteTest(ushort address)
    {
        var program = new byte[] { 0xA9, 0xDE, 0x8D, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
    
    [Theory]
    [InlineData(0x0000, 0x00)]
    [InlineData(0x0000, 0x05)]
    [InlineData(0x0000, 0xff)]
    [InlineData(0x3101, 0x00)]
    [InlineData(0x3101, 0x05)]
    [InlineData(0x3101, 0xff)]
    [InlineData(0xFFFF, 0x00)]
    [InlineData(0xFFFF, 0x05)]
    [InlineData(0xFFFF, 0xff)]
    public void STAAbsoluteXTest(ushort address, byte offset)
    {
        var program = new byte[] { 0xA2, offset, 0xA9, 0xDE, 0x9D, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte((ushort)(address+ offset)).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
    
    [Theory]
    [InlineData(0x0000, 0x00)]
    [InlineData(0x0000, 0x05)]
    [InlineData(0x0000, 0xff)]
    [InlineData(0x3101, 0x00)]
    [InlineData(0x3101, 0x05)]
    [InlineData(0x3101, 0xff)]
    [InlineData(0xFFFF, 0x00)]
    [InlineData(0xFFFF, 0x05)]
    [InlineData(0xFFFF, 0xff)]
    public void STAAbsoluteYTest(ushort address, byte offset)
    {
        var program = new byte[] { 0xA0, offset, 0xA9, 0xDE, 0x99, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte((ushort)(address+ offset)).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
    
    [Theory]
    [InlineData(0x00, 0x00)]
    [InlineData(0x00, 0x05)]
    [InlineData(0x00, 0xff)]
    [InlineData(0x01, 0x00)]
    [InlineData(0x01, 0x05)]
    [InlineData(0x01, 0xff)]
    [InlineData(0xFF, 0x00)]
    [InlineData(0xFF, 0x05)]
    [InlineData(0xFF, 0xff)]
    public void STAIndirectXTest(byte address, byte offset)
    {
        var program = new byte[] { 0xA2, offset, 0xA9, 0xDE, 0x81, address };
        var tested = new Cpu();
        tested.MemWriteShort((byte)((byte)address + offset), 0x605);

        tested.Interpret(program);

        tested.MemReadByte(0x605).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
    
    [Theory]
    [InlineData(0x00, 0x00)]
    [InlineData(0x00, 0x05)]
    [InlineData(0x00, 0xff)]
    [InlineData(0x01, 0x00)]
    [InlineData(0x01, 0x05)]
    [InlineData(0x01, 0xff)]
    [InlineData(0xFF, 0x00)]
    [InlineData(0xFF, 0x05)]
    [InlineData(0xFF, 0xff)]
    public void STAIndirectYTest(byte address, byte offset)
    {
        var program = new byte[] { 0xA0, offset, 0xA9, 0xDE, 0x91, address };
        var tested = new Cpu();
        tested.MemWriteShort(address, 0x605);

        tested.Interpret(program);

        tested.MemReadByte((ushort)(0x605+offset)).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
}

namespace NesEmu.Test.InstructionsTests;

[TestFixture]
public class STATests
{
    [Test]
    public void STAZeroPageTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte address)
    {
        var program = new byte[] { 0xA9, 0xDE, 0x85, address };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void STAZeroPageXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var program = new byte[] { 0xA2, offset, 0xA9, 0xDE, 0x95, address };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte((byte)(address + offset)).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void STAAbsoluteTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))] ushort address)
    {
        var program = new byte[] { 0xA9, 0xDE, 0x8D, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void STAAbsoluteXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))] ushort address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var program = new byte[] { 0xA2, offset, 0xA9, 0xDE, 0x9D, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte((ushort)(address + offset)).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void STAAbsoluteYTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))] ushort address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var program = new byte[] { 0xA0, offset, 0xA9, 0xDE, 0x99, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.MemReadByte((ushort)(address + offset)).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void STAIndirectXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte offset)
    {
        var program = new byte[] { 0xA2, offset, 0xA9, 0xDE, 0x81, address };
        var tested = new Cpu();
        tested.MemWriteShort((byte)((byte)address + offset), 0x605);

        tested.Interpret(program);

        tested.MemReadByte(0x605).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void STAIndirectYTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var program = new byte[] { 0xA0, offset, 0xA9, 0xDE, 0x91, address };
        var tested = new Cpu();
        tested.MemWriteShort(address, 0x605);

        tested.Interpret(program);

        tested.MemReadByte((ushort)(0x605 + offset)).Should().Be(0xDE);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
}

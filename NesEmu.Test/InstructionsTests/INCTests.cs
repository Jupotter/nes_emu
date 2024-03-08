namespace NesEmu.Test.InstructionsTests;

[TestFixture]
public class INCTests
{
    [Test]
    public void INCZeroPageTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value+1));
        var program = new byte[] { 0xE6, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be((byte)(value+1));
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void INCZeroPageXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value+1));
        var program = new byte[] { 0xA2, offset, 0xF6, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((byte)(address + offset)).Should().Be((byte)(value+1));
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void INCAbsoluteTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value+1));
        var program = new byte[] { 0xEE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be((byte)(value+1));
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void INCAbsoluteXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value+1));
        var program = new byte[] { 0xA2, offset, 0xFE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((ushort)(address + offset)).Should().Be((byte)(value+1));
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

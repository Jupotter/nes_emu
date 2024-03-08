namespace NesEmu.Test.InstructionsTests;

[TestFixture]
public class DECTests
{
    [Test]
    public void DECZeroPageTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value-1));
        var program = new byte[] { 0xC6, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be((byte)(value-1));
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void DECZeroPageXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value-1));
        var program = new byte[] { 0xA2, offset, 0xD6, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((byte)(address + offset)).Should().Be((byte)(value-1));
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void DECAbsoluteTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value-1));
        var program = new byte[] { 0xCE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be((byte)(value-1));
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void DECAbsoluteXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value-1));
        var program = new byte[] { 0xA2, offset, 0xDE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((ushort)(address + offset)).Should().Be((byte)(value-1));
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

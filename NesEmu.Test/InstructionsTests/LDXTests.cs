namespace NesEmu.Test.InstructionsTests;

[TestFixture]
public class LDXTests
{
    [Test]
    public void LDXImmTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, value };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }


    [Test]
    public void LDXZeroPageTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA6, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }


    [Test]
    public void LDXZeroPageYTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA0, offset, 0xB6, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }


    [Test]
    public void LDXAbsoluteTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xAE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }


    [Test]
    public void LDXAbsoluteYTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA0, offset, 0xBE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

[TestFixture]
public class LDYTests
{
    [Test]
    public void LDYImmTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA0, value };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void LDYZeroPageTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA4, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void LDYZeroPageXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, offset, 0xB4, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [TestCase(0xAC, (ushort)0x8000)]
    public void LDYAbsoluteTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xAC, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [TestCase(0xBC, (ushort)0x8000, 0x02)]
    public void LDYAbsoluteXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, offset, 0xBC, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

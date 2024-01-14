namespace NesEmu.Test.InstructionsTests;

[TestFixture]
public class LDXTests
{
    [Test]
    public void LDXImmTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, value };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void LDXZeroPageTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte address)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA6, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void LDXZeroPageYTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA0, offset, 0xB6, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void LDXAbsoluteTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))]
        ushort address)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xAE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void LDXAbsoluteYTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))]
        ushort address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA0, offset, 0xBE, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
}

[TestFixture]
public class LDYTests
{
    [Test]
    public void LDYImmTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA0, value };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [Test]
    public void LDYZeroPageTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte address)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA4, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [Test]
    public void LDYZeroPageXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, offset, 0xB4, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [TestCase(0xAC, (ushort)0x8000)]
    public void LDYAbsoluteTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))]
        ushort address)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xAC, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [TestCase(0xBC, (ushort)0x8000, 0x02)]
    public void LDYAbsoluteXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))]
        ushort address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, offset, 0xBC, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
}

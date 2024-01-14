namespace NesEmu.Test.InstructionsTests;

[TestFixture]
public class LDATests
{
    [Test]
    public void LDAImmTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA9, value };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [Test]
    public void LDAZeroPageTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte address)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA5, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [Test]
    public void LDAZeroPageXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, offset, 0xB5, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [Test]
    [TestCase(0xAD, (ushort)0x8000)]
    public void LDAAbsoluteTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))]
        ushort address)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xAD, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [Test]
    [TestCase(0xBD, (ushort)0x8000, 0x02)]
    public void LDAAbsoluteXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))]
        ushort address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, offset, 0xBD, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }

    [Test]
    public void LDAAbsoluteYTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestWords))]
        ushort address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA0, offset, 0xB9, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void LDAIndirectXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, offset, 0xA1, address };
        var tested = new Cpu();
        tested.MemWriteShort((byte)(address + offset), 0x605);
        tested.MemWriteByte(0x605, value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }


    [Test]
    public void LDAIndirectYTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte address,
        [ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))]
        byte offset)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA0, offset, 0xB1, address };
        var tested = new Cpu();
        tested.MemWriteShort(address, 0x605);
        tested.MemWriteByte((ushort)(0x605 + offset), value);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be((ushort)(0x8001 + program.Length));
    }
}

namespace NesEmu.Test.InstructionsTests;

public class LSRTest
{
    private static (byte result, CpuFlags flags) GetExpectedResult(byte value)
    {
        var resultInt = value / 2;
        var result = (byte)resultInt;
        
        var expectedStatus = Utils.GetExpectedFlag(result);
        if ((value & 1 ) != 0)
            expectedStatus |= CpuFlags.Carry;
        return (result, expectedStatus);
    }
    
    [Test]
    public void LSRAccTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0xA9, value, 0x4A };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void LSRZeroPageTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0x46, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void LSRZeroPageXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0xA2, offset, 0x56, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((byte)(address + offset)).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void LSRAbsoluteTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0x4E, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void LSRAbsoluteXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0xA2, offset, 0x5E, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((ushort)(address + offset)).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

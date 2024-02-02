namespace NesEmu.Test.InstructionsTests;

public class ROLTest
{
    private static (byte result, CpuFlags flags) GetExpectedResult(byte value, bool carry = false)
    {
        var resultInt = value * 2;
        var result = (byte)resultInt;

        if (carry)
            result |= 0x1;
        
        var expectedStatus = Utils.GetExpectedFlag(result);
        if ((value & 0x80 ) != 0)
            expectedStatus |= CpuFlags.Carry;
        return (result, expectedStatus);
    }
    
    [Test]
    public void ROLAccTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var (result, expectedStatus) = GetExpectedResult(value, false);
        var program = new byte[] { 0xA9, value, 0x2A };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ROLAccTestsWithCarry([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var (result, expectedStatus) = GetExpectedResult(value, true);
        var program = new byte[] { 0xA9, value, 0x38, 0x2A };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ROLZeroPageTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0x26, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ROLZeroPageXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0xA2, offset, 0x36, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((byte)(address + offset)).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ROLAbsoluteTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0x2E, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ROLAbsoluteXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0xA2, offset, 0x3E, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((ushort)(address + offset)).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

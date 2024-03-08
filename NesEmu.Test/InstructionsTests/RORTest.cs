namespace NesEmu.Test.InstructionsTests;

public class RORTest
{
    private static (byte result, CpuFlags flags) GetExpectedResult(byte value, bool carry = false)
    {
        var resultInt = value / 2;
        var result = (byte)resultInt;

        if (carry)
            result |= 0x80;
        
        var expectedStatus = Utils.GetExpectedFlag(result);
        if ((value & 1 ) != 0)
            expectedStatus |= CpuFlags.Carry;
        return (result, expectedStatus);
    }
    
    [Test]
    public void RORAccTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var (result, expectedStatus) = GetExpectedResult(value, false);
        var program = new byte[] { 0xA9, value, 0x6A };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void RORAccTestsWithCarry([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var (result, expectedStatus) = GetExpectedResult(value, true);
        var program = new byte[] { 0xA9, value, 0x38, 0x6A };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void RORZeroPageTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0x66, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void RORZeroPageXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0xA2, offset, 0x76, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((byte)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((byte)(address + offset)).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void RORAbsoluteTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0x6E, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, value);

        tested.Interpret(program);

        tested.MemReadByte(address).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void RORAbsoluteXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(value);
        var program = new byte[] { 0xA2, offset, 0x7E, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((ushort)(address + offset), value);

        tested.Interpret(program);

        tested.MemReadByte((ushort)(address + offset)).Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

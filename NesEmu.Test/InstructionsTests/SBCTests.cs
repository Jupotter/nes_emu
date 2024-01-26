namespace NesEmu.Test.InstructionsTests;

public class SBCTests
{
    private static (byte result, CpuFlags flags) GetExpectedResult(byte left, byte right, bool carry = false)
    {
        var resultInt = left - right - (carry ? 0 : 1);
        var result = (byte)resultInt;

        carry = resultInt >= 0;
        var expectedStatus = Utils.GetExpectedFlag(result);
        if (carry)
        {
            expectedStatus |= CpuFlags.Carry;
        }
        
        if (Utils.ByteSign(left) == Utils.ByteSign((byte)~right) && Utils.ByteSign(left) != Utils.ByteSign(result))
        {
            expectedStatus |= CpuFlags.Overflow;
        }

        return (result, expectedStatus);
    }

    [Test]
    public void SBCImmTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0xE9, right };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void SBCImmWithCarryTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right, true);
        var program = new byte[] { 0xA9, left, 0x38, 0xE9, right };
        var tested = new Cpu();
    
        tested.Interpret(program);
    
        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void SBCZeroPageTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0xE5, address };
        var tested = new Cpu();
        tested.MemWriteByte(address, right);
    
        tested.Interpret(program);
    
        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void SBCZeroPageXTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, offset, 0xA9, left, 0xF5, address };
        var tested = new Cpu();
        tested.MemWriteByte((byte)(address + offset), right);
    
        tested.Interpret(program);
    
        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void SBCAbsoluteTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0xED, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte(address, right);
    
        tested.Interpret(program);
    
        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void SBCAbsoluteXTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, offset, 0xA9, left, 0xFD,  (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), right);
    
        tested.Interpret(program);
    
        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void SBCAbsoluteYTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA0, offset, 0xA9, left, 0xF9,  (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        tested.MemWriteByte((ushort)(address + offset), right);
    
        tested.Interpret(program);
    
        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void SBCIndirectXTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, offset, 0xA9, left, 0xE1, address };
        var tested = new Cpu();
        tested.MemWriteShort((byte)(address + offset), 0x605);
        tested.MemWriteByte(0x605, right);
    
        tested.Interpret(program);
    
        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void SBCIndirectYTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA0, offset, 0xA9, left, 0xF1, address };
        var tested = new Cpu();
        tested.MemWriteShort(address, 0x605);
        tested.MemWriteByte((ushort)(0x605+offset), right);
    
        tested.Interpret(program);
    
        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

namespace NesEmu.Test.InstructionsTests;

public class CPXTest
{
    private static (byte result, CpuFlags flags) GetExpectedResult(byte left, byte right)
    {
        var resultInt = left - right;
        var result = (byte)resultInt;
        
        var expectedStatus = Utils.GetExpectedFlag(result);
        if (resultInt >= 0)
            expectedStatus |= CpuFlags.Carry;
        return (result, expectedStatus);
    }

    [Test]
    public void CPXImmTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, left, 0xE0, right };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void CPXZeroPageTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, left, 0xE4, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, right);

        tested.Interpret(program);

        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void CPXAbsoluteTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, left, 0xEC, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, right);

        tested.Interpret(program);

        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}


public class CPYTest
{
    private static (byte result, CpuFlags flags) GetExpectedResult(byte left, byte right)
    {
        var resultInt = left - right;
        var result = (byte)resultInt;
        
        var expectedStatus = Utils.GetExpectedFlag(result);
        if (resultInt >= 0)
            expectedStatus |= CpuFlags.Carry;
        return (result, expectedStatus);
    }

    [Test]
    public void CPYImmTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA0, left, 0xC0, right };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void CPYZeroPageTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA0, left, 0xC4, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, right);

        tested.Interpret(program);

        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void CPYAbsoluteTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA0, left, 0xCC, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, right);

        tested.Interpret(program);

        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}
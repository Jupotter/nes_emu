namespace NesEmu.Test.InstructionsTests;

public class EORTest
{
    private static (byte result, CpuFlags flags) GetExpectedResult(byte left, byte right)
    {
        var result = (byte)(left ^ right);
        
        var expectedStatus = Utils.GetExpectedFlag(result);
        return (result, expectedStatus);
    }

    [Test]
    public void EORImmTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0x49, right };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void EORZeroPageTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0x45, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void EORZeroPageXTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, offset, 0xA9, left, 0x55, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((byte)(address + offset), right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void EORAbsoluteTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0x4D, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void EORAbsoluteXTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, offset, 0xA9, left, 0x5D,  (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((ushort)(address + offset), right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void EORAbsoluteYTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA0, offset, 0xA9, left, 0x59,  (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((ushort)(address + offset), right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void EORIndirectXTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, offset, 0xA9, left, 0x41, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteShortZeroPage((byte)(address + offset), 0x605);
        tested.MemWriteByte(0x605, right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void EORIndirectYTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA0, offset, 0xA9, left, 0x51, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteShortZeroPage(address, 0x605);
        tested.MemWriteByte((ushort)(0x605+offset), right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

namespace NesEmu.Test.InstructionsTests;

public class ANDTest
{
    private static (byte result, CpuFlags flags) GetExpectedResult(byte left, byte right)
    {
        var result = (byte)(left & right);
        
        var expectedStatus = Utils.GetExpectedFlag(result);
        return (result, expectedStatus);
    }

    [Test]
    public void ANDImmTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0x29, right };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ANDZeroPageTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0x25, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ANDZeroPageXTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, offset, 0xA9, left, 0x35, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((byte)(address + offset), right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ANDAbsoluteTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0x2D, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte(address, right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ANDAbsoluteXTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, offset, 0xA9, left, 0x3D,  (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((ushort)(address + offset), right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ANDAbsoluteYTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestWords))]
        ushort address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA0, offset, 0xA9, left, 0x39,  (byte)address, (byte)(address >> 8) };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteByte((ushort)(address + offset), right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ANDIndirectXTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA2, offset, 0xA9, left, 0x21, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteShort((byte)(address + offset), 0x605);
        tested.MemWriteByte(0x605, right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
    
    [Test]
    public void ANDIndirectYTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte address,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte offset)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA0, offset, 0xA9, left, 0x31, address };
        var tested = new Cpu(new Utils.TestBus());
        tested.MemWriteShort(address, 0x605);
        tested.MemWriteByte((ushort)(0x605+offset), right);

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

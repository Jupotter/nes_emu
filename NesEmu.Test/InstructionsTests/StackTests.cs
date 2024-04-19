namespace NesEmu.Test.InstructionsTests;

public class StackTests
{
    [Test]
    public void TXSTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var program = new byte[] { 0xA2, value, 0xA9, 0x00, 0x9A };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterS.Should().Be(value);
        tested.RegisterX.Should().Be(value);
        tested.Status.Should().HaveFlag(CpuFlags.Zero);
    }

    [Test]
    public void TSXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, value, 0x9A, 0xA2, 0x00, 0xBA };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterS.Should().Be(value);
        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }
    
    [Test]
    public void PHATest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA9, value, 0x48 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterS.Should().Be(0xFC);
        tested.MemReadByte(0x1FD).Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }
    
    [Test]
    public void PHPTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        expectedStatus |= CpuFlags.BreakCommand;
        var program = new byte[] { 0xA9, value, 0x08 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterS.Should().Be(0xFC);
        tested.MemReadByte(0x1FD).Should().Be((byte)expectedStatus);
    }
        
    [Test]
    public void PLATest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA9, value, 0x48, 0xA9, 0x01, 0x68 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterS.Should().Be(0xFD);
        tested.RegisterA.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }
    
    [Test]
    public void PLPTest()
    {
        var program = new byte[] { 0xA9, 0b11001111, 0x48, 0xA9, 0x01, 0x28 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.RegisterS.Should().Be(0xFD);
        tested.Status.Should().Be(CpuFlags.Negative | CpuFlags.Overflow | CpuFlags.DecimalMode | CpuFlags.InterruptDisable | CpuFlags.Zero | CpuFlags.Carry);
    }
}

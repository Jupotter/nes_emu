namespace NesEmu.Test.InstructionsTests;

public class BranchTests
{
    [Test]
    public void BCCTestTrue([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0x18, 0x90, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(Utils.ExpectedPc(program) + (sbyte)value));
        tested.Status.Should().NotHaveFlag(CpuFlags.Carry);
    }

    [Test]
    public void BCCTestFalse([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0x38, 0x90, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be(Utils.ExpectedPc(program));
        tested.Status.Should().HaveFlag(CpuFlags.Carry);
    }

    [Test]
    public void BCSTestTrue([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0x38, 0xB0, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(Utils.ExpectedPc(program) + (sbyte)value));
        tested.Status.Should().HaveFlag(CpuFlags.Carry);
    }

    [Test]
    public void BCSTestFalse([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0x18, 0xB0, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be(Utils.ExpectedPc(program));
        tested.Status.Should().NotHaveFlag(CpuFlags.Carry);
    }

    [Test]
    public void BEQTestTrue([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 0, 0xF0, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(Utils.ExpectedPc(program) + (sbyte)value));
        tested.Status.Should().HaveFlag(CpuFlags.Zero);
    }

    [Test]
    public void BEQTestFalse([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 1, 0xF0, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be(Utils.ExpectedPc(program));
        tested.Status.Should().NotHaveFlag(CpuFlags.Zero);
    }

    [Test]
    public void BNETestTrue([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 1, 0xD0, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(Utils.ExpectedPc(program) + (sbyte)value));
        tested.Status.Should().NotHaveFlag(CpuFlags.Zero);
    }

    [Test]
    public void BNETestFalse([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 0, 0xD0, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be(Utils.ExpectedPc(program));
        tested.Status.Should().HaveFlag(CpuFlags.Zero);
    }


    [Test]
    public void BMITestTrue([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 0xff, 0x30, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(Utils.ExpectedPc(program) + (sbyte)value));
        tested.Status.Should().HaveFlag(CpuFlags.Negative);
    }

    [Test]
    public void BMITestFalse([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 1, 0x30, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be(Utils.ExpectedPc(program));
        tested.Status.Should().NotHaveFlag(CpuFlags.Negative);
    }

    [Test]
    public void BPLTestTrue([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 1, 0x10, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(Utils.ExpectedPc(program) + (sbyte)value));
        tested.Status.Should().NotHaveFlag(CpuFlags.Negative);
    }

    [Test]
    public void BPLTestFalse([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 0xff, 0x10, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be(Utils.ExpectedPc(program));
        tested.Status.Should().HaveFlag(CpuFlags.Negative);
    }
    
    [Test]
    public void BVCTestTrue([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 1, 0x50, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(Utils.ExpectedPc(program) + (sbyte)value));
        tested.Status.Should().NotHaveFlag(CpuFlags.Overflow);
    }

    [Test]
    public void BVCTestFalse([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] {0xA9, 0x7f, 0x69, 0x7f, 0x50, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be(Utils.ExpectedPc(program));
        tested.Status.Should().HaveFlag(CpuFlags.Overflow);
    }
    
    [Test]
    public void BVSTestTrue([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 0x7f, 0x69, 0x7f, 0x70, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(Utils.ExpectedPc(program) + (sbyte)value));
        tested.Status.Should().HaveFlag(CpuFlags.Overflow);
    }

    [Test]
    public void BVSTestFalse([ValueSource(typeof(Utils), nameof(Utils.TestBranchBytes))] byte value)
    {
        var program = new byte[] { 0xA9, 1, 0x70, value };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.PC.Should().Be(Utils.ExpectedPc(program));
        tested.Status.Should().NotHaveFlag(CpuFlags.Overflow);
    }
}

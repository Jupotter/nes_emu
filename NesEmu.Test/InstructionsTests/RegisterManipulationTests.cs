namespace NesEmu.Test.InstructionsTests;

public class RegisterManipulationTests
{
    
    [Test]
    public void SECTest()
    {
        var program = new byte[] { 0x38 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().HaveFlag(CpuFlags.Carry);
    }
    
    [Test]
    public void SEDTest()
    {
        var program = new byte[] { 0xF8 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().HaveFlag(CpuFlags.DecimalMode);
    }
    
    [Test]
    public void SEITest()
    {
        var program = new byte[] { 0x78 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().HaveFlag(CpuFlags.InterruptDisable);
    }

    [Test]
    public void CombinedTest()
    {
        var program = new byte[] { 0x38, 0x78, 0xF8 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().Be(CpuFlags.Carry | CpuFlags.DecimalMode | CpuFlags.InterruptDisable | CpuFlags.Unused);
    }
    
    [Test]
    public void CLCTest()
    {
        var program = new byte[] { 0x38, 0x78, 0xF8, 0x18 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().Be(CpuFlags.DecimalMode | CpuFlags.InterruptDisable | CpuFlags.Unused);
    }
    
    [Test]
    public void CLDTest()
    {
        var program = new byte[] { 0x38, 0x78, 0xF8, 0xd8 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().Be(CpuFlags.Carry | CpuFlags.InterruptDisable | CpuFlags.Unused);
    }
    
    [Test]
    public void CLITest()
    {
        var program = new byte[] { 0x38, 0x78, 0xF8, 0x58 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().Be(CpuFlags.Carry | CpuFlags.DecimalMode | CpuFlags.Unused);
    }
    
    [Test]
    public void CLVTest()
    {
        var program = new byte[] {  0xA9, 0x7f, 0x69, 0x7f, 0xb8 };
        var tested = new Cpu(new Utils.TestBus());

        tested.Interpret(program);

        tested.Status.Should().NotHaveFlag(CpuFlags.Overflow);
    }
}

namespace NesEmu.Test.InstructionsTests;

[TestFixture]
public class OtherInstructionsTests
{
    [Test]
    public void TAXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA9, value, 0xAA };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }

    [Test]
    public void TAYTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA9, value, 0xA8 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }
    
    [Test]
    public void TXATest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA2, value, 0x8A };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }
    
    [Test]
    public void TYATest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag(value);
        var program = new byte[] { 0xA0, value, 0x98 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }

    [Test]
    public void INXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value + 1));
        var program = new byte[] { 0xA9, value, 0xAA, 0xE8 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterX.Should().Be((byte)(value + 1));
        tested.Status.Should().Be(expectedStatus);
    }
    
    [Test]
    public void INYTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value + 1));
        var program = new byte[] { 0xA0, value, 0xC8 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterY.Should().Be((byte)(value + 1));
        tested.Status.Should().Be(expectedStatus);
    }
    
    [Test]
    public void DEXTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value - 1));
        var program = new byte[] { 0xA2, value, 0xCA };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterX.Should().Be((byte)(value - 1));
        tested.Status.Should().Be(expectedStatus);
    }
    
    [Test]
    public void DEYTest([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte value)
    {
        var expectedStatus = Utils.GetExpectedFlag((byte)(value - 1));
        var program = new byte[] { 0xA0, value, 0x88 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterY.Should().Be((byte)(value - 1));
        tested.Status.Should().Be(expectedStatus);
    }

    [Test]
    public void SECTest()
    {
        var program = new byte[] { 0x38 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.Status.Should().Be(CpuFlags.Carry);
    }
}

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
    public void JMPAbsoluteTest([ValueSource(typeof(Utils), nameof(Utils.TestWords))] ushort address)
    {
        var program = new byte[] { 0x4C, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        
        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(address+1));
    }
    
    [Test]
    public void JMPIndirectBugTest()
    {
        var program = new byte[] { 0x6C, 0xFF, 0x01 };
        var tested = new Cpu();
        
        tested.MemWriteByte(0x0100, 0xDE);
        tested.MemWriteByte(0x01FF, 0xAC);
        tested.MemWriteByte(0x0200, 0x0D);
        
        tested.Interpret(program);

        tested.PC.Should().Be(0xDEAD);
    }
    
    [Test]
    public void JSRAbsoluteTest([ValueSource(typeof(Utils), nameof(Utils.TestWords))] ushort address)
    {
        var program = new byte[] { 0x20, (byte)address, (byte)(address >> 8) };
        var tested = new Cpu();
        
        tested.Interpret(program);

        tested.PC.Should().Be((ushort)(address+1));
        tested.RegisterS.Should().Be(0xFD);
        tested.MemReadShort(0x1FE).Should().Be(0x8002);
    }
    
    [Test]
    public void RTSTest()
    {
        var program = new byte[] { 0x20, 0x06, 0x80, 0xAA, 0x00, 0x00, 0xA9, 0x77, 0x60 };
        var tested = new Cpu();
        
        tested.Interpret(program);

        tested.PC.Should().Be(0x8005);
        tested.RegisterS.Should().Be(0xFF);
        tested.RegisterX.Should().Be(0x77);
    }
}

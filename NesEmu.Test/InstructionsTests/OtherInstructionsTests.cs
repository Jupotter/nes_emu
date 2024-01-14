namespace NesEmu.Test.InstructionsTests;

[TestFixture]
public class OtherInstructionsTests
{
    [Test]
    public void TAXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA9, value, 0xAA };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterX.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }

    [Test]
    public void TAYTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag(value);
        var program = new byte[] { 0xA9, value, 0xA8 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterY.Should().Be(value);
        tested.Status.Should().Be(expectedStatus);
    }


    [Test]
    public void INXTest([ValueSource(typeof(DataGenerator), nameof(DataGenerator.TestBytes))] byte value)
    {
        var expectedStatus = DataGenerator.GetExpectedFlag((byte)(value + 1));
        var program = new byte[] { 0xA9, value, 0xAA, 0xE8 };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(value);
        tested.RegisterX.Should().Be((byte)(value + 1));
        tested.Status.Should().Be(expectedStatus);
    }
}

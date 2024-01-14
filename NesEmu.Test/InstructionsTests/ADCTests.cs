namespace NesEmu.Test.InstructionsTests;

public class ADCTests
{
    private static (byte result, CpuFlags flags) GetExpectedResult(byte left, byte right, bool carry = false)
    {
        var resultInt = left + right + (carry ? 1 : 0);
        var result = (byte)resultInt;

        carry = resultInt > byte.MaxValue;
        var expectedStatus = Utils.GetExpectedFlag(result);
        if (carry)
        {
            expectedStatus |= CpuFlags.Carry;
        }

        return (result, expectedStatus);
    }

    [Test]
    public void ADCImmTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right);
        var program = new byte[] { 0xA9, left, 0x69, right };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }

    [Test]
    public void ADCImmWithCarryTests([ValueSource(typeof(Utils), nameof(Utils.TestBytes))] byte left,
        [ValueSource(typeof(Utils), nameof(Utils.TestBytes))]
        byte right)
    {
        var (result, expectedStatus) = GetExpectedResult(left, right, true);
        var program = new byte[] { 0xA9, left, 0x38, 0x69, right };
        var tested = new Cpu();

        tested.Interpret(program);

        tested.RegisterA.Should().Be(result);
        tested.Status.Should().Be(expectedStatus);
        tested.PC.Should().Be(Utils.ExpectedPc(program));
    }
}

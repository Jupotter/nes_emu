namespace NesEmu.Test.InstructionsTests;

public class Utils
{
    public static readonly byte[] TestBytes = [0x00, 0x01, 0x05, 0x80, 0xff];

    public static readonly ushort[] TestWords = [0x0000, 0x0001, 0x00ff, 0x3005, 0xffff];

    public static CpuFlags GetExpectedFlag(byte value)
    {
        if ((sbyte)value < 0)
        {
            return CpuFlags.Negative;
        }

        return value switch
        {
            0x00 => CpuFlags.Zero,
            _ => CpuFlags.None,
        };
    }

    public static ushort ExpectedPc(byte[] program)
    {
        return (ushort)(0x8001 + program.Length);
    }

    public static byte ByteSign(byte value)
    {
        return (byte)(value & 0x80);
    }
}

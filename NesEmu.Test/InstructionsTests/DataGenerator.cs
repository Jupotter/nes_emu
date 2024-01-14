namespace NesEmu.Test.InstructionsTests;

public class DataGenerator
{
    public static readonly byte[] TestBytes = [0x00, 0x01, 0x05, 0x80, 0xff];
    
    public static readonly ushort[] TestWords = [0x0000, 0x0001, 0x00ff, 0x3005, 0xffff];
    
    public static CpuFlags GetExpectedFlag(byte value)
    {
        if ((sbyte)value < 0)
            return CpuFlags.Negative;

        return value switch
        {
            0x00 => CpuFlags.Zero,
            _ => CpuFlags.None,
        };
    }
}

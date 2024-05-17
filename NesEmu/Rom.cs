namespace NesEmu;

public enum ScreenMirroring
{
    Vertical,
    Horizontal,
    FourScreen
}

public class Rom
{
    private const int PrgRomPageSize = 0x4000;
    private const int ChrRomPageSize = 0x2000;
    private static readonly byte[] NesTag = [(byte)'N', (byte)'E', (byte)'S', 0x1A];

    public Rom(byte[] prgRom, byte[] chrRom, byte mapper, ScreenMirroring mirroring)
    {
        Mirroring = mirroring;
        PrgRom = prgRom.ToImmutableArray();
        ChrRom = chrRom.ToImmutableArray();
        Mapper = mapper;
    }

    public ImmutableArray<byte> PrgRom { get; }
    public ImmutableArray<byte> ChrRom { get; }

    public byte Mapper { get; }

    public ScreenMirroring Mirroring { get; }

    public static Rom Parse(byte[] input)
    {
        if (!input[..4].SequenceEqual(NesTag) || input.Length < 16)
        {
            throw new InvalidDataException("Not a NES rom");
        }

        var controlByte1 = input[6];
        var controlByte2 = input[7];

        // Upper 4 bites of control bytes are the mapper definition, in LSB
        var mapper = (byte)((controlByte2 & 0b1111_0000) | (controlByte1 >> 4));

        // bytes 2/3 are iNES version
        var inesVersion = controlByte2 >> 2 & 0b11;
        if (inesVersion != 0)
        {
            throw new NotSupportedException("NES2.0 format is not supported");
        }

        var fourScreen = (controlByte1 & 0b1000) != 0;
        var verticalMirroring = (controlByte1 & 0b1) != 0;
        var mirroring = (fourScreen, verticalMirroring) switch
        {
            (true, _) => ScreenMirroring.FourScreen,
            (false, true) => ScreenMirroring.Vertical,
            (false, false) => ScreenMirroring.Horizontal,
        };

        var prgRomSize = input[4] * PrgRomPageSize;
        var chrRomSize = input[5] * ChrRomPageSize;

        var skipTrainer = (controlByte1 & 0b100) != 0;

        var prgRomStart = 16 + (skipTrainer ? 512 : 0);
        var chrRomStart = prgRomStart + prgRomSize;

        return new Rom(input[prgRomStart..(prgRomStart + prgRomSize)],
            input[chrRomStart..(chrRomStart + chrRomSize)],
            mapper,
            mirroring);
    }

    public static Rom Empty { get; } = new(Array.Empty<byte>(), Array.Empty<byte>(), 0, ScreenMirroring.Horizontal);
    
}

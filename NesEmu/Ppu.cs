namespace NesEmu;

public class Ppu
{
    private ImmutableArray<byte> chrRom = ImmutableArray<byte>.Empty;
    private readonly byte[] paletteTable = new byte[32];
    private readonly byte[] vRam = new byte[2048];
    private readonly byte[] oamData = new byte[256];
    private ScreenMirroring mirroring = ScreenMirroring.Vertical;

    private AddressRegister addressRegister = new AddressRegister();

    public void Load(Rom rom)
    {
        chrRom = rom.ChrRom;
        mirroring = rom.Mirroring;
    }

    public byte PpuAddr
    {
        set => addressRegister.Update(value);
    }

    private byte memBuffer = 0;

    public byte PpuData
    {
        get
        {
            var val = memBuffer;
            memBuffer = VRamRead(addressRegister.Value);
            return memBuffer;
        }
        set { return; }
    }


    public void VRamWrite(ushort address, byte value)
    {
        vRam[address - 0x2000] = value;
    }

    public byte VRamRead(ushort address)
    {
        return address switch
        {
            < 0x2000 => chrRom.Length == 0 ? (byte)0 : chrRom[address],
            >= 0x2000 and < 0x3F00 => vRam[(address - 0x2000) % 0x800],
            _ => 0,
        };
    }

    private class AddressRegister
    {
        private byte valueHi;
        private byte valueLo;

        private bool updateHi = true;

        public ushort Value
        {
            get => (ushort)((valueHi << 8) + valueLo);
            set
            {
                valueLo = (byte)value;
                valueHi = (byte)(value >> 8);
            }
        }

        public void Update(byte value)
        {
            if (updateHi)
            {
                valueHi = value;
            }
            else
            {
                valueLo = value;
            }

            if (Value > 0x3fff)
            {
                Value = (ushort)(Value & 0b11111111111111);
            }

            updateHi = !updateHi;
        }
    }
}

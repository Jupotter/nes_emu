namespace NesEmu;

public class Ppu
{
    [Flags]
    public enum ControlRegisterFlags : byte
    {
        None = 0,
        NameTable1 = 0b00000001,
        NameTable2 = 0b00000010,
        VRamAddIncrement = 0b00000100,
        SpritePatternAddr = 0b00001000,
        BackgroundPatternAddr = 0b00010000,
        SpriteSize = 0b00100000,
        MasterSlaveSelect = 0b01000000,
        GenerateNmi = 0b10000000,
    }

    private ImmutableArray<byte> chrRom = ImmutableArray<byte>.Empty;
    private readonly byte[] paletteTable = new byte[32];
    private readonly byte[] vRam = new byte[2048];
    private readonly byte[] oamData = new byte[256];
    private ScreenMirroring mirroring = ScreenMirroring.Vertical;

    private readonly AddressRegister addressRegister = new();
    
    public ControlRegisterFlags ControlRegister { get; set; }

    public byte PpuAddr
    {
        set => addressRegister.Update(value);
    }

    public ushort ReadAddress => addressRegister.Value;

    private byte memBuffer = 0;

    public byte PpuData
    {
        get
        {
            var address = addressRegister.Value;
            IncrementAddress();
            return Read(address);
        }
        set { return; }
    }

    public void IncrementAddress()
    {
        addressRegister.Increment(HasFlag(ControlRegisterFlags.VRamAddIncrement) ? (byte)32 : (byte)1);
    }

    public void VRamWrite(ushort address, byte value)
    {
        vRam[address - 0x2000] = value;
    }

    public void PaletteWrite(byte address, byte value)
    {
        paletteTable[address] = value;
    }
    

    private byte Read(ushort address)
    {
        byte val;
        switch (address)
        {
            case < 0x2000:
                val = memBuffer;
                memBuffer = ChrRomRead(address);
                return val;
            case >= 0x2000 and < 0x3f00:
                val = memBuffer;
                memBuffer = VRamRead(address);
                return val;
            case >= 0x3f00 and < 0x3fff:
                return paletteTable[(address - 0x3f00)];
            default:
                return 0;
        }
    }

    private byte ChrRomRead(int address)
    {
        return chrRom.Length == 0 ? (byte)0 : chrRom[address];
    }

    private byte VRamRead(int address)
    {
        return vRam[address % 0x800];
    }
    
    public void Load(Rom rom)
    {
        chrRom = rom.ChrRom;
        mirroring = rom.Mirroring;
    }

    private bool HasFlag(ControlRegisterFlags flag)
    {
        return (ControlRegister & flag) != 0;
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
                Value &= 0b11111111111111;
            }

            updateHi = !updateHi;
        }

        public void Increment(byte increment)
        {
            var lo = valueLo;
            valueLo += increment;
            if (lo > valueLo)
            {
                valueHi += 1;
            }

            if (Value > 0x3bff)
            {
                Value &= 0b11111111111111;
            }
        }
    }
}

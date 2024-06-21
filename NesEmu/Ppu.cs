using System.Diagnostics;
using System.Runtime.CompilerServices;

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

    [Flags]
    public enum MaskRegisterFlags : byte
    {
        None = 0,
        Greyscale = 1 << 0,
        ShowBackgroundLeft = 1 << 1,
        ShowSpritesLeft = 1 << 2,
        ShowBackground = 1 << 3,
        ShowSprites = 1 << 4,
        EmphasizeRed = 1 << 5,
        EmphasizeGreen = 1 << 6,
        EmphasizeBlue = 1 << 7,
    }

    [Flags]
    public enum StatusRegisterFlags : byte
    {
        VBlank = 1 << 7,
        Sprite0Hit = 1 << 6,
        SpriteOverflow = 1 << 5,
        Unused = 0b0001_1111,
    }
    
    private readonly AddressRegister addressRegister = new();

    private readonly byte[] oamData = new byte[256];
    private readonly byte[] paletteTable = new byte[32];
    private readonly byte[] vRam = new byte[2048];


    private ImmutableArray<byte> chrRom = ImmutableArray<byte>.Empty;

    private byte memBuffer = 0;
    private ScreenMirroring mirroring = ScreenMirroring.Vertical;
    private StatusRegisterFlags ppuStatus;

    private byte scrollX;
    private byte scrollY;
    
    public Frame Frame { get; } = new ();

    public ControlRegisterFlags ControlRegister { get; set; }
    public MaskRegisterFlags PpuMask { get; set; }

    public StatusRegisterFlags PpuStatus
    {
        get
        {
            var value = ppuStatus;
            ppuStatus &= ~StatusRegisterFlags.VBlank;
            addressRegister.WriteLatch = true;
            return value;
        }
        private set => ppuStatus = value;
    }

    public byte OamAddr { get; set; }

    public byte OamData
    {
        get => oamData[OamAddr];
        set => oamData[OamAddr] = value;
    }

    public byte PpuScroll
    {
        set
        {
            if (addressRegister.WriteLatch)
            {
                scrollX = value;
            }
            else
            {
                scrollY = value;
            }
            addressRegister.WriteLatch = !addressRegister.WriteLatch;
        }
    }

    public byte PpuAddr
    {
        set => addressRegister.Update(value);
    }

    public ushort ReadAddress => addressRegister.Value;

    public byte PpuData
    {
        get
        {
            var address = addressRegister.Value;
            IncrementAddress();
            return Read(address);
        }
        set
        {
            var address = addressRegister.Value;
            IncrementAddress();
            Write(address, value);
        }
    }

    public int Cycles { get; private set; }
    public int ScanLine { get; private set; }
    public int FrameNumber { get; private set; }

    public event Action? GenerateNmi;

    private void IncrementAddress()
    {
        addressRegister.Increment(ControlRegister.HasFlag(ControlRegisterFlags.VRamAddIncrement) ? (byte)32 : (byte)1);
    }

    public void VRamWrite(ushort address, byte value)
    {
        address = GetMirroredVRamAddress(address);
        vRam[address] = value;
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
                Debug.Write($"Tried to read from unsupported address {address}");
                return 0;
        }
    }

    public byte DebugRead()
    {
        return DirectRead(addressRegister.Value);
    }

    public byte DirectRead(ushort address)
    {
        switch (address)
        {
            case < 0x2000:
                return ChrRomRead(address);
            case >= 0x2000 and < 0x3f00:
                return VRamRead(address);
                ;
            case >= 0x3f00 and < 0x3fff:
                return paletteTable[(address - 0x3f00)];
            default:
                Debug.Write($"Tried to read from unsupported address {address}");
                return 0;
        }
    }

    private void Write(ushort address, byte value)
    {
        switch (address)
        {
            case < 0x2000:
                Debug.Write($"Tried to write to CHR Rom address {address}");
                return;
            case >= 0x2000 and < 0x3f00:
                VRamWrite(address, value);
                break;
            case >= 0x3f00 and < 0x3fff:
                PaletteWrite((byte)(address - 0x3f00), value);
                break;
            default:
                Debug.Write($"Tried to write to unsupported {address}");
                break;
        }
    }

    private byte ChrRomRead(ushort address)
    {
        return chrRom.Length == 0 ? (byte)0 : chrRom[address];
    }

    private byte VRamRead(ushort address)
    {
        address = GetMirroredVRamAddress(address);
        return vRam[address];
    }

    /// <summary>
    /// Convert a Ppu bus address into a mirrored vram address depending on the screen mirroring configuration
    /// </summary>
    /// VRam:
    ///   [ A ] 0x0000
    ///   [ B ] 0x0400
    ///
    /// Screen: 
    /// Horizontal:
    ///   [ A ] 0x2000 [ a ] 0x2400
    ///   [ B ] 0x2800 [ b ] 0x2c00
    /// Vertical:
    ///   [ A ] [ B ] 
    ///   [ a ] [ b ]
    private ushort GetMirroredVRamAddress(ushort address)
    {
        var mirrored = address & 0x2fff; // mirror down 0x3000-0x3eff to 0x2000 - 0x2eff
        var vRamIndex = mirrored - 0x2000;
        var nameTableNum = vRamIndex / 0x400;

        return (ushort)((mirroring, nameTableNum) switch
        {
            (ScreenMirroring.Vertical, 2) => vRamIndex - 0x800,
            (ScreenMirroring.Vertical, 3) => vRamIndex - 0x800,
            (ScreenMirroring.Horizontal, 1) => vRamIndex - 0x400,
            (ScreenMirroring.Horizontal, 2) => vRamIndex - 0x400,
            (ScreenMirroring.Horizontal, 3) => vRamIndex - 0x800,
            _ => vRamIndex
        });
    }

    public void Steps(int cycles)
    {
        Cycles += cycles;
        if (Cycles < 341)
        {
            return;
        }

        Cycles -= 341;
        ScanLine += 1;

        if (ScanLine == 241)
        {
            PpuStatus |= StatusRegisterFlags.VBlank;
            if (ControlRegister.HasFlag(ControlRegisterFlags.GenerateNmi))
            {
                RenderFrame();
                GenerateNmi?.Invoke();
            }
        }

        if (ScanLine >= 262)
        {
            FrameNumber += 1;
            ScanLine = 0;
            PpuStatus &= ~StatusRegisterFlags.VBlank;
        }
    }

    public void Load(Rom rom)
    {
        chrRom = rom.ChrRom;
        mirroring = rom.Mirroring;
    }

    private void RenderFrame()
    {
        var bank = (ControlRegister & ControlRegisterFlags.BackgroundPatternAddr) == 0 ? 0 : 1;

        for (int i = 0; i < 0x03c0; i++)
        {
            var tileNum = vRam[i];
            var tileColumn = i % 32;
            var tileRow = i / 32;
            
            DrawTile(bank, tileNum, tileColumn, tileRow);
        }
    }
    
    private void DrawTile(int bank, ushort tileN, int tileColumn, int tileRow)
    {
        if (chrRom.Length == 0)
            return;

        bank = (ushort)(bank * 0x1000);

        var palette = GetBackgroundPalette(tileColumn, tileRow);
        var tile = chrRom.Slice(bank + tileN * 16, 16);

        for (int y = 0; y < 8; y++)
        {
            var upper = tile[y];
            var lower = tile[y + 8];
            for (int x = 8 - 1; x >= 0; x--)
            {
                var value = (1 & lower) << 1 | (1 & upper);
                upper >>= 1;
                lower >>= 1;

                Debug.Assert(value < 4, "Tile value should be between 0 and 4");
                var rgb = value switch
                {
                    0 => Frame.SystemPalette[paletteTable[0]],
                    1 => Frame.SystemPalette[palette[0]],
                    2 => Frame.SystemPalette[palette[1]],
                    3 => Frame.SystemPalette[palette[2]],
                    _ => throw new InvalidOperationException("Tile value should be between 0 and 4")
                };

                Frame[tileRow * 8 + y, tileColumn * 8 + x] = new Frame.PaletteColor(rgb);
            }
        }
    }

    private byte[] GetBackgroundPalette(int tileCol, int tileRow)
    {
        var attrTableIndex = tileRow / 4 * 8 + tileCol / 4;
        var attrByte = vRam[0x3c0 + attrTableIndex];
        var paletteIndex = (tileCol % 4 / 2, tileRow % 4 / 2) switch
        {
            (0, 0) => attrByte & 0b11,
            (1, 0) => (attrByte >> 2) & 0b11,
            (0, 1) => (attrByte >> 4) & 0b11,
            (1, 1) => (attrByte >> 6) & 0b11,
            _ => throw new InvalidOperationException("Should not happen")
        };

        var paletteStart = 1 + paletteIndex * 4;
        return paletteTable[paletteStart..(paletteStart + 3)];

    }
    
    public string GetTrace()
    {
        return $"PPU:{ScanLine,3},{Cycles,3}";
    }

    private class AddressRegister
    {
        private byte valueHi;
        private byte valueLo;

        public bool WriteLatch { get; set; } = true;

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
            if (WriteLatch)
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

            WriteLatch = !WriteLatch;
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

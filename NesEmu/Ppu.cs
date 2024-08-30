using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace NesEmu;

public class Ppu
{
    public const ushort PaletteStart = 0x3f00;
    public const ushort PaletteEnd = 0x3fff;
    public const ushort VRamStart = 0x2000;
    private const int TileDataLength = 16;
    private const int NameTableSize = 0x3c0;

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

    public byte ScrollX => scrollX;
    public byte ScrollY => scrollY;
    public ScreenMirroring Mirroring => mirroring;

    public Frame Frame { get; } = new();

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
        address = address switch
        {
            0x10 => 0x00,
            0x14 => 0x04,
            0x18 => 0x08,
            0x1C => 0x0C,
            _ => address,
        };

        paletteTable[address] = value;
    }

    public byte PaletteRead(byte address)
    {
        address = (address % 0x20) switch
        {
            0x10 => 0x00,
            0x14 => 0x04,
            0x18 => 0x08,
            0x1C => 0x0C,
            var val => (byte)val,
        };

        return paletteTable[address];
    }

    private byte Read(ushort address)
    {
        byte val;
        switch (address)
        {
            case < VRamStart:
                val = memBuffer;
                memBuffer = ChrRomRead(address);
                return val;
            case >= VRamStart and < PaletteStart:
                val = memBuffer;
                memBuffer = VRamRead(address);
                return val;
            case >= PaletteStart and < PaletteEnd:
                return PaletteRead((byte)(address - PaletteStart));
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
            case < VRamStart:
                return ChrRomRead(address);
            case >= VRamStart and < PaletteStart:
                return VRamRead(address);
                ;
            case >= PaletteStart and < PaletteEnd:
                return PaletteRead((byte)(address - PaletteStart));
            default:
                Debug.Write($"Tried to read from unsupported address {address}");
                return 0;
        }
    }

    private void Write(ushort address, byte value)
    {
        switch (address)
        {
            case < VRamStart:
                Debug.Write($"Tried to write to CHR Rom address {address}");
                return;
            case >= VRamStart and < PaletteStart:
                VRamWrite(address, value);
                break;
            case >= PaletteStart and < PaletteEnd:
                PaletteWrite((byte)((address - PaletteStart) % 32), value);
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
        var vRamIndex = mirrored - VRamStart;
        var nameTableNum = vRamIndex / 0x400;

        return (ushort)((Mirroring, nameTableNum) switch
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

        if (IsSprite0Hit(Cycles))
        {
            PpuStatus |= StatusRegisterFlags.Sprite0Hit;
        }

        if (Cycles < 341)
        {
            return;
        }


        Cycles -= 341;
        ScanLine += 1;

        if (ScanLine == 241)
        {
            PpuStatus |= StatusRegisterFlags.VBlank;
            PpuStatus &= ~StatusRegisterFlags.Sprite0Hit;
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
            PpuStatus &= ~StatusRegisterFlags.Sprite0Hit;
        }
    }

    private bool IsSprite0Hit(int cycles)
    {
        var y = oamData[0];
        var x = oamData[3];

        return y == ScanLine && x <= cycles && PpuMask.HasFlag(MaskRegisterFlags.ShowSprites);
    }

    public void Load(Rom rom)
    {
        chrRom = rom.ChrRom;
        mirroring = rom.Mirroring;
    }


    private void RenderFrame()
    {
        var bank = (ControlRegister & ControlRegisterFlags.BackgroundPatternAddr) == 0 ? 0 : 0x1000;

        if (PpuMask.HasFlag(MaskRegisterFlags.ShowBackground))
        {
            var nameTableAddress =
                (int)(ControlRegister & (ControlRegisterFlags.NameTable1 | ControlRegisterFlags.NameTable2));
            var (firstNameTable, secondNameTable) = (Mirroring, nameTableAddress) switch
            {
                (ScreenMirroring.Vertical, 0) => (0, 0x400),
                (ScreenMirroring.Vertical, 2) => (0, 0x400),
                (ScreenMirroring.Vertical, 1) => (0x400, 0),
                (ScreenMirroring.Vertical, 3) => (0x400, 0),
                (ScreenMirroring.Horizontal, 0) => (0, 0x400),
                (ScreenMirroring.Horizontal, 1) => (0, 0x400),
                (ScreenMirroring.Horizontal, 2) => (0x400, 0),
                (ScreenMirroring.Horizontal, 3) => (0x400, 0),
                _ => throw new NotImplementedException(),
            };
            RenderNameTable((ushort)firstNameTable, -scrollX, -scrollY);
            if (mirroring == ScreenMirroring.Vertical)
            {
                RenderNameTable((ushort)secondNameTable, 256 - scrollX, -scrollY);
            }
            else
            {
                RenderNameTable((ushort)secondNameTable, -scrollX, 240 - scrollY);
            }
        }

        if (PpuMask.HasFlag(MaskRegisterFlags.ShowSprites))
        {
            for (int i = 0; i < 64; i++)
            {
                DrawSprite(i);
            }
        }
    }

    private void DrawSprite(int spriteIdx)
    {
        var oamIndex = spriteIdx * 4;
        var tileY = oamData[oamIndex];
        var tileX = oamData[oamIndex + 3];
        var tileN = oamData[oamIndex + 1];

        var palette = GetSpritePalette(oamData[oamIndex + 2] & 0b11);
        var bank = (ControlRegister & ControlRegisterFlags.SpritePatternAddr) == 0 ? 0 : 1;
        bank = (ushort)(bank * 0x1000);

        var flipVertical = (oamData[oamIndex + 2] >> 7 & 1) == 1;
        var flipHorizontal = (oamData[oamIndex + 2] >> 6 & 1) == 1;

        var tile = chrRom.AsSpan(bank + tileN * TileDataLength, TileDataLength);

        for (int y = 0; y < 8; y++)
        {
            var upper = tile[y];
            var lower = tile[y + 8];
            for (int x = 8 - 1; x >= 0; x--)
            {
                var value = (1 & lower) << 1 | (1 & upper);
                upper >>= 1;
                lower >>= 1;

                if (value == 0)
                    continue;
                Debug.Assert(value < 4, "Tile value should be between 0 and 4");
                var rgb = value switch
                {
                    1 => Frame.SystemPalette[palette[0]],
                    2 => Frame.SystemPalette[palette[1]],
                    3 => Frame.SystemPalette[palette[2]],
                    _ => throw new InvalidOperationException("Tile value should be between 0 and 4")
                };

                var drawX = flipHorizontal ? tileX + 7 - x : tileX + x;
                var drawY = flipVertical ? tileY + 7 - y : tileY + y;

                Frame[drawY, drawX] = new Frame.PaletteColor(rgb);
            }
        }
    }

    private void RenderNameTable(ushort nameTableBase, int shiftX, int shiftY)
    {
        // nameTableBase = GetMirroredVRamAddress(nameTableBase);
        var bank = (ControlRegister & ControlRegisterFlags.BackgroundPatternAddr) == 0 ? 0 : 0x1000;

        for (int i = 0; i < NameTableSize; i++)
        {
            var tileNum = vRam[nameTableBase + i];
            var tileColumn = i % 32;
            var tileRow = i / 32;

            var palette = GetBackgroundPalette(nameTableBase, tileColumn, tileRow);
            DrawTile(bank, tileNum, palette, tileColumn * 8 + shiftX, tileRow * 8 + shiftY);
        }
    }

    private void DrawTile(int bank, ushort tileIndex, Span<byte> palette, int tileColumn, int tileRow)
    {
        var tile = chrRom.AsSpan(bank + tileIndex * TileDataLength, TileDataLength);

        for (int y = 0; y < 8; y++)
        {
            var upper = tile[y];
            var lower = tile[y + 8];
            for (int x = 8 - 1; x >= 0; x--)
            {
                var pixelX = tileColumn + x;
                var pixelY = tileRow + y;
                if (0 > pixelX || pixelX >= Frame.Width
                               || 0 > pixelY || pixelY >= Frame.Height)
                    continue;

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

                Frame[pixelY, pixelX] = new Frame.PaletteColor(rgb);
            }
        }
    }

    private Span<byte> GetBackgroundPalette(int nameTableBase, int tileCol, int tileRow)
    {
        var attrTableIndex = tileRow / 4 * 8 + tileCol / 4;
        var attrByte = vRam[nameTableBase + NameTableSize + attrTableIndex];
        var paletteIndex = (tileCol % 4 / 2, tileRow % 4 / 2) switch
        {
            (0, 0) => attrByte & 0b11,
            (1, 0) => (attrByte >> 2) & 0b11,
            (0, 1) => (attrByte >> 4) & 0b11,
            (1, 1) => (attrByte >> 6) & 0b11,
            _ => throw new InvalidOperationException("Should not happen")
        };

        var paletteStart = 1 + paletteIndex * 4;
        return paletteTable.AsSpan(paletteStart, 3);
    }

    private Span<byte> GetSpritePalette(int paletteIdx)
    {
        var paletteStart = 0x11 + (paletteIdx * 4);
        return paletteTable.AsSpan(paletteStart, 3);
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

            if (Value > PaletteEnd)
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

    public void WriteOamDma(Span<byte> ramPage)
    {
        ramPage.CopyTo(oamData);
    }
}

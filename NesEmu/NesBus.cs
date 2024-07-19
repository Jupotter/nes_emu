using System.Diagnostics;

namespace NesEmu;

public interface IBus
{
    byte MemRead(ushort address);
    void MemWrite(ushort address, byte value);

    void Load(ushort address, byte[] data);

    void Load(Rom newRom);
    bool CanDebugRead(ushort address);
}

public class NesBus : IBus
{
    private const ushort RamStart = 0x0000;
    private const ushort RamEnd = 0x7ff;
    private const ushort RamMirrorsEnd = 0x1FFF;
    private const ushort PpuRegisterStart = 0x2000;
    private const ushort PpuRegisterMirrorsEnd = 0x3fff;
    private const ushort RomStart = 0x8000;

    private readonly byte[] mainRam = new byte[0x800];
    private Rom rom;
    private readonly Ppu ppu;
    
    public Joypad? Joypad1 { get; set; }
    public Joypad? Joypad2 { get; set; }

    public NesBus(Rom rom, Ppu ppu)
    {
        this.rom = rom;
        this.ppu = ppu;
    }

    public bool CanDebugRead(ushort address)
    {
        return address switch
        {
            >= RamStart and <= RamMirrorsEnd => true,
            >= PpuRegisterStart and <= PpuRegisterMirrorsEnd => false,
            0x4014 => false,
            > RomStart => true,
            _ => false
        };
    }
    
    public byte MemRead(ushort address)
    {
        switch (address)
        {
            case >= RamStart and <= RamMirrorsEnd:
            {
                var realAddress = (ushort)(address & RamEnd);
                var value = mainRam[realAddress];
                return value;
            }
            case >= PpuRegisterStart and <= PpuRegisterMirrorsEnd:
            {
                return ReadPpuRegister(address);
            }
            case 0x4014:
                throw new InvalidOperationException($"Attempted to read from a write only address {address}");
            case 0x4016:
                return Joypad1?.Read() ?? 0;
            case 0x4017:
                return Joypad2?.Read() ?? 0;
            case > RomStart:
            {
                return ReadPrgRom(address);
            }
            default:
                return 0;
        }
    }

    private byte ReadPpuRegister(ushort address)
    {
        var realAddress = address & 0x2007;
        switch (realAddress)
        {
            case 0x2000:
            case 0x2001:
            case 0x2003:
            case 0x2005:
            case 0x2006:
                throw new InvalidOperationException($"Attempted to read from a write only address {address}");
            case 0x2002:
                return (byte)ppu.PpuStatus;
            case 0x2004:
                return ppu.OamData;
            case 0x2007:
                return ppu.PpuData;
            default:
                throw new ArgumentOutOfRangeException(nameof(address), address, "Not a ppu address");
        }
    }

    private byte ReadPrgRom(ushort address)
    {
        var realAddress = address & 0x7fff;
        if (realAddress >= 0x4000 && rom.PrgRom.Length == 0x4000)
            realAddress %= 0x4000;
        return rom.PrgRom[realAddress];
    }

    public void MemWrite(ushort address, byte value)
    {
        switch (address)
        {
            case >= RamStart and <= RamMirrorsEnd:
            {
                var realAddress = (ushort)(address & RamEnd);
                mainRam[realAddress] = value;
                break;
            }
            case >= PpuRegisterStart and <= PpuRegisterMirrorsEnd:
                WritePpuRegister(address, value);
                break;
            case 0x4014:
                ppu.WriteOamDma(mainRam.AsSpan(value << 8, 0xff));
                break;
            case 0x4016:
                Joypad1?.Write(value);
                Joypad2?.Write(value);
                break;
            case 0x4017:
                break;

            case > RomStart:
            {
                throw new InvalidOperationException($"Tried to write to ROM address {address}");
            }
        }
    }

    private void WritePpuRegister(ushort address, byte value)
    {
        var realAddress = address & 0x2007;
        switch (realAddress)
        {
            case 0x2000:
                ppu.ControlRegister = (Ppu.ControlRegisterFlags)value;
                return;
            case 0x2001:
                ppu.PpuMask = (Ppu.MaskRegisterFlags)value;
                return;
            case 0x2006:
                ppu.PpuAddr = value;
                return;
            case 0x2003:
                ppu.OamAddr = value;
                return;
            case 0x2004:
                ppu.OamData = value;
                return;
            case 0x2005:
                ppu.PpuScroll = value;
                return;
            case 0x2007:
                ppu.PpuData = value;
                return;
            case 0x2002:
                throw new InvalidOperationException($"Attempted write to read only PPU Register {address}");
            default:
                throw new ArgumentOutOfRangeException(nameof(address), address, "Not a ppu address");
        }
    }

    public void Load(ushort address, byte[] data)
    {
        if (address + data.Length > RamEnd)
            throw new InvalidOperationException($"Cannot load outside RAM to address {address}");
        data.CopyTo(mainRam, address);
    }

    public void Load(Rom newRom)
    {
        rom = newRom;
        ppu.Load(rom);
    }
}

namespace NesEmu;

public interface IBus
{
    byte MemRead(ushort address);
    void MemWrite(ushort address, byte value);

    void Load(ushort address, byte[] data);
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
    private readonly Rom rom;

    public NesBus(Rom rom)
    {
        this.rom = rom;
    }

    public byte MemRead(ushort address)
    {
        switch (address)
        {
            case > RamStart and < RamMirrorsEnd:
            {
                var realAddress = (ushort)(address & RamEnd);
                return mainRam[realAddress];
            }
            case > RomStart:
            {
                return ReadPrgRom(address);
            }
            default:
                return 0;
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
            case > RamStart and < RamMirrorsEnd:
            {
                var realAddress = (ushort)(address & RamEnd);
                mainRam[realAddress] = value;
                break;
            }
            
            case > RomStart:
            {
                throw new InvalidOperationException($"Tried to write to ROM address {address}");
            }
        }
    }

    public void Load(ushort address, byte[] data)
    {
        if (address + data.Length > RamEnd)
            throw new InvalidOperationException($"Cannot load outside RAM to address {address}");
        data.CopyTo(mainRam, address);
    }
}

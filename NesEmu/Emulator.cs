namespace NesEmu;

public class Emulator
{
    public Ppu Ppu;
    public IBus Bus { get; }
    public Cpu Cpu { get; }
    public Rom Rom { get; private set; }

    private bool running = false;

    public static Emulator Initialize()
    {
        return new Emulator();
    }

    private Emulator()
    {
        var ppu = new Ppu();
        var bus = new NesBus(Rom.Empty, ppu);
        var cpu = new Cpu(bus);
        Ppu = ppu;
        Cpu = cpu;
        Bus = bus;
        Rom = Rom.Empty;
    }

    public void LoadRom(Rom rom)
    {
        Rom = rom;
        Bus.Load(rom);
        Cpu.Reset();
    }
}

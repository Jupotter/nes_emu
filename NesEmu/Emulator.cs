namespace NesEmu;

public class Emulator
{
    public Ppu Ppu;
    public IBus Bus { get; }
    public Cpu CPU { get; }

    private bool running = false;
    
    public static Emulator Initialize()
    {
        var ppu = new Ppu();
        var bus = new NesBus(Rom.Empty, ppu);
        var cpu = new Cpu(bus);
        return new Emulator(cpu, bus, ppu);
    }

    private Emulator(Cpu cpu, IBus bus, Ppu ppu)
    {
        Ppu = ppu;
        CPU = cpu;
        Bus = bus;
    }
}

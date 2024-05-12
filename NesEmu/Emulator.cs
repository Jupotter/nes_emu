namespace NesEmu;

public class Emulator
{
    public IBus Bus { get; }
    public Cpu CPU { get; }

    private bool running = false;
    
    public static Emulator Initialize()
    {
        var bus = new NesBus(Rom.Empty, new Ppu());
        var cpu = new Cpu(bus);
        return new Emulator(cpu, bus);
    }

    private Emulator(Cpu cpu, IBus bus)
    {
        CPU = cpu;
        Bus = bus;
    }
}

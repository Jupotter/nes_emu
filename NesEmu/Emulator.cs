namespace NesEmu;

public class Emulator
{
    public Ppu Ppu;
    public IBus Bus { get; }
    public Cpu Cpu { get; }
    public Rom Rom { get; private set; }

    public int Cycles { get; private set; } = 7;

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
        ppu.GenerateNmi += InterruptCpuHandler;
    }

    public void LoadRom(Rom rom)
    {
        Rom = rom;
        Bus.Load(rom);
        Reset();
    }

    public string GetTrace()
    {
        var cpuStatus = Cpu.GetTrace();
        var ppuStatus = Ppu.GetTrace();

        return $"{cpuStatus} {ppuStatus} CYC:{Cycles}";
    }

    public void Reset()
    {
        Cpu.Reset();
        Cycles = 0;
        RunCycles(7);
    }
    
    public bool Step()
    {
        var (brk, cycles) = Cpu.Step();

        RunCycles(cycles);
        
        return brk;
    }

    private void RunCycles(int cycles)
    {
        Ppu.Steps(cycles*3);
        Cycles += cycles;
    }

    private void InterruptCpuHandler()
    {
        Cpu.InterruptNmi();
        RunCycles(2);
    }
}

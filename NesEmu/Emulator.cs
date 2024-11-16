namespace NesEmu;

public class Emulator
{
    private const double CpuClockPerSecond = 1_789_773.0;
    
    public Ppu Ppu { get; } = new();
    public IBus Bus { get; }
    public Cpu Cpu { get; }
    public Apu Apu { get; } = new();
    public Rom Rom { get; private set; }
    
    public Joypad Joypad1 { get; } = new Joypad();
    public Joypad Joypad2 { get; } = new Joypad();

    public int Cycles { get; private set; } = 7;
    
    public double AudioTimePerSystemSample { get; private set; }
    public double AudioTimePerNesClock { get; private set; }
    public double AudioTime { get; private set; }

    
    public static Emulator Initialize()
    {
        return new Emulator();
    }

    private Emulator()
    {
        var bus = new NesBus(Rom.Empty, Ppu, Apu);
        Cpu = new Cpu(bus);
        Bus = bus;
        Rom = Rom.Empty;

        bus.Joypad1 = Joypad1;
        bus.Joypad2 = Joypad2;
        
        Ppu.GenerateNmi += InterruptCpuHandler;
    }

    public void LoadRom(Rom rom)
    {
        Rom = rom;
        Bus.Load(rom);
        Reset();
    }

    public void SetSampleFrequency(int sampleRate)
    {
        AudioTimePerSystemSample = 1.0 / sampleRate;
        AudioTimePerNesClock = 1.0 / CpuClockPerSecond;
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
    
    public (bool brk, bool audioReady)  Step()
    {
        var (brk, cycles) = Cpu.Step();


        RunCycles(cycles);

        var audioReady = false;
        AudioTime += AudioTimePerNesClock*cycles;
        if (AudioTime >= AudioTimePerSystemSample)
        {
            AudioTime -= AudioTimePerSystemSample;
            // Get audio 
            audioReady = true;
        }
        
        return (brk, audioReady);
    }

    private void RunCycles(int cycles)
    {
        Ppu.Steps(cycles*3);
        Apu.Steps(cycles);
        Cycles += cycles;
    }

    private void InterruptCpuHandler()
    {
        Cpu.InterruptNmi();
        RunCycles(2);
    }

    public double GetAudioSample()
    {
        return Apu.GetCombinedAudioSample();
    }
}

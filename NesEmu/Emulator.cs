namespace NesEmu;

public class Emulator
{
    public NesBus Bus { get; }
    public Cpu Cpu { get; }

    public Emulator()
    {
        Bus = new NesBus();
        Cpu = new Cpu(Bus);
    }
}

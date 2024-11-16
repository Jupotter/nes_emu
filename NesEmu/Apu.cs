using System.Runtime.InteropServices.Marshalling;

namespace NesEmu;

public class Apu
{
    private bool pulse1Enable = false;
    private double pulse1Sample = 0;
    

    public void Write(ushort address, byte value)
    {
        
    }

    public void Steps(int cycles)
    {
        
    }

    public double GetCombinedAudioSample()
    {
        return 0f;
    }

    public void Reset()
    {
        
    }
}

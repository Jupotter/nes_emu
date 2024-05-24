using ImGuiNET;

namespace NesEmu.UI;

public class PpuWindow: IElement
{
    private readonly Ppu ppu;

    public PpuWindow(Ppu ppu)
    {
        this.ppu = ppu;
    }

    public void NewFrame()
    {
        if (!ImGui.Begin("Pixel Processing Unit"))
            return;
        
        ImGui.Value("Address", ppu.ReadAddress);
        ImGui.Value("Value", ppu.DebugRead());
        
        ImGui.End();
    }
}

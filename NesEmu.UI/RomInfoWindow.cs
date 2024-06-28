using ImGuiNET;

namespace NesEmu.UI;

public class RomInfoWindow : IElement
{
    public Rom Rom { get; set; } = Rom.Empty;

    public void NewFrame()
    {
        if (!ImGui.Begin("Rom Info"))
        {
            return;
        }
        
        ImGui.Value("Mapper", Rom.Mapper);
        ImGui.Value("PrgRom Size", Rom.PrgRom.Length);
        ImGui.Value("ChrRom Size", Rom.ChrRom.Length);
        ImGui.LabelText("Screen Mirroring", Rom.Mirroring.ToString());
            
        ImGui.End();
    }
}

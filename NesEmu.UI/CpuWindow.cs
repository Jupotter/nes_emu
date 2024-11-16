using ImGuiNET;

namespace NesEmu.UI;

public class CpuWindow: IElement
{
    private readonly Emulator emulator;
    public bool Running { get; private set; } = false;
    private int stepPerFrame = 50;

    public CpuWindow(Emulator emulator)
    {
        this.emulator = emulator;
    }

    public void NewFrame()
    {

        if (!ImGui.Begin("CPU Control"))
            return;

        // ImGui.Text(emulator.GetTrace());

        ImGui.BeginDisabled(Running);
        {
            if (ImGui.Button("Reset"))
            {
                emulator.Reset();
            }
            ImGui.SameLine();
            ImGui.BeginDisabled();
            {
                if (ImGui.Button("Step"))
                {
                    // emulator.Step();
                }
                ImGui.EndDisabled();
            }
            ImGui.EndDisabled();
        }
        ImGui.SameLine();
        if (Running)
        {
            if (ImGui.Button("Pause"))
            {
                Running = false;
            }
        }
        else
        {
            if (ImGui.Button("Run"))
            {
                Running = true;
            }
        }

        ImGui.SliderInt("Step per frame", ref stepPerFrame, 1, 100);

        ImGui.End();
    }
}

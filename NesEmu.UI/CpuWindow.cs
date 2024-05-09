using ImGuiNET;

namespace NesEmu.UI;

public class CpuWindow: IElement
{
    private readonly Cpu cpu;
    private bool running = false;
    private int stepPerFrame = 50;

    public CpuWindow(Cpu cpu)
    {
        this.cpu = cpu;
    }

    public void NewFrame()
    {
        if (running)
        {
            for (var i = 0; i < stepPerFrame; i++)
            {
                running = !cpu.Step();
            }
        }

        if (!ImGui.Begin("CPU Control"))
            return;

        ImGui.Text(cpu.ToString());

        ImGui.BeginDisabled(running);
        if (ImGui.Button("Reset"))
        {
            cpu.Reset();
        }
        ImGui.SameLine();
        if (ImGui.Button("Step"))
        {
            cpu.Step();
        }
        ImGui.EndDisabled();
        ImGui.SameLine();
        if (running)
        {
            if (ImGui.Button("Pause"))
            {
                running = false;
            }
        }
        else
        {
            if (ImGui.Button("Run"))
            {
                running = true;
            }
        }

        ImGui.SliderInt("Step per frame", ref stepPerFrame, 1, 100);

        ImGui.End();
    }
}

using ImGuiNET;

namespace NesEmu.UI;

public class CpuWindow: IElement
{
    private readonly Emulator emulator;
    private bool running = false;
    private int stepPerFrame = 50;

    public CpuWindow(Emulator emulator)
    {
        this.emulator = emulator;
    }

    public void NewFrame()
    {
        if (running)
        {
            var currentFrame = emulator.Ppu.FrameNumber;
            var brk = false;
            do
            {
                brk = emulator.Step();
            } while (!brk && emulator.Ppu.FrameNumber == currentFrame);
        }

        if (!ImGui.Begin("CPU Control"))
            return;

        ImGui.Text(emulator.GetTrace());

        ImGui.BeginDisabled(running);
        if (ImGui.Button("Reset"))
        {
            emulator.Reset();
        }
        ImGui.SameLine();
        if (ImGui.Button("Step"))
        {
            emulator.Step();
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

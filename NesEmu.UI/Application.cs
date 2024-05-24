using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ImGuiNET;

namespace NesEmu.UI;

public class Application
{
    private readonly Emulator emulator;

    private readonly List<IElement> displayedElements = [];
    
    private readonly CpuWindow cpuWindow;
    
    public Application(Emulator emulator)
    {
        this.emulator = emulator;
        cpuWindow = new CpuWindow(emulator.CPU);
        displayedElements.Add(cpuWindow);
        displayedElements.Add(new PpuWindow(emulator.Ppu));
        
    }
    
    public void NewFrame()
    {
        ShowMenu();
        foreach (var element in displayedElements)
        {
            element.NewFrame();
        }
    }

    private void ShowMenu()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Samples"))
            {
                if (ImGui.MenuItem("Snake"))
                    LoadSnake();
                ImGui.EndMenu();
            }
            ImGui.EndMainMenuBar();
        }
    }

    private void LoadSnake()
    {
        var snakeRom = File.ReadAllBytes("Roms/snake.nes"); 
        
        emulator.Bus.Load(Rom.Parse(snakeRom));
        emulator.CPU.MemWriteByte(0xff, 0x77);
        emulator.CPU.Reset();
        
        displayedElements.Add(new SnakeDisplay(emulator));
    }
}

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
    private ChrRomWindow? chrRomWindow;

    public Application(Emulator emulator)
    {
        this.emulator = emulator;
        var cpuWindow = new CpuWindow(emulator);
        displayedElements.Add(cpuWindow);
        displayedElements.Add(new PpuWindow(emulator.Ppu));
    }

    public void Initialize()
    {
        chrRomWindow = new ChrRomWindow(emulator);
        displayedElements.Add(chrRomWindow);
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
            if (ImGui.BeginMenu("Files"))
            {
                var romsDirectory = new DirectoryInfo("Roms/NonFree");
                foreach (var file in romsDirectory.GetFiles("*.nes"))
                {
                    if (ImGui.MenuItem(file.Name))
                        LoadFile(file);
                }
                ImGui.EndMenu();
            }
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
        
        emulator.LoadRom(Rom.Parse(snakeRom));
        emulator.Cpu.MemWriteByte(0xff, 0x77);
        emulator.Cpu.Reset();
        
        displayedElements.Add(new SnakeDisplay(emulator));
    }

    private void LoadFile(FileInfo file)
    {
        var romBytes = File.ReadAllBytes(file.FullName);
        var rom = Rom.Parse(romBytes);
        emulator.LoadRom(rom);
        
        chrRomWindow?.UpdateRom(rom);
    }
}

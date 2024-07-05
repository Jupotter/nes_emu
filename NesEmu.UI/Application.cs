using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ImGuiNET;
using SDL2;

namespace NesEmu.UI;

public class Application
{
    private readonly Emulator emulator;

    private readonly List<IElement> displayedElements = [];
    private ChrRomWindow? chrRomWindow;
    private readonly RomInfoWindow romInfoWindow;

    public Application(Emulator emulator)
    {
        this.emulator = emulator;
        var cpuWindow = new CpuWindow(emulator);
        displayedElements.Add(cpuWindow);
        romInfoWindow = new RomInfoWindow();
        displayedElements.Add(romInfoWindow);
    }

    public void Initialize()
    {
        chrRomWindow = new ChrRomWindow(emulator);
        displayedElements.Add(chrRomWindow);
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

    private Dictionary<SDL.SDL_Keycode, Joypad.Button> keymap = new()
    {
        { SDL.SDL_Keycode.SDLK_DOWN, Joypad.Button.Down },
        { SDL.SDL_Keycode.SDLK_UP, Joypad.Button.Up },
        { SDL.SDL_Keycode.SDLK_LEFT, Joypad.Button.Left },
        { SDL.SDL_Keycode.SDLK_RIGHT, Joypad.Button.Right },
        { SDL.SDL_Keycode.SDLK_RETURN, Joypad.Button.Start },
        { SDL.SDL_Keycode.SDLK_SPACE, Joypad.Button.Select },
        { SDL.SDL_Keycode.SDLK_a, Joypad.Button.ButtonA },
        { SDL.SDL_Keycode.SDLK_s, Joypad.Button.ButtonB },
    };

    public void HandleSdlEvent(in SDL.SDL_Event sdlEvent)
    {
        switch (sdlEvent.type)
        {
            case SDL.SDL_EventType.SDL_KEYUP:
            case SDL.SDL_EventType.SDL_KEYDOWN:
                HandleSdlKeyboard(sdlEvent.key);
                return;
            default:
                return;
        }
    }
    
    private void HandleSdlKeyboard(in SDL.SDL_KeyboardEvent keyboardEvent)
    {
        var keycode = keyboardEvent.keysym.sym;

        if (keymap.TryGetValue(keycode, out var button))
        {
            var pressed = keyboardEvent.type == SDL.SDL_EventType.SDL_KEYDOWN;
            emulator.Joypad1.SetButtonStatus(button, pressed);
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

        var rom = Rom.Parse(snakeRom);
        emulator.LoadRom(rom);
        emulator.Cpu.MemWriteByte(0xff, 0x77);
        emulator.Cpu.Reset();
        
        displayedElements.Add(new SnakeDisplay(emulator));

        romInfoWindow.Rom = rom;
    }

    private void LoadFile(FileSystemInfo file)
    {
        var romBytes = File.ReadAllBytes(file.FullName);
        var rom = Rom.Parse(romBytes);
        romInfoWindow.Rom = rom;
        emulator.LoadRom(rom);
        
        chrRomWindow?.UpdateRom(rom);
    }
}

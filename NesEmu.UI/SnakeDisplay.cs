using System;
using System.Numerics;
using ImGuiNET;
using NesEmu.UI.ImGuiSDLRendering;
using SDL2;

namespace NesEmu.UI;

public class SnakeDisplay : IElement
{
    private readonly Emulator emulator;
    private readonly IntPtr renderer;
    private readonly IntPtr texture;
    private int state = 0;

    public SnakeDisplay(Emulator emulator)
    {
        renderer = ImGuiWindow.renderer;
        texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGB24,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, 512, 512);
        this.emulator = emulator;
    }
    
    public void NewFrame()
    {
        if (UpdateGameState())
            UpdateTexture();

        ImGui.Begin("Snake");
        ImGui.Image(texture, new Vector2(512, 512));
        ImGui.End();
    }

    private bool UpdateGameState()
    {
        var oldState = state; 
        emulator.Bus.MemWrite(0xFE, (byte)Random.Shared.Next());
        
        if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.UpArrow)))
        {
            emulator.Bus.MemWrite(0xff, 0x77);
        }
        else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.DownArrow)))
        {
            emulator.Bus.MemWrite(0xff, 0x73);
        }
        else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.LeftArrow)))
        {
            emulator.Bus.MemWrite(0xff, 0x61);
        }
        else if (ImGui.IsKeyPressed(ImGui.GetKeyIndex(ImGuiKey.RightArrow)))
        {
            emulator.Bus.MemWrite(0xff,  0x64);
        }
        
        state = 1;
        for (ushort i = 0x200; i < 0x600; i++)
        {
            state += i * emulator.Bus.MemRead(i);
        }

        return state != oldState;
    }

    private void UpdateTexture()
    {
        SDL.SDL_SetRenderTarget(renderer, texture).ThrowOnError();
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255).ThrowOnError();
        SDL.SDL_RenderFillRect(renderer, IntPtr.Zero).ThrowOnError();

        var rect = new SDL.SDL_Rect { x = 0, y = 0, w = 16, h = 16 };
        for (byte i = 0; i < 32; i++)
        {
            for (byte j = 0; j < 32; j++)
            {
                var color = GetColor(emulator.Bus.MemRead((ushort)(0x0200 + 32 * i + j)));
                SDL.SDL_SetRenderDrawColor(renderer, color.r, color.g, color.b, 255);
                rect.x = j * 16;
                rect.y = i * 16;
                SDL.SDL_RenderDrawRect(renderer, ref rect);
            }
        }
        
        SDL.SDL_SetRenderTarget(renderer, IntPtr.Zero).ThrowOnError();
    }

    private SDL.SDL_Color GetColor(byte b)
    {
        return b switch
        {
            0 => new SDL.SDL_Color { r = 0, g = 0, b = 0, a = 255 },
            1 => new SDL.SDL_Color { r = 255, g = 255, b = 255, a = 255 },
            _ => new SDL.SDL_Color { r = 0, g = 255, b = 255, a = 255 },
        };
    }
}

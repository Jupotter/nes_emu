using System;
using System.Numerics;
using ImGuiNET;
using NesEmu.UI.ImGuiSDLRendering;
using SDL2;

namespace NesEmu.UI;

public class PpuWindow : IElement
{
    private readonly IntPtr renderer;
    private readonly IntPtr texture;

    private readonly Ppu ppu;

    private int lastFrameNumber = 0;

    public PpuWindow(Ppu ppu)
    {
        renderer = ImGuiWindow.renderer;
        texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGB24,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Frame.Width, Frame.Height);

        this.ppu = ppu;
    }

    public void NewFrame()
    {
        if (!ImGui.Begin("Pixel Processing Unit"))
            return;

        if (ppu.FrameNumber != lastFrameNumber)
        {
            RenderFrame();
            lastFrameNumber = ppu.FrameNumber;
        }

        ImGui.Image(texture, new Vector2(Frame.Width, Frame.Height));

        ImGui.SameLine();

        ImGui.BeginGroup();
        {
            ImGui.Value("Address", ppu.ReadAddress);
            ImGui.Value("Value", ppu.DebugRead());
            
            ImGui.LabelText("Control", ppu.ControlRegister.ToString());

            ImGui.Separator();

            ImGui.Value("Cycle", ppu.Cycles);
            ImGui.Value("Scanline", ppu.ScanLine);
            ImGui.Value("Frame", ppu.FrameNumber);
            ImGui.EndGroup();
        }
        ImGui.End();
    }


    private void RenderFrame()
    {
        SDL.SDL_SetRenderTarget(renderer, texture).ThrowOnError();
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255).ThrowOnError();
        SDL.SDL_RenderFillRect(renderer, IntPtr.Zero).ThrowOnError();

        for (var i = 0; i < Frame.Width; i++)
        {
            for (var j = 0; j < Frame.Height; j++)
            {
                var color = PaletteToSDLColor(ppu.Frame[j, i]);
                SDL.SDL_SetRenderDrawColor(renderer, color.r, color.g, color.b, 255);
                SDL.SDL_RenderDrawPoint(renderer, i, j);
            }
        }

        SDL.SDL_SetRenderTarget(renderer, IntPtr.Zero).ThrowOnError();
    }

    private static SDL.SDL_Color PaletteToSDLColor(Frame.PaletteColor paletteColor)
    {
        return new SDL.SDL_Color { r = paletteColor.R, g = paletteColor.G, b = paletteColor.B, a = 255 };
    }
}

using System;
using System.Numerics;
using ImGuiNET;
using NesEmu.UI.ImGuiSDLRendering;
using SDL2;

namespace NesEmu.UI;

public class PaletteWindow : IElement
{
    private const int PaletteCount = 8;
    private const int PixelPerColor = 16;
    private const int ColorPerPalette = 4;

    private readonly IntPtr renderer;
    private readonly IntPtr texture;

    private readonly Ppu ppu;

    private int lastFrameNumber = 0;

    public PaletteWindow(Ppu ppu)
    {
        renderer = ImGuiWindow.renderer;
        texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGB24,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, ColorPerPalette * PixelPerColor, PaletteCount * PixelPerColor);

        this.ppu = ppu;
    }

    public void NewFrame()
    {
        if (!ImGui.Begin("Palette"))
            return;

        if (ppu.FrameNumber != lastFrameNumber)
        {
            RenderFrame();
            lastFrameNumber = ppu.FrameNumber;
        }

        ImGui.Image(texture, new Vector2(ColorPerPalette * PixelPerColor, PaletteCount * PixelPerColor));
    }

    private void RenderFrame()
    {
        SDL.SDL_SetRenderTarget(renderer, texture).ThrowOnError();
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255).ThrowOnError();
        SDL.SDL_RenderFillRect(renderer, IntPtr.Zero).ThrowOnError();

        var rect = new SDL.SDL_Rect { x = 0, y = 0, w = PixelPerColor, h = PixelPerColor };
        for (var i = 0; i < PaletteCount * ColorPerPalette; i++)
        {
            var address = (ushort)(Ppu.PaletteStart + i);
            var value = ppu.DirectRead(address);
            
            var paletteColor = new Frame.PaletteColor(value);
            var color = PaletteToSDLColor(paletteColor);
            SDL.SDL_SetRenderDrawColor(renderer, color.r, color.g, color.b, 255);
            rect.x = (i % ColorPerPalette) * PixelPerColor;
            rect.y = (i / ColorPerPalette) * PixelPerColor;
            SDL.SDL_RenderFillRect(renderer, ref rect);
        }

        SDL.SDL_SetRenderTarget(renderer, IntPtr.Zero).ThrowOnError();
    }

    private static SDL.SDL_Color PaletteToSDLColor(Frame.PaletteColor paletteColor)
    {
        return new SDL.SDL_Color { r = paletteColor.R, g = paletteColor.G, b = paletteColor.B, a = 255 };
    }
}

using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using NesEmu.UI.ImGuiSDLRendering;
using SDL2;

namespace NesEmu.UI;

public class ChrRomWindow : IElement
{
    private readonly IntPtr renderer;
    private readonly IntPtr texture;
    private Rom rom = Rom.Empty;

    private const int Width = 10*16;
    private const int Height = 10*16;

    private readonly Frame.PaletteColor[,] frameData = new Frame.PaletteColor[Height, Width];
    private int activeBank = 0;
    private int zoom = 1;

    public ChrRomWindow(Emulator emulator)
    {
        renderer = ImGuiWindow.renderer;
        texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGB24,
            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET, Width, Height);

        UpdateRom(emulator.Rom);
    }

    public void UpdateRom(Rom newRom)
    {
        rom = newRom;

        ShowTiles();
        RenderFrame();
    }

    public void NewFrame()
    {
        if (!ImGui.Begin("Chr Rom Preview"))
            return;


        ImGui.Image(texture, new Vector2(Width * zoom, Height * zoom));

        ImGui.SameLine();
        bool changed = false;

        ImGui.BeginGroup();
        {
            ImGui.SeparatorText("Tile Bank");
            changed |= ImGui.RadioButton("0", ref activeBank, 0);
            ImGui.SameLine();
            changed |= ImGui.RadioButton("1", ref activeBank, 1);

            ImGui.SeparatorText("Zoom");
            ImGui.RadioButton("1", ref zoom, 1);
            ImGui.SameLine();
            ImGui.RadioButton("2", ref zoom, 2);
            ImGui.EndGroup();
        }
        ImGui.End();

        if (changed)
        {
            ShowTiles();
        }
    }

    private void ShowTiles()
    {
        for (ushort t = 0; t < 256; t++)
        {
            var x = t % 16 * 10;
            var y = t / 16 * 10;
            ShowTile((ushort)activeBank, t, x + 1, y + 1);
        }
        RenderFrame();
    }

    private void ShowTile(ushort bank, ushort tileN, int renderX, int renderY)
    {
        if (rom.ChrRom.Length == 0)
            return;

        bank = (ushort)(bank * 0x1000);

        var tile = rom.ChrRom.Slice(bank + tileN * 16, 16);

        for (int y = 0; y < 8; y++)
        {
            var upper = tile[y];
            var lower = tile[y + 8];
            for (int x = 8 - 1; x >= 0; x--)
            {
                var value = (1 & upper) << 1 | (1 & lower);
                upper >>= 1;
                lower >>= 1;

                Debug.Assert(value < 4, "Tile value should be between 0 and 4");
                var rgb = value switch
                {
                    0 => Frame.SystemPalette[0x02],
                    1 => Frame.SystemPalette[0x23],
                    2 => Frame.SystemPalette[0x27],
                    3 => Frame.SystemPalette[0x30],
                    _ => throw new InvalidOperationException("Tile value should be between 0 and 4")
                };

                frameData[renderY + y, renderX + x] = new Frame.PaletteColor(rgb);
            }
        }
    }


    private void RenderFrame()
    {
        SDL.SDL_SetRenderTarget(renderer, texture).ThrowOnError();
        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255).ThrowOnError();
        SDL.SDL_RenderFillRect(renderer, IntPtr.Zero).ThrowOnError();

        for (var i = 0; i < Width; i++)
        {
            for (var j = 0; j < Height; j++)
            {
                var color = PaletteToSDLColor(frameData[j, i]);
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

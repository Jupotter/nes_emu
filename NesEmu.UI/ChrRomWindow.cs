using System;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using NesEmu.UI.ImGuiSDLRendering;
using SDL2;

namespace NesEmu.UI;

public class ChrRomWindow : IElement
{
    private static readonly (byte R, byte G, byte B)[] SystemPalette =
    [
        (0x80, 0x80, 0x80), (0x00, 0x3D, 0xA6), (0x00, 0x12, 0xB0), (0x44, 0x00, 0x96), (0xA1, 0x00, 0x5E),
        (0xC7, 0x00, 0x28), (0xBA, 0x06, 0x00), (0x8C, 0x17, 0x00), (0x5C, 0x2F, 0x00), (0x10, 0x45, 0x00),
        (0x05, 0x4A, 0x00), (0x00, 0x47, 0x2E), (0x00, 0x41, 0x66), (0x00, 0x00, 0x00), (0x05, 0x05, 0x05),
        (0x05, 0x05, 0x05), (0xC7, 0xC7, 0xC7), (0x00, 0x77, 0xFF), (0x21, 0x55, 0xFF), (0x82, 0x37, 0xFA),
        (0xEB, 0x2F, 0xB5), (0xFF, 0x29, 0x50), (0xFF, 0x22, 0x00), (0xD6, 0x32, 0x00), (0xC4, 0x62, 0x00),
        (0x35, 0x80, 0x00), (0x05, 0x8F, 0x00), (0x00, 0x8A, 0x55), (0x00, 0x99, 0xCC), (0x21, 0x21, 0x21),
        (0x09, 0x09, 0x09), (0x09, 0x09, 0x09), (0xFF, 0xFF, 0xFF), (0x0F, 0xD7, 0xFF), (0x69, 0xA2, 0xFF),
        (0xD4, 0x80, 0xFF), (0xFF, 0x45, 0xF3), (0xFF, 0x61, 0x8B), (0xFF, 0x88, 0x33), (0xFF, 0x9C, 0x12),
        (0xFA, 0xBC, 0x20), (0x9F, 0xE3, 0x0E), (0x2B, 0xF0, 0x35), (0x0C, 0xF0, 0xA4), (0x05, 0xFB, 0xFF),
        (0x5E, 0x5E, 0x5E), (0x0D, 0x0D, 0x0D), (0x0D, 0x0D, 0x0D), (0xFF, 0xFF, 0xFF), (0xA6, 0xFC, 0xFF),
        (0xB3, 0xEC, 0xFF), (0xDA, 0xAB, 0xEB), (0xFF, 0xA8, 0xF9), (0xFF, 0xAB, 0xB3), (0xFF, 0xD2, 0xB0),
        (0xFF, 0xEF, 0xA6), (0xFF, 0xF7, 0x9C), (0xD7, 0xE8, 0x95), (0xA6, 0xED, 0xAF), (0xA2, 0xF2, 0xDA),
        (0x99, 0xFF, 0xFC), (0xDD, 0xDD, 0xDD), (0x11, 0x11, 0x11), (0x11, 0x11, 0x11),
    ];

    private readonly IntPtr renderer;
    private readonly IntPtr texture;
    private Rom rom = Rom.Empty;

    private const int Width = 10*16;
    private const int Height = 10*16;

    private readonly PaletteColor[,] frameData = new PaletteColor[Height, Width];
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
                    0 => SystemPalette[0x02],
                    1 => SystemPalette[0x23],
                    2 => SystemPalette[0x27],
                    3 => SystemPalette[0x30],
                    _ => throw new InvalidOperationException("Tile value should be between 0 and 4")
                };

                frameData[renderY + y, renderX + x] = new PaletteColor(rgb);
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

    private static SDL.SDL_Color PaletteToSDLColor(PaletteColor paletteColor)
    {
        return new SDL.SDL_Color { r = paletteColor.R, g = paletteColor.G, b = paletteColor.B, a = 255 };
    }

    private record struct PaletteColor(byte R, byte G, byte B)
    {
        public PaletteColor((byte R, byte G, byte B) value) : this(value.R, value.G, value.B)
        {
        }
    }
}

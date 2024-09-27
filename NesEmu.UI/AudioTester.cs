using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ImGuiNET;
using NesEmu.UI.ImGuiSDLRendering;
using SDL2;

namespace NesEmu.UI;

public class AudioTester : IElement
{
    const int SampleRate = 44100;
    const int BufferSize = 4096;

    private bool active = false;

    private float frequency = 440f;
    private float volume = 0.8f;
    private IEnumerator<float> toneGenerator;

    private SDL.SDL_AudioSpec spec;

    public AudioTester()
    {
        spec = new SDL.SDL_AudioSpec
        {
            format = SDL.AUDIO_F32,
            channels = 1,
            freq = SampleRate,
            samples = BufferSize,
            callback = AudioCallback
        };
    }

    private void AudioCallback(IntPtr userdata, IntPtr stream, int len)
    {
        var data = new float[BufferSize];
        for (int i = 0; i < BufferSize; i++)
        {
            toneGenerator.MoveNext();
            data[i] = toneGenerator.Current;
        }
        Console.WriteLine(data[0]);
        Marshal.Copy(data, 0, stream, BufferSize);
    }

    public void NewFrame()
    {
        ImGui.Begin("Audio Test");
        
        ImGui.BeginDisabled(active);
        ImGui.SliderFloat("Frequency", ref frequency, 110f, 880f);
        ImGui.SliderFloat("Volume", ref volume, 0f, 1f);

        if (ImGui.Button("Beep"))
        {
            Beep();
        }
        
        ImGui.EndDisabled();

        ImGui.End();
    }

    private async void Beep()
    {
        toneGenerator = Oscillator(SampleRate / frequency, volume);
        SDL.SDL_OpenAudio(ref spec, IntPtr.Zero).ThrowOnError();
        active = true;
        SDL.SDL_PauseAudio(0);

        await Task.Delay(TimeSpan.FromSeconds(0.5));
        
        SDL.SDL_PauseAudio(1);
        SDL.SDL_CloseAudio();
        active = false;
    }

    // ReSharper disable once IteratorNeverReturns
    private static IEnumerator<float> Oscillator(float rate, float volume)
    {
        var current = 0f;
        var step = (2f * MathF.PI) / rate;
        while (true)
        {
            current += step;
            yield return MathF.Sin(current) * volume;
        }
    }
}

using System;
using SDL2;

namespace NesEmu.UI.ImGuiSDLRendering;

public class SDLException : ApplicationException
{
    public SDLException() : base($"An error occured from the SDL2 backend: {SDL.SDL_GetError()}")
    {
    }

    public SDLException(string message) : base($"An error occured from the SDL2 backend: {message}")
    {
    }

    public SDLException(int code) : base($"An error occured from the SDL2 backend: {SDL.SDL_GetError()} ({code})")
    {
    }
}

public static class SDLExceptionExtension
{
    public static void ThrowOnError(this int code)
    {
        if (code >= 0) return;
        throw new SDLException(code);
    }
}

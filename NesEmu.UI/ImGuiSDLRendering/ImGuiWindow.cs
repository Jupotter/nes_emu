using System;
using System.Diagnostics;
using ImGuiNET;
using static SDL2.SDL;


namespace NesEmu.UI.ImGuiSDLRendering;

public class ImGuiWindow : IDisposable
{
    private readonly Application application;
    public static IntPtr window;
    public static IntPtr renderer;

    public ImGuiWindow(Application application)
    {
        this.application = application;
        SDL_Init(SDL_INIT_EVERYTHING).ThrowOnError();

        const SDL_WindowFlags windowFlags = SDL_WindowFlags.SDL_WINDOW_RESIZABLE |
                                            SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI |
                                            SDL_WindowFlags.SDL_WINDOW_MAXIMIZED;

        const SDL_RendererFlags rendererFlags = SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                                                SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC;

        window = SDL_CreateWindow("Untitled", 0, 0, 1920, 1080, windowFlags);
        renderer = SDL_CreateRenderer(window, -1, rendererFlags);
        if (renderer == IntPtr.Zero) throw new SDLException();
    }

    public void Dispose()
    {
        SDL_DestroyWindow(window);
        SDL_DestroyRenderer(renderer);
        SDL_Quit();
    }

    public void Launch()
    {
        var stopwatch = Stopwatch.StartNew();
        ImGui.CreateContext();
        
        MainLoop(stopwatch);
        
        ImGui.DestroyContext();
    }

    private void MainLoop(Stopwatch stopwatch)
    {
        using var device = new ImGuiDevice(window, renderer);

        device.Initialize();
        
        SDL_SetWindowTitle(window, "NesEmu");
        application.Initialize();

        while (true)
        {
            stopwatch.Restart();
            while (SDL_PollEvent(out var e) != 0)
            {
                if (IsQuitEvent(e)) return;
                application.HandleSdlEvent(e);
                device.ProcessEvent(e);
            }

            stopwatch.Stop();
            device.NewFrame(stopwatch.Elapsed);
            ImGui.NewFrame();

            application.NewFrame();

            ImGui.EndFrame();
            ImGui.Render();
            device.Render(ImGui.GetDrawData());
        }
    }

    private bool IsQuitEvent(in SDL_Event sdlEvent) => sdlEvent.type switch
    {
        SDL_EventType.SDL_QUIT => true,
        SDL_EventType.SDL_WINDOWEVENT => sdlEvent.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE &&
                                         sdlEvent.window.windowID == SDL_GetWindowID(window),
        _ => false
    };
}

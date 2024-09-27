using NesEmu.UI;
using NesEmu.UI.ImGuiSDLRendering;

var emulator = NesEmu.Emulator.Initialize();



var application = new Application(emulator);

using var window = new ImGuiWindow(application);

window.Launch();
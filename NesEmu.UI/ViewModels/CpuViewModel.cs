using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using NesEmu;
using ReactiveUI;

namespace NesEmu.UI.ViewModels;

public class CpuViewModel : ViewModelBase
{
    private readonly Cpu cpu;

    public CpuViewModel()
    {
        cpu = new Cpu();
        cpu.Load(StaticPrograms.Snake, 0x0600);

        cpu.Reset();

        StepCommand = ReactiveCommand.Create(Step);
        RunCommand = ReactiveCommand.Create(Run);
    }

    public ushort PC => cpu.PC;

    public byte RegisterA => cpu.RegisterA;
    public byte RegisterX => cpu.RegisterX;
    public byte RegisterY => cpu.RegisterY;
    public byte RegisterS => cpu.RegisterS;
    public CpuFlags Status => cpu.Status;

    public ReactiveCommand<Unit, Unit> StepCommand { get; }
    public ReactiveCommand<Unit, Task> RunCommand { get; }

    private void Step()
    {
        cpu.Step();
        RefreshCpu();
    }
    
    private async Task Run()
    {
        while (!cpu.Step())
        {
            RefreshCpu();
            await Task.Delay(TimeSpan.FromSeconds(0.25));
        }
    }

    private void RefreshCpu()
    {
        this.RaisePropertyChanged(nameof(PC));
        this.RaisePropertyChanged(nameof(RegisterA));
        this.RaisePropertyChanged(nameof(RegisterX));
        this.RaisePropertyChanged(nameof(RegisterY));
        this.RaisePropertyChanged(nameof(RegisterS));
        this.RaisePropertyChanged(nameof(Status));
    }
}

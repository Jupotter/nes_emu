using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using NesEmu;
using ReactiveUI;

namespace NesEmu.UI.ViewModels;

public class CpuViewModel : ViewModelBase
{
    private readonly Cpu cpu;

    public CpuViewModel()
    {
        var snakeRom = File.ReadAllBytes("Roms/snake.nes"); 
        cpu = new Cpu(new NesBus(Rom.Parse(snakeRom)));
        
        cpu.MemWriteByte(0xff, 0x77);
        cpu.Reset();

        StepCommand = ReactiveCommand.CreateFromTask(Step);
        RunCommand = ReactiveCommand.CreateFromTask(Run);
        ResetCommand = ReactiveCommand.Create(() => cpu.Reset());
        frame = frames[0];
    }

    public ushort PC => cpu.PC;

    public byte RegisterA => cpu.RegisterA;
    public byte RegisterX => cpu.RegisterX;
    public byte RegisterY => cpu.RegisterY;
    public byte RegisterS => cpu.RegisterS;
    public CpuFlags Status => cpu.Status;

    public string NextInstruction
    {
        get
        {
            var instruction = cpu.GetInstruction(cpu.PC);
            return $"{instruction.Name} ({instruction.AddressingMode})";
        }
    }

    public ReactiveCommand<Unit, Unit> StepCommand { get; }
    public ReactiveCommand<Unit, Unit> RunCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }

    private bool running;
    
    private async Task Step()
    {
        running = false;
        cpu.Step();
        await RefreshCpu();
    }

    private async Task Run()
    {
        if (running)
            return;
        running = true;
        while (running && !cpu.Step())
        {
            await RefreshCpu();
            await Task.Delay(TimeSpan.FromMicroseconds(0.5587));
        }
    }

    private async Task RefreshCpu()
    {
        await UpdateImage();
        this.RaisePropertyChanged(nameof(PC));
        this.RaisePropertyChanged(nameof(RegisterA));
        this.RaisePropertyChanged(nameof(RegisterX));
        this.RaisePropertyChanged(nameof(RegisterY));
        this.RaisePropertyChanged(nameof(RegisterS));
        this.RaisePropertyChanged(nameof(Status));
        this.RaisePropertyChanged(nameof(NextInstruction));
        cpu.MemWriteByte(0xfe, (byte)Random.Shared.Next());
    }

    private readonly WriteableBitmap[] frames =
    [
        new(new PixelSize(32, 32), new Vector(96, 96), PixelFormats.Rgb24, AlphaFormat.Opaque),
        new(new PixelSize(32, 32), new Vector(96, 96), PixelFormats.Rgb24, AlphaFormat.Opaque)
    ];

    private int currentFrameIdx = 0;
    private Bitmap frame;

    public Bitmap Frame
    {
        get => frame;
        private set
        {
            frame = value;
            this.RaisePropertyChanged();
        }
    }

    public void HandleKey(KeyEventArgs e){
        switch (e.Key)
        {
            case Key.W:
                cpu.MemWriteByte(0xff, (byte)'w');
                break;
            case Key.S:
                cpu.MemWriteByte(0xff, (byte)'s');
                break;
            case Key.A:
                cpu.MemWriteByte(0xff, (byte)'a');
                break;
            case Key.D:
                cpu.MemWriteByte(0xff, (byte)'d');
                break;
            default:
                return;
        }
    }

    private List<byte> prevFrameData = new();
    
    private async Task UpdateImage()
    {
        var frameData = new List<byte>();

        for (ushort i = 0x200; i < 0x600; i++)
        {
            var value = cpu.MemReadByte(i);
            var color = GetColor(value);
            frameData.Add(color.Item1);
            frameData.Add(color.Item2);
            frameData.Add(color.Item3);
        }
        if (prevFrameData.SequenceEqual(frameData))
            return;

        var array = frameData.ToArray();
        using (var frameBuffer = frames[currentFrameIdx].Lock())
        {
            Marshal.Copy(array, 0, frameBuffer.Address, array.Length);
        }

        prevFrameData = frameData;
        Frame = frames[currentFrameIdx];

        currentFrameIdx = (currentFrameIdx + 1) % 2;

        await Task.Delay(TimeSpan.FromMilliseconds(50));
    }

    private (byte, byte, byte) GetColor(byte value)
    {
        return (value % 16) switch
        {
            0 => (0, 0, 0),
            1 => (0xff, 0xff, 0xff),
            2 or 8 => (0x10, 0x10, 0x10),
            3 or 9 => (0xff, 0x00, 0x00),
            4 or 10 => (0x00, 0xff, 0x00),
            5 or 11 => (0x00, 0x00, 0xff),
            6 or 12 => (0xff, 0xff, 0x00),
            7 or 13 => (0xff, 0x00, 0xff),
            _ => (0x00, 0xff, 0xff),
        };
    }
}

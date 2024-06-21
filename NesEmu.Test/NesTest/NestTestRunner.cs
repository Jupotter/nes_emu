namespace NesEmu.Test.NesTest;

public class NestTestRunner
{
    public Rom LoadNestTestRom()
    {
        var bytes = File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "NesTest/nestest.nes"));
        return Rom.Parse(bytes);
    }
    
    public string[] LoadNestTestLog()
    {
        return File.ReadAllLines(Path.Combine(TestContext.CurrentContext.TestDirectory, "NesTest/nestest.log"));
    }

    [Test]
    public void TestTraceFormat()
    {
        var bus = new NesBus(LoadNestTestRom(), new Ppu());
        bus.MemWrite(100, 0xa2);
        bus.MemWrite(101, 0x01);
        bus.MemWrite(102, 0xca);
        bus.MemWrite(103, 0x88);
        bus.MemWrite(104, 0x00);

        var cpu = new Cpu(bus);
        cpu.Reset();
        
        cpu.SetRegisterA(1);
        cpu.SetRegisterX(2);
        cpu.SetRegisterY(3);
        cpu.SetPc(0x64);

        cpu.GetTrace().Should().Be("0064  A2 01     LDX #$01                        A:01 X:02 Y:03 P:24 SP:FD");
        cpu.Step();
        cpu.GetTrace().Should().Be("0066  CA        DEX                             A:01 X:01 Y:03 P:24 SP:FD");
        cpu.Step();
        cpu.GetTrace().Should().Be("0067  88        DEY                             A:01 X:00 Y:03 P:26 SP:FD");
        cpu.Step();
    }
    
    [Test]
    public void TestFormatMemAccess()
    {
        var bus = new NesBus(LoadNestTestRom(), new Ppu());
        // ORA ($33), Y
        bus.MemWrite(100, 0x11);
        bus.MemWrite(101, 0x33);
        
        // Data
        bus.MemWrite(0x33, 00);
        bus.MemWrite(0x34, 04);
        
        // Target
        bus.MemWrite(0x400, 0xAA);

        var cpu = new Cpu(bus);
        cpu.Reset();
        
        cpu.SetRegisterY(0);
        cpu.SetPc(0x64);

        cpu.GetTrace().Should().Be("0064  11 33     ORA ($33),Y = 0400 @ 0400 = AA  A:00 X:00 Y:00 P:24 SP:FD");
    }

    [Test]
    public void RunNesTest()
    {
        var emulator = Emulator.Initialize();
        emulator.LoadRom(LoadNestTestRom());
        emulator.Cpu.SetPc(0xc000);

        var nesTestLog = LoadNestTestLog().Select(l => l[..73] + " PPU  0,  0 " + l[86..]).ToList();

        foreach (var t in nesTestLog.TakeWhile(t => !t.StartsWith("C68B")))
        {
            emulator.GetTrace().Should().Be(t);
            emulator.Step();
        }
    }
}

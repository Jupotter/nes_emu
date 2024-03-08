namespace NesEmu.Test.NesTest;

public class NestTestRunner
{
    public Rom LoadNestTestRom()
    {
        var bytes = File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, "NesTest/nestest.nes"));
        return Rom.Parse(bytes);
    }

    [Test]
    public void TestTraceFormat()
    {
        var bus = new NesBus(LoadNestTestRom());
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

        cpu.ToString().Should().Be("0064  A2 01     LDX #$01                        A:01 X:02 Y:03 P:24 SP:FD");
        cpu.Step();
        cpu.ToString().Should().Be("0066  CA        DEX                             A:01 X:01 Y:03 P:24 SP:FD");
        cpu.Step();
        cpu.ToString().Should().Be("0067  88        DEY                             A:01 X:00 Y:03 P:26 SP:FD");
        cpu.Step();
    }
}

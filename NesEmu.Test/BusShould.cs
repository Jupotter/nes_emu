namespace NesEmu.Test;

public class BusShould
{
    [Test]
    [TestCase((ushort) 0x10)]
    [TestCase((ushort) 0xff)]
    [TestCase((ushort) 0x100)]
    [TestCase((ushort) 0x800)]
    [TestCase((ushort) 0x1000)]
    [TestCase((ushort) 0x1800)]
    public void WriteValueToMemory(ushort address)
    {
        var tested = new NesBus(Rom.Empty, new Ppu(), new Apu());

        tested.MemWrite(address, 0xDE);

        tested.MemRead(address).Should().Be(0xDE);
    }
    
    [Test]
    [TestCase((ushort) 0xAA)]
    public void MirrorMainRam(ushort address)
    {
        var tested = new NesBus(Rom.Empty, new Ppu(), new Apu());

        tested.MemWrite(address, 0xDE);

        tested.MemRead((ushort)(address+0x800)).Should().Be(0xDE);
        tested.MemRead((ushort)(address+0x1000)).Should().Be(0xDE);
        tested.MemRead((ushort)(address+0x1800)).Should().Be(0xDE);
    }
}

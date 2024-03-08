namespace NesEmu.Test;

public class RomShould
{
    [Test]
    public void LoadEmptyRom()
    {
        byte[] testRom = [(byte)'N', (byte)'E', (byte)'S', 0x1A, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        var rom = Rom.Parse(testRom);

        rom.Mirroring.Should().Be(ScreenMirroring.Horizontal);
        rom.Mapper.Should().Be(0);
        rom.ChrRom.Should().BeEmpty();
        rom.PrgRom.Should().BeEmpty();
    }

    [Test]
    public void ThrowWithoutNesTag()
    {
        byte[] testRom = [(byte)'N', (byte)'E', 0, 0x1A, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

        var call = () => Rom.Parse(testRom);
        call.Should().ThrowExactly<InvalidDataException>();
    }

    [Test]
    public void ThrowIfVersion2()
    {
        byte[] testRom = [(byte)'N', (byte)'E', (byte)'S', 0x1A, 0, 0, 0, 0b00001000, 0, 0, 0, 0, 0, 0, 0, 0];

        var call = () => Rom.Parse(testRom);
        call.Should().ThrowExactly<NotSupportedException>();
    }

    [Test]
    public void LoadMapper()
    {
        byte[] testRom =
            [(byte)'N', (byte)'E', (byte)'S', 0x1A, 0, 0, 0b0110_0000, 0b1001_0000, 0, 0, 0, 0, 0, 0, 0, 0];

        var rom = Rom.Parse(testRom);

        rom.Mirroring.Should().Be(ScreenMirroring.Horizontal);
        rom.Mapper.Should().Be(0b1001_0110);
        rom.ChrRom.Should().BeEmpty();
        rom.PrgRom.Should().BeEmpty();
    }

    [Test]
    public void LoadFourScreenMirroring()
    {
        byte[] testRom =
            [(byte)'N', (byte)'E', (byte)'S', 0x1A, 0, 0, 0b0000_1000, 0b0000_0000, 0, 0, 0, 0, 0, 0, 0, 0];

        var rom = Rom.Parse(testRom);

        rom.Mirroring.Should().Be(ScreenMirroring.FourScreen);
        rom.Mapper.Should().Be(0);
        rom.ChrRom.Should().BeEmpty();
        rom.PrgRom.Should().BeEmpty();
    }

    [Test]
    public void LoadVerticalMirroring()
    {
        byte[] testRom =
            [(byte)'N', (byte)'E', (byte)'S', 0x1A, 0, 0, 0b0000_0001, 0b0000_0000, 0, 0, 0, 0, 0, 0, 0, 0];

        var rom = Rom.Parse(testRom);

        rom.Mirroring.Should().Be(ScreenMirroring.Vertical);
        rom.Mapper.Should().Be(0);
        rom.ChrRom.Should().BeEmpty();
        rom.PrgRom.Should().BeEmpty();
    }

    [Test]
    public void LoadRoms()
    {
        var testRom =
            new byte[]
                {
                    (byte)'N', (byte)'E', (byte)'S', 0x1A, 2, 3, 0b0000_0000, 0b0000_0000, 0, 0, 0, 0, 0, 0, 0, 0,
                }.Concat(Enumerable.Repeat((byte)0x90, 0x4000*2))
                .Concat(Enumerable.Repeat((byte)0xDE, 0x2000*3))
                .Concat(Enumerable.Repeat((byte)0x00, 0x1))
                .ToArray();

        var rom = Rom.Parse(testRom);

        rom.PrgRom.Should().BeEquivalentTo(Enumerable.Repeat((byte)0x90, 0x4000*2));
        rom.ChrRom.Should().BeEquivalentTo(Enumerable.Repeat((byte)0xDE, 0x2000*3));
    }
    
    [Test]
    public void SkipTrainer()
    {
        var testRom =
            new byte[]
                {
                    (byte)'N', (byte)'E', (byte)'S', 0x1A, 2, 2, 0b0000_0100, 0b0000_0000, 0, 0, 0, 0, 0, 0, 0, 0,
                }.Concat(Enumerable.Repeat((byte)0xAA, 512))
                .Concat(Enumerable.Repeat((byte)0x90, 0x4000*2))
                .Concat(Enumerable.Repeat((byte)0xDE, 0x4000))
                .Concat(Enumerable.Repeat((byte)0x00, 0x1))
                .ToArray();

        var rom = Rom.Parse(testRom);

        rom.PrgRom.Should().BeEquivalentTo(Enumerable.Repeat((byte)0x90, 0x4000*2));
        rom.ChrRom.Should().BeEquivalentTo(Enumerable.Repeat((byte)0xDE, 0x2000*2));
    }
}

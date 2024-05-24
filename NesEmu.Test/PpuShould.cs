namespace NesEmu.Test;

public class PpuShould
{
    public static readonly ushort[] TestVRamAddresses = [0x2000, 0x2001, 0x20ff, 0x2100, 0x2400, 0x2401, 0x24ff];
    public static readonly ushort[] TestVRamAddressesHorizontal = [0x2000, 0x2001, 0x20ff, 0x2100, 0x2800, 0x2801, 0x28ff];
    public static readonly ushort[] TestChrRomAddresses = [0x000, 0x0001, 0x00ff, 0x0100, 0x0101, 0x1fff];

    [Test]
    public void ReadDataFromVRamAddressVertical([ValueSource(typeof(PpuShould), nameof(TestVRamAddresses))] ushort address)
    {
        var tested = new Ppu();

        tested.VRamWrite(address, 0xDE);

        tested.PpuAddr = (byte)(address >> 8);
        tested.PpuAddr = (byte)address;
        var discard = tested.PpuData;
        var result = tested.PpuData;
        result.Should().Be(0xDE);
        discard.Should().Be(0x00);
    }

    [Test]
    public void ReadDataFromVRamAddressMirroredVertical([ValueSource(typeof(PpuShould), nameof(TestVRamAddresses))] ushort address)
    {
        var tested = new Ppu();

        tested.VRamWrite(address, 0xDE);

        tested.PpuAddr = (byte)((address >> 8) + 0x8);
        tested.PpuAddr = (byte)address;
        var discard = tested.PpuData;
        var result = tested.PpuData;
        result.Should().Be(0xDE);
        discard.Should().Be(0x00);
    }
    
    [Test]
    public void ReadDataFromVRamAddressHorizontal([ValueSource(typeof(PpuShould), nameof(TestVRamAddressesHorizontal))] ushort address)
    {
        var tested = new Ppu();
        tested.Load(new Rom(Array.Empty<byte>(), Array.Empty<byte>(), 0, ScreenMirroring.Horizontal));

        tested.VRamWrite(address, 0xDE);

        tested.PpuAddr = (byte)(address >> 8);
        tested.PpuAddr = (byte)address;
        var discard = tested.PpuData;
        var result = tested.PpuData;
        result.Should().Be(0xDE);
        discard.Should().Be(0x00);
    }

    [Test]
    public void ReadDataFromVRamAddressMirroredHorizontal([ValueSource(typeof(PpuShould), nameof(TestVRamAddressesHorizontal))] ushort address)
    {
        var tested = new Ppu();
        tested.Load(new Rom(Array.Empty<byte>(), Array.Empty<byte>(), 0, ScreenMirroring.Horizontal));

        tested.VRamWrite(address, 0xDE);

        var readAddress = (ushort)(address + 0x400);

        tested.PpuAddr = (byte)(readAddress >> 8);
        tested.PpuAddr = (byte)readAddress;
        var discard = tested.PpuData;
        var result = tested.PpuData;
        result.Should().Be(0xDE);
        discard.Should().Be(0x00);
    }
    
    [Test]
    public void ReadDataFromChrRomAddress([ValueSource(typeof(PpuShould), nameof(TestChrRomAddresses))] ushort address)
    {
        var tested = new Ppu();
        var chrRom = new byte[8192];
        chrRom[address] = 0xDE;
        
        tested.Load(new Rom(Array.Empty<byte>(), chrRom, 0, ScreenMirroring.Vertical));

        tested.PpuAddr = (byte)(address >> 8);
        tested.PpuAddr = (byte)address;
        var discard = tested.PpuData;
        var result = tested.PpuData;
        result.Should().Be(0xDE);
        discard.Should().Be(0x00);
    }
    
    [Test]
    public void ReadDataFromPaletteAddress()
    {
        var tested = new Ppu();
        tested.PaletteWrite(0x16, 0xDE);

        tested.PpuAddr = 0x3f;
        tested.PpuAddr = 0x16;
        var result = tested.PpuData;
        result.Should().Be(0xDE);
    }

    [Test]
    public void IncrementAddressOnReadAcross([ValueSource(typeof(PpuShould), nameof(TestVRamAddresses))] ushort address)
    {
        var tested = new Ppu();

        tested.VRamWrite(address, 0xDE);

        tested.PpuAddr = (byte)((0xff00 & address) >> 8);
        tested.PpuAddr = (byte)address;
        _ = tested.PpuData;
        
        tested.ReadAddress.Should().Be((ushort)(address + 1));
        
        var result = tested.PpuData;
        result.Should().Be(0xDE);
        tested.ReadAddress.Should().Be((ushort)(address + 2));
    }
    
    [Test]
    public void IncrementAddressOnReadDown([ValueSource(typeof(PpuShould), nameof(TestVRamAddresses))] ushort address)
    {
        var tested = new Ppu();

        tested.VRamWrite(address, 0xDE);
        tested.ControlRegister = Ppu.ControlRegisterFlags.VRamAddIncrement;

        tested.PpuAddr = (byte)((0xff00 & address) >> 8);
        tested.PpuAddr = (byte)address;
        _ = tested.PpuData;
        
        tested.ReadAddress.Should().Be((ushort)(address + 32));
        
        var result = tested.PpuData;
        result.Should().Be(0xDE);
        tested.ReadAddress.Should().Be((ushort)(address + 64));
    }
    
    [Test]
    public void WriteDataToVRamAddress([ValueSource(typeof(PpuShould), nameof(TestVRamAddresses))] ushort address)
    {
        var tested = new Ppu();

        tested.PpuAddr = (byte)((0xff00 & address) >> 8);
        tested.PpuAddr = (byte)address;
        tested.PpuData = 0xDE;

        tested.ReadAddress.Should().Be((ushort)(address + 1));
        
        tested.PpuAddr = (byte)((0xff00 & address) >> 8);
        tested.PpuAddr = (byte)address;
        _ = tested.PpuData;
        var result = tested.PpuData;
        result.Should().Be(0xDE);
    }
}

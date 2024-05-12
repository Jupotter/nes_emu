namespace NesEmu.Test;

public class PpuShould
{
    public static readonly ushort[] TestVRamAddresses = [0x2000, 0x2001, 0x20ff, 0x2100];

    [Test]
    public void ReadDataFromAddress([ValueSource(typeof(PpuShould), nameof(TestVRamAddresses))] ushort address)
    {
        var tested = new Ppu();

        tested.VRamWrite(address, 0xDE);

        tested.PpuAddr = (byte)((0xff00 & address) >> 8);
        tested.PpuAddr = (byte)address;
        _ = tested.PpuData;
        var result = tested.PpuData;
        result.Should().Be(0xDE);
    }

    [Test]
    public void ReadDataFromAddressMirrored([ValueSource(typeof(PpuShould), nameof(TestVRamAddresses))] ushort address)
    {
        var tested = new Ppu();

        tested.VRamWrite(address, 0xDE);

        tested.PpuAddr = (byte)((address >> 8) + 0x8);
        tested.PpuAddr = (byte)address;
        _ = tested.PpuData;
        var result = tested.PpuData;
        result.Should().Be(0xDE);
    }
}

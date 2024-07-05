namespace NesEmu.Test;

public class JoypadShould
{
    [Test]
    public void InitializeToEmpty()
    {
        var tested = new Joypad();
        tested.Strobe.Should().BeFalse();
        tested.DownButtons.Should().Be(Joypad.Button.None);
    }
    
    [Test]
    public void WriteStrobeFlag()
    {
        var tested = new Joypad();

        tested.Write(1);

        tested.Strobe.Should().BeTrue();
    }

    [Test]
    public void UpdateButtonStatus()
    {
        var tested = new Joypad();
        
        tested.SetButtonStatus(Joypad.Button.Down, true);

        tested.DownButtons.Should().HaveFlag(Joypad.Button.Down);
        
        tested.SetButtonStatus(Joypad.Button.ButtonA, true);
        
        tested.DownButtons.Should().HaveFlag(Joypad.Button.ButtonA);
        tested.DownButtons.Should().HaveFlag(Joypad.Button.Down);
        
        tested.SetButtonStatus(Joypad.Button.Down, false);
        
        tested.DownButtons.Should().HaveFlag(Joypad.Button.ButtonA);
        tested.DownButtons.Should().NotHaveFlag(Joypad.Button.Down);
    }
    
    [Test]
    public void ReadButtonSequence()
    {
        var sequence = 0b10110010;
        var buttons = (Joypad.Button)sequence;

        var tested = new Joypad();
        tested.SetButtonStatus(buttons, true);
        tested.Write(1);
        tested.Write(0);

        for (var i = 0; i < 8; i++)
        {
            var result = tested.Read();
            (result & 0b11111110).Should().Be(0);

            var expected = (byte)(sequence >> i & 1);
            result.Should().Be(expected);
        }
        tested.Read().Should().Be(1);
        tested.Read().Should().Be(1);
    }
    
    [Test]
    public void NotAdvanceIfStrobing()
    {
        var tested = new Joypad();
        tested.SetButtonStatus(Joypad.Button.ButtonA, true);
        tested.Write(1);

        tested.Read().Should().Be(1);
        tested.Read().Should().Be(1);
    }
}

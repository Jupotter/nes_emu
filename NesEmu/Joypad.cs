namespace NesEmu;

public class Joypad
{
    [Flags]
    public enum Button : byte
    {
        None = 0,
        Right = 0b10000000,
        Left = 0b01000000,
        Down = 0b00100000,
        Up = 0b00010000,
        Start = 0b00001000,
        Select = 0b00000100,
        ButtonB = 0b00000010,
        ButtonA = 0b00000001,
    }

    public enum ControlFlags : byte
    {
        None = 0,
        Strobe = 0b00000001,
        Unused = 0b11111110,
    }

    public bool Strobe { get; private set; }
    private int ButtonIndex { get; set; } = 0;

    public Button DownButtons { get; private set; } = Button.None;

    public void Write(byte flags)
    {
        Strobe = ((ControlFlags)flags).HasFlag(ControlFlags.Strobe);
        if (Strobe)
        {
            ButtonIndex = 0;
        }
    }

    public byte Read()
    {
        if (ButtonIndex > 7)
            return 1;
        
        var result = ((byte)DownButtons & (1 << ButtonIndex)) != 0;
        if (!Strobe)
            ButtonIndex++;

        return result ? (byte)1 : (byte)0;
    }

    public void SetButtonStatus(Button button, bool pressed)
    {
        if (button == Button.None)
            return;

        if (pressed)
            DownButtons |= button;
        else
            DownButtons &= ~button;
    }
}

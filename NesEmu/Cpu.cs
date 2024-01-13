namespace NesEmu;

[Flags]
public enum CpuFlags : byte
{
    None = 0,
    Carry = 1,
    Zero = 2,
    InterruptDisable = 4,
    DecimalMode = 8,
    BreakCommand = 16,
    Overflow = 32,
    Negative = 64,
}

public enum AddressingMode
{
    Immediate,
    ZeroPage,
    ZeroPage_X,
    ZeroPage_Y,
    Absolute,
    Absolute_X,
    Absolute_Y,
    Indirect_X,
    Indirect_Y,
    NoneAddressing,
}

public class Cpu
{
    private byte registerA = 0;
    private byte registerX = 0;
    private byte registerY = 0;
    private CpuFlags status = CpuFlags.None;
    private ushort program_counter = 0;

    private readonly byte[] memory = new byte[0xffff];

    private Span<byte> Rom => memory.AsSpan()[0x8000..];

    public byte RegisterA => registerA;
    public byte RegisterX => registerX;
    public byte RegisterY => registerY;
    public CpuFlags Status => status;
    public ushort PC => program_counter;

    public override string ToString()
    {
        return @$"Register A: {RegisterA}
Register X: {RegisterX}
Status: {Status}
PC: {PC}";
    }

    public void Interpret(byte[] program)
    {
        Load(program);
        Reset();
        Run();
    }

    private void Load(byte[] rom)
    {
        rom.AsSpan().CopyTo(Rom);
        MemWriteShort(0xfffc, 0x8000);
    }

    private void Reset()
    {
        registerA = 0;
        registerX = 0;
        registerY = 0;
        status = CpuFlags.None;

        program_counter = MemReadShort(0xFFFC);
    }

    private void Run()
    {
        while (true)
        {
            var opcode = MemReadByte(program_counter++);

            switch (opcode)
            {
                case 0xA9:
                    LDA(AddressingMode.Immediate);
                    program_counter++;
                    break;
                case 0xA5:
                    LDA(AddressingMode.ZeroPage);
                    program_counter += 1;
                    break;
                case 0xAD:
                    LDA(AddressingMode.Absolute);
                    program_counter += 2;
                    break;
                case 0xB5:
                    LDA(AddressingMode.ZeroPage_X);
                    program_counter += 1;
                    break;
                case 0xBD:
                    LDA(AddressingMode.Absolute_X);
                    program_counter += 2;
                    break;
                case 0xB9:
                    LDA(AddressingMode.Absolute_Y);
                    program_counter += 2;
                    break;
                case 0xA1:
                    LDA(AddressingMode.Indirect_X);
                    program_counter += 1;
                    break;
                case 0xB1:
                    LDA(AddressingMode.Indirect_Y);
                    program_counter += 1;
                    break;
                case 0xAA:
                    TAX();
                    break;
                case 0xA8:
                    TAY();
                    break;
                case 0xE8:
                    INX();
                    break;
                case 0x00:
                    return;
                default:
                    throw new NotImplementedException($"Instruction 0x{opcode:X} not implemented");
            }
        }
    }

    private void LDA(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        registerA = param;
        UpdateZeroAndNegativeFlags(registerA);
    }

    private void TAX()
    {
        registerX = registerA;
        UpdateZeroAndNegativeFlags(registerX);
    }

    private void TAY()
    {
        registerY = registerA;
        UpdateZeroAndNegativeFlags(registerY);
    }

    private void INX()
    {
        registerX++;
        UpdateZeroAndNegativeFlags(registerX);
    }

    private void UpdateZeroAndNegativeFlags(byte value)
    {
        if (value == 0)
        {
            status |= CpuFlags.Zero;
        }
        else
        {
            status &= ~CpuFlags.Zero;
        }
        if ((sbyte)value < 0)
        {
            status |= CpuFlags.Negative;
        }
        else
        {
            status &= ~CpuFlags.Negative;
        }
    }

    public byte MemReadByte(ushort address)
    {
        return memory[address];
    }

    public void MemWriteByte(ushort address, byte value)
    {
        memory[address] = value;
    }

    public ushort MemReadShort(ushort address)
    {
        ushort low = memory[address];
        ushort high = memory[address + 1];

        return (ushort)(high << 8 | low);
    }

    public void MemWriteShort(ushort address, ushort value)
    {
        var low = (byte)value;
        var high = (byte)(value >> 8);

        memory[address] = low;
        memory[address + 1] = high;
    }

    private ushort GetOperandAddress(AddressingMode mode)
    {
        return mode switch
        {
            AddressingMode.Immediate => program_counter,
            AddressingMode.ZeroPage => MemReadByte(program_counter),
            AddressingMode.Absolute => MemReadShort(program_counter),
            AddressingMode.ZeroPage_X => (byte)(MemReadByte(program_counter) + registerX),
            AddressingMode.ZeroPage_Y => (byte)(MemReadByte(program_counter) + registerY),
            AddressingMode.Absolute_X => (ushort)(MemReadShort(program_counter) + registerX),
            AddressingMode.Absolute_Y => (ushort)(MemReadShort(program_counter) + registerY),
            AddressingMode.Indirect_X
                => MemReadShort((byte)(MemReadByte(program_counter) + registerX)),
            AddressingMode.Indirect_Y
                => (ushort)(MemReadShort(MemReadByte(program_counter)) + registerY),
            AddressingMode.NoneAddressing
                => throw new InvalidOperationException($"Mode {mode} is not supported"),
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };
    }

    public void SetRegisterX(byte value)
    {
        registerX = value;
    }

    public void SetRegisterY(byte value)
    {
        registerY = value;
    }
}

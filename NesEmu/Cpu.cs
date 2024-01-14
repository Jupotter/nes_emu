using System.Diagnostics.CodeAnalysis;

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

[SuppressMessage("ReSharper", "InconsistentNaming")]
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
    NoAddressing,
}

public class Cpu
{
    public record struct Instruction(byte Opcode, string Name, int Bytes, int Cycles, AddressingMode AddressingMode);

    public static readonly IReadOnlyDictionary<byte, Instruction> Instructions = new Instruction[]
    {
        new(0x00, "BRK", 1, 7, AddressingMode.NoAddressing),
        new(0xE8, "INX", 1, 2, AddressingMode.NoAddressing),
        new(0xAA, "TAX", 1, 2, AddressingMode.NoAddressing),
        new(0xA8, "TAY", 1, 2, AddressingMode.NoAddressing),
        // LDA
        new(0xA9, "LDA", 2, 2, AddressingMode.Immediate),
        new(0xA5, "LDA", 2, 3, AddressingMode.ZeroPage),
        new(0xB5, "LDA", 2, 4, AddressingMode.ZeroPage_X),
        new(0xAD, "LDA", 3, 4, AddressingMode.Absolute),
        new(0xBD, "LDA", 3, 4, AddressingMode.Absolute_X),
        new(0xB9, "LDA", 3, 4, AddressingMode.Absolute_Y),
        new(0xA1, "LDA", 2, 6, AddressingMode.Indirect_X),
        new(0xB1, "LDA", 2, 5, AddressingMode.Indirect_Y),
        //LDX
        new(0xA2, "LDX", 2, 2, AddressingMode.Immediate),
        new(0xA6, "LDX", 2, 3, AddressingMode.ZeroPage),
        new(0xB6, "LDX", 2, 4, AddressingMode.ZeroPage_Y),
        new(0xAE, "LDX", 3, 4, AddressingMode.Absolute),
        new(0xBE, "LDX", 3, 4, AddressingMode.Absolute_Y),
        //LDY
        new(0xA0, "LDY", 2, 2, AddressingMode.Immediate),
        new(0xA4, "LDY", 2, 3, AddressingMode.ZeroPage),
        new(0xB4, "LDY", 2, 4, AddressingMode.ZeroPage_X),
        new(0xAC, "LDY", 3, 4, AddressingMode.Absolute),
        new(0xBC, "LDY", 3, 4, AddressingMode.Absolute_X),
        // STA
        new(0x85, "STA", 2, 3, AddressingMode.ZeroPage),
        new(0x95, "STA", 2, 4, AddressingMode.ZeroPage_X),
        new(0x8D, "STA", 3, 4, AddressingMode.Absolute),
        new(0x9D, "STA", 3, 5, AddressingMode.Absolute_X),
        new(0x99, "STA", 3, 5, AddressingMode.Absolute_Y),
        new(0x81, "STA", 2, 6, AddressingMode.Indirect_X),
        new(0x91, "STA", 2, 6, AddressingMode.Indirect_Y),
    }.ToDictionary(x => x.Opcode);

    private byte registerA = 0;
    private byte registerX = 0;
    private byte registerY = 0;
    private CpuFlags status = CpuFlags.None;
    private ushort programCounter = 0;

    private readonly byte[] memory = new byte[0x10000];

    private Span<byte> Rom => memory.AsSpan()[0x8000..];

    public byte RegisterA => registerA;
    public byte RegisterX => registerX;
    public byte RegisterY => registerY;
    public CpuFlags Status => status;
    public ushort PC => programCounter;

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

        programCounter = MemReadShort(0xFFFC);
    }

    private void Run()
    {
        while (true)
        {
            var code = MemReadByte(programCounter++);
            if (!Instructions.TryGetValue(code, out var opcode))
                throw new NotImplementedException($"Instruction 0x{code:X} not implemented");

            switch (opcode.Name)
            {
                case "LDA":
                    LDA(opcode.AddressingMode);
                    break;                
                case "LDX":
                    LDX(opcode.AddressingMode);
                    break;                
                case "LDY":
                    LDY(opcode.AddressingMode);
                    break;
                case "STA":
                    STA(opcode.AddressingMode);
                    break;
                case "TAX":
                    TAX();
                    break;
                case "TAY":
                    TAY();
                    break;
                case "INX":
                    INX();
                    break;
                case "BRK":
                    return;
                default:
                    throw new NotImplementedException($"Instruction 0x{code:X} not implemented");
            }

            programCounter += (ushort)(opcode.Bytes - 1);
        }
    }

    private void LDA(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        registerA = param;
        UpdateZeroAndNegativeFlags(registerA);
    }
    
    private void LDX(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        registerX = param;
        UpdateZeroAndNegativeFlags(registerX);
    }
    private void LDY(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        registerY = param;
        UpdateZeroAndNegativeFlags(registerY);
    }
    
    private void STA(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        MemWriteByte(address, RegisterA);
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
            AddressingMode.Immediate => programCounter,
            AddressingMode.ZeroPage => MemReadByte(programCounter),
            AddressingMode.Absolute => MemReadShort(programCounter),
            AddressingMode.ZeroPage_X => (byte)(MemReadByte(programCounter) + registerX),
            AddressingMode.ZeroPage_Y => (byte)(MemReadByte(programCounter) + registerY),
            AddressingMode.Absolute_X => (ushort)(MemReadShort(programCounter) + registerX),
            AddressingMode.Absolute_Y => (ushort)(MemReadShort(programCounter) + registerY),
            AddressingMode.Indirect_X
                => MemReadShort((byte)(MemReadByte(programCounter) + registerX)),
            AddressingMode.Indirect_Y
                => (ushort)(MemReadShort(MemReadByte(programCounter)) + registerY),
            AddressingMode.NoAddressing
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

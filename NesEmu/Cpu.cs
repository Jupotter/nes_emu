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
    public static readonly IReadOnlyDictionary<byte, Instruction> Instructions = new Instruction[]
    {
        new(0x00, "BRK", 1, 7, AddressingMode.NoAddressing),
        new(0xEA, "NOP", 1, 2, AddressingMode.NoAddressing),
        // ADC
        new(0x69, "ADC", 2, 2, AddressingMode.Immediate),
        new(0x65, "ADC", 2, 3, AddressingMode.ZeroPage),
        new(0x75, "ADC", 2, 4, AddressingMode.ZeroPage_X),
        new(0x6D, "ADC", 3, 4, AddressingMode.Absolute),
        new(0x7D, "ADC", 3, 4, AddressingMode.Absolute_X),
        new(0x79, "ADC", 3, 4, AddressingMode.Absolute_Y),
        new(0x61, "ADC", 2, 6, AddressingMode.Indirect_X),
        new(0x71, "ADC", 2, 5, AddressingMode.Indirect_Y),
        // SBC
        new(0xE9, "SBC", 2, 2, AddressingMode.Immediate),
        new(0xE5, "SBC", 2, 3, AddressingMode.ZeroPage),
        new(0xF5, "SBC", 2, 4, AddressingMode.ZeroPage_X),
        new(0xED, "SBC", 3, 4, AddressingMode.Absolute),
        new(0xFD, "SBC", 3, 4, AddressingMode.Absolute_X),
        new(0xF9, "SBC", 3, 4, AddressingMode.Absolute_Y),
        new(0xE1, "SBC", 2, 6, AddressingMode.Indirect_X),
        new(0xF1, "SBC", 2, 5, AddressingMode.Indirect_Y),
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
        // Register Transfers
        new(0xAA, "TAX", 1, 2, AddressingMode.NoAddressing),
        new(0xA8, "TAY", 1, 2, AddressingMode.NoAddressing),
        new(0x8A, "TXA", 1, 2, AddressingMode.NoAddressing),
        new(0x98, "TYA", 1, 2, AddressingMode.NoAddressing),
        // Increment/Decrement
        new(0xE8, "INX", 1, 2, AddressingMode.NoAddressing),
        new(0xC8, "INY", 1, 2, AddressingMode.NoAddressing),
        new(0xCA, "DEX", 1, 2, AddressingMode.NoAddressing),
        new(0x88, "DEY", 1, 2, AddressingMode.NoAddressing),
        // Status Flag Changes
        new(0x18, "CLC", 1, 2, AddressingMode.NoAddressing),
        new(0xD8, "CLD", 1, 2, AddressingMode.NoAddressing),
        new(0x58, "CLI", 1, 2, AddressingMode.NoAddressing),
        new(0xB8, "CLV", 1, 2, AddressingMode.NoAddressing),
        new(0x38, "SEC", 1, 2, AddressingMode.NoAddressing),
        new(0xF8, "SED", 1, 2, AddressingMode.NoAddressing),
        new(0x78, "SEI", 1, 2, AddressingMode.NoAddressing),
    }.ToDictionary(x => x.Opcode);

    private readonly byte[] memory = new byte[0x10000];
    private ushort programCounter;

    private byte registerA;
    private byte registerX;
    private byte registerY;
    private CpuFlags status = CpuFlags.None;

    private Span<byte> Rom => memory.AsSpan()[0x8000..];

    public byte RegisterA => registerA;
    public byte RegisterX => registerX;
    public byte RegisterY => registerY;
    public CpuFlags Status => status;
    public ushort PC => programCounter;

    public override string ToString()
    {
        return $"""
                Register A: {RegisterA}
                Register X: {RegisterX}
                Status: {Status}
                PC: {PC}
                """;
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
            {
                throw new NotImplementedException($"Instruction 0x{code:X} not implemented");
            }

            switch (opcode.Name)
            {
                case "ADC":
                    ADC(opcode.AddressingMode);
                    break;
                case "SBC":
                    SBC(opcode.AddressingMode);
                    break;
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
                case "TXA":
                    TXA();
                    break;
                case "TYA":
                    TYA();
                    break;
                case "INX":
                    INX();
                    break;
                case "INY":
                    INY();
                    break;
                case "DEX":
                    DEX();
                    break;
                case "DEY":
                    DEY();
                    break;
                case "CLC":
                    ResetFlag(CpuFlags.Carry);
                    break;
                case "CLD":
                    ResetFlag(CpuFlags.DecimalMode);
                    break;
                case "CLI":
                    ResetFlag(CpuFlags.InterruptDisable);
                    break;
                case "CLV":
                    ResetFlag(CpuFlags.Overflow);
                    break;
                case "SEC":
                    SetFlag(CpuFlags.Carry);
                    break;
                case "SED":
                    SetFlag(CpuFlags.DecimalMode);
                    break;
                case "SEI":
                    SetFlag(CpuFlags.InterruptDisable);
                    break;
                case "NOP":
                    break;
                case "BRK":
                    return;
                default:
                    throw new NotImplementedException($"Instruction 0x{code:X} not implemented");
            }

            programCounter += (ushort)(opcode.Bytes - 1);
        }
    }

    private void ADC(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        ADCImpl(param);
    }

    private void ADCImpl(byte param)
    {
        var result = RegisterA + param + (TestFlag(CpuFlags.Carry) ? 1 : 0);

        UpdateFlags(CpuFlags.Carry, result > byte.MaxValue);

        // Check if the sign of A and param are equal to the sign of the result
        // https://stackoverflow.com/questions/29193303/6502-emulation-proper-way-to-implement-adc-and-sbc
        var hasOverflow = ~(RegisterA ^ param) & (RegisterA ^ result) & 0x80;
        UpdateFlags(CpuFlags.Overflow, hasOverflow != 0);

        registerA = (byte)result;
        UpdateZeroAndNegativeFlags(registerA);
    }

    private void SBC(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        ADCImpl((byte)~param);
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

    private void TXA()
    {
        registerA = registerX;
        UpdateZeroAndNegativeFlags(registerA);
    }
    
    private void TYA()
    {
        registerA = registerY;
        UpdateZeroAndNegativeFlags(registerA);
    }
    
    private void INX()
    {
        registerX++;
        UpdateZeroAndNegativeFlags(registerX);
    }
    
    private void INY()
    {
        registerY++;
        UpdateZeroAndNegativeFlags(registerY);
    }
    private void DEX()
    {
        registerX--;
        UpdateZeroAndNegativeFlags(registerX);
    }
    
    private void DEY()
    {
        registerY--;
        UpdateZeroAndNegativeFlags(registerY);
    }

    private void UpdateZeroAndNegativeFlags(byte value)
    {
        UpdateFlags(CpuFlags.Zero, value == 0);
        UpdateFlags(CpuFlags.Negative, (sbyte)value < 0);
    }

    public CpuFlags UpdateFlags(CpuFlags flag, bool value)
    {
        return value ? SetFlag(flag) : ResetFlag(flag);
    }
    
    public CpuFlags SetFlag(CpuFlags flag)
    {
        status |= flag;
        return status;
    }

    public CpuFlags ResetFlag(CpuFlags flag)
    {
        status &= ~flag;
        return status;
    }

    public bool TestFlag(CpuFlags flag)
    {
        return (Status & flag) != 0;
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

    public record struct Instruction(byte Opcode, string Name, int Bytes, int Cycles, AddressingMode AddressingMode);
}

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
    Overflow = 64,
    Negative = 128,
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum AddressingMode
{
    Immediate,
    Relative = Immediate,
    ZeroPage,
    ZeroPage_X,
    ZeroPage_Y,
    Absolute,
    Absolute_X,
    Absolute_Y,
    Indirect_X,
    Indirect_Y,
    NoAddressing,
    Accumulator = NoAddressing,
}

public class Cpu
{
    private static readonly IReadOnlyDictionary<byte, Instruction> Instructions = new Instruction[]
    {
        new(0x00, "BRK", 1, 7, AddressingMode.NoAddressing, (_, _) => { }),
        new(0xEA, "NOP", 1, 2, AddressingMode.NoAddressing, (_, _) => { }),
        // ADC
        new(0x69, "ADC", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.ADC(mode)),
        new(0x65, "ADC", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.ADC(mode)),
        new(0x75, "ADC", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.ADC(mode)),
        new(0x6D, "ADC", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.ADC(mode)),
        new(0x7D, "ADC", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.ADC(mode)),
        new(0x79, "ADC", 3, 4, AddressingMode.Absolute_Y, (cpu, mode) => cpu.ADC(mode)),
        new(0x61, "ADC", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.ADC(mode)),
        new(0x71, "ADC", 2, 5, AddressingMode.Indirect_Y, (cpu, mode) => cpu.ADC(mode)),
        // AND
        new(0x29, "AND", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.AND(mode)),
        new(0x25, "AND", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.AND(mode)),
        new(0x35, "AND", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.AND(mode)),
        new(0x2D, "AND", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.AND(mode)),
        new(0x3D, "AND", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.AND(mode)),
        new(0x39, "AND", 3, 4, AddressingMode.Absolute_Y, (cpu, mode) => cpu.AND(mode)),
        new(0x21, "AND", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.AND(mode)),
        new(0x31, "AND", 2, 5, AddressingMode.Indirect_Y, (cpu, mode) => cpu.AND(mode)),
        // ASL
        new(0x0A, "ASL", 1, 2, AddressingMode.Accumulator, (cpu, mode) => cpu.ASL(mode)),
        new(0x06, "ASL", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.ASL(mode)),
        new(0x16, "ASL", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.ASL(mode)),
        new(0x0E, "ASL", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.ASL(mode)),
        new(0x1E, "ASL", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.ASL(mode)),
        // LSR
        new(0x4A, "LSR", 1, 2, AddressingMode.Accumulator, (cpu, mode) => cpu.LSR(mode)),
        new(0x46, "LSR", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.LSR(mode)),
        new(0x56, "LSR", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.LSR(mode)),
        new(0x4E, "LSR", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.LSR(mode)),
        new(0x5E, "LSR", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.LSR(mode)),
        // ROL
        new(0x2A, "ROL", 1, 2, AddressingMode.Accumulator, (cpu, mode) => cpu.ROL(mode)),
        new(0x26, "ROL", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.ROL(mode)),
        new(0x36, "ROL", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.ROL(mode)),
        new(0x2E, "ROL", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.ROL(mode)),
        new(0x3E, "ROL", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.ROL(mode)),
        // ROR
        new(0x6A, "ROR", 1, 2, AddressingMode.Accumulator, (cpu, mode) => cpu.ROR(mode)),
        new(0x66, "ROR", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.ROR(mode)),
        new(0x76, "ROR", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.ROR(mode)),
        new(0x6E, "ROR", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.ROR(mode)),
        new(0x7E, "ROR", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.ROR(mode)),
        // SBC
        new(0xE9, "SBC", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.SBC(mode)),
        new(0xE5, "SBC", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.SBC(mode)),
        new(0xF5, "SBC", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.SBC(mode)),
        new(0xED, "SBC", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.SBC(mode)),
        new(0xFD, "SBC", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.SBC(mode)),
        new(0xF9, "SBC", 3, 4, AddressingMode.Absolute_Y, (cpu, mode) => cpu.SBC(mode)),
        new(0xE1, "SBC", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.SBC(mode)),
        new(0xF1, "SBC", 2, 5, AddressingMode.Indirect_Y, (cpu, mode) => cpu.SBC(mode)),
        // LDA
        new(0xA9, "LDA", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.LDA(mode)),
        new(0xA5, "LDA", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.LDA(mode)),
        new(0xB5, "LDA", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.LDA(mode)),
        new(0xAD, "LDA", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.LDA(mode)),
        new(0xBD, "LDA", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.LDA(mode)),
        new(0xB9, "LDA", 3, 4, AddressingMode.Absolute_Y, (cpu, mode) => cpu.LDA(mode)),
        new(0xA1, "LDA", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.LDA(mode)),
        new(0xB1, "LDA", 2, 5, AddressingMode.Indirect_Y, (cpu, mode) => cpu.LDA(mode)),
        //LDX
        new(0xA2, "LDX", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.LDX(mode)),
        new(0xA6, "LDX", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.LDX(mode)),
        new(0xB6, "LDX", 2, 4, AddressingMode.ZeroPage_Y, (cpu, mode) => cpu.LDX(mode)),
        new(0xAE, "LDX", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.LDX(mode)),
        new(0xBE, "LDX", 3, 4, AddressingMode.Absolute_Y, (cpu, mode) => cpu.LDX(mode)),
        //LDY
        new(0xA0, "LDY", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.LDY(mode)),
        new(0xA4, "LDY", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.LDY(mode)),
        new(0xB4, "LDY", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.LDY(mode)),
        new(0xAC, "LDY", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.LDY(mode)),
        new(0xBC, "LDY", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.LDY(mode)),
        // STA
        new(0x85, "STA", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.STA(mode)),
        new(0x95, "STA", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.STA(mode)),
        new(0x8D, "STA", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.STA(mode)),
        new(0x9D, "STA", 3, 5, AddressingMode.Absolute_X, (cpu, mode) => cpu.STA(mode)),
        new(0x99, "STA", 3, 5, AddressingMode.Absolute_Y, (cpu, mode) => cpu.STA(mode)),
        new(0x81, "STA", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.STA(mode)),
        new(0x91, "STA", 2, 6, AddressingMode.Indirect_Y, (cpu, mode) => cpu.STA(mode)),
        // Branch
        new(0x90, "BCC", 2, 2, AddressingMode.Relative, (cpu, _) => cpu.BCC()),
        new(0xB0, "BCS", 2, 2, AddressingMode.Relative, (cpu, _) => cpu.BCS()),
        new(0xF0, "BEQ", 2, 2, AddressingMode.Relative, (cpu, _) => cpu.BEQ()),
        new(0xD0, "BNE", 2, 2, AddressingMode.Relative, (cpu, _) => cpu.BNE()),
        new(0x30, "BMI", 2, 2, AddressingMode.Relative, (cpu, _) => cpu.BMI()),
        new(0x10, "BPL", 2, 2, AddressingMode.Relative, (cpu, _) => cpu.BPL()),
        new(0x50, "BVC", 2, 2, AddressingMode.Relative, (cpu, _) => cpu.BVC()),
        new(0x70, "BVS", 2, 2, AddressingMode.Relative, (cpu, _) => cpu.BVS()),
        // Register Transfers
        new(0xAA, "TAX", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.TAX()),
        new(0xA8, "TAY", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.TAY()),
        new(0x8A, "TXA", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.TXA()),
        new(0x98, "TYA", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.TYA()),
        // Stack manipulation
        new(0x9A, "TXS", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.TXS()),
        new(0xBA, "TSX", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.TSX()),
        new(0x48, "PHA", 1, 3, AddressingMode.NoAddressing, (cpu, _) => cpu.PHA()),
        new(0x08, "PHP", 1, 3, AddressingMode.NoAddressing, (cpu, _) => cpu.PHP()),
        new(0x68, "PLA", 1, 4, AddressingMode.NoAddressing, (cpu, _) => cpu.PLA()),
        new(0x28, "PLP", 1, 4, AddressingMode.NoAddressing, (cpu, _) => cpu.PLP()),
        // Increment/Decrement
        new(0xE8, "INX", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.INX()),
        new(0xC8, "INY", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.INY()),
        new(0xCA, "DEX", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.DEX()),
        new(0x88, "DEY", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.DEY()),
        // Status Flag Changes
        new(0x18, "CLC", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.ResetFlag(CpuFlags.Carry)),
        new(0xD8, "CLD", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.ResetFlag(CpuFlags.DecimalMode)),
        new(0x58, "CLI", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.ResetFlag(CpuFlags.InterruptDisable)),
        new(0xB8, "CLV", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.ResetFlag(CpuFlags.Overflow)),
        new(0x38, "SEC", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.SetFlag(CpuFlags.Carry)),
        new(0xF8, "SED", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.SetFlag(CpuFlags.DecimalMode)),
        new(0x78, "SEI", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.SetFlag(CpuFlags.InterruptDisable)),
    }.ToDictionary(x => x.Opcode);

    private readonly byte[] memory = new byte[0x10000];
    private ushort programCounter;

    private byte registerA;
    private byte registerX;
    private byte registerY;
    private byte registerS;
    private CpuFlags status = CpuFlags.None;

    private Span<byte> Rom => memory.AsSpan()[0x8000..];

    public byte RegisterA => registerA;
    public byte RegisterX => registerX;
    public byte RegisterY => registerY;
    public byte RegisterS => registerS;
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

    public void Load(byte[] rom)
    {
        rom.AsSpan().CopyTo(Rom);
        MemWriteShort(0xfffc, 0x8000);
    }

    public void Reset()
    {
        registerA = 0;
        registerX = 0;
        registerY = 0;
        registerS = 0;
        status = CpuFlags.None;

        programCounter = MemReadShort(0xFFFC);
    }

    public bool Step()
    {
        var code = MemReadByte(programCounter++);
        var pcBefore = programCounter;
        if (!Instructions.TryGetValue(code, out var opcode))
        {
            throw new NotImplementedException($"Instruction 0x{code:X} not implemented");
        }

        if (opcode.Opcode == 0x00) // BRK
            return true;

        opcode.Action(this, opcode.AddressingMode);

        // check if we jumped during the action
        if (programCounter == pcBefore)
            programCounter += (ushort)(opcode.Bytes - 1);
        return false;
    }
    
    public void Run()
    {
        var brk = false;
        while (!brk)
        {
            brk = Step();
        }
    }

    private void ADC(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        ADCImpl(param);
    }

    private void SBC(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        ADCImpl((byte)~param);
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

    private void AND(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        registerA &= param;
        UpdateZeroAndNegativeFlags(registerA);
    }

    private void ASL(AddressingMode mode)
    {
        var param = RegisterA;
        ushort address = 0;
        if (mode != AddressingMode.NoAddressing)
        {
            address = GetOperandAddress(mode);
            param = MemReadByte(address);
        }

        UpdateFlags(CpuFlags.Carry, (param & 0x80) != 0);
        param <<= 1;

        UpdateZeroAndNegativeFlags(param);

        if (mode != AddressingMode.NoAddressing)
        {
            MemWriteByte(address, param);
        }
        else
        {
            registerA = param;
        }
    }

    private void LSR(AddressingMode mode)
    {
        var param = RegisterA;
        ushort address = 0;
        if (mode != AddressingMode.NoAddressing)
        {
            address = GetOperandAddress(mode);
            param = MemReadByte(address);
        }

        UpdateFlags(CpuFlags.Carry, (param & 0x1) != 0);
        param >>= 1;

        UpdateZeroAndNegativeFlags(param);

        if (mode != AddressingMode.NoAddressing)
        {
            MemWriteByte(address, param);
        }
        else
        {
            registerA = param;
        }
    }

    private void ROR(AddressingMode mode)
    {
        var param = RegisterA;
        ushort address = 0;
        if (mode != AddressingMode.NoAddressing)
        {
            address = GetOperandAddress(mode);
            param = MemReadByte(address);
        }

        var carry = TestFlag(CpuFlags.Carry);
        UpdateFlags(CpuFlags.Carry, (param & 0x1) != 0);
        param >>= 1;
        if (carry)
            param |= 0x80;

        UpdateZeroAndNegativeFlags(param);

        if (mode != AddressingMode.NoAddressing)
        {
            MemWriteByte(address, param);
        }
        else
        {
            registerA = param;
        }
    }

    private void ROL(AddressingMode mode)
    {
        var param = RegisterA;
        ushort address = 0;
        if (mode != AddressingMode.NoAddressing)
        {
            address = GetOperandAddress(mode);
            param = MemReadByte(address);
        }

        var carry = TestFlag(CpuFlags.Carry);
        UpdateFlags(CpuFlags.Carry, (param & 0x80) != 0);
        param <<= 1;
        if (carry)
            param |= 0x1;

        UpdateZeroAndNegativeFlags(param);

        if (mode != AddressingMode.NoAddressing)
        {
            MemWriteByte(address, param);
        }
        else
        {
            registerA = param;
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

    private void TSX()
    {
        registerX = registerS;
        UpdateZeroAndNegativeFlags(registerX);
    }
    
    private void TAY()
    {
        registerY = registerA;
        UpdateZeroAndNegativeFlags(registerY);
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

    private void BCC()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (!TestFlag(CpuFlags.Carry))
            programCounter = (ushort)(programCounter + value + 1);
    }
    
    private void BCS()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (TestFlag(CpuFlags.Carry))
            programCounter = (ushort)(programCounter + value + 1);
    }
    
    private void BEQ()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (TestFlag(CpuFlags.Zero))
            programCounter = (ushort)(programCounter + value + 1);
    }    
    
    private void BNE()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (!TestFlag(CpuFlags.Zero))
            programCounter = (ushort)(programCounter + value + 1);
    }
    
    private void BMI()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (TestFlag(CpuFlags.Negative))
            programCounter = (ushort)(programCounter + value + 1);
    }
    
    private void BPL()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (!TestFlag(CpuFlags.Negative))
            programCounter = (ushort)(programCounter + value + 1);
    }
    
    private void BVC()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (!TestFlag(CpuFlags.Overflow))
            programCounter = (ushort)(programCounter + value + 1);
    }
    
    private void BVS()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (TestFlag(CpuFlags.Overflow))
            programCounter = (ushort)(programCounter + value + 1);
    }
    
    private void TXA()
    {
        registerA = registerX;
        UpdateZeroAndNegativeFlags(registerA);
    }
    
    private void TXS()
    {
        registerS = registerX;
    }

    private void PHA()
    {
        MemWriteByte((ushort)(0x100+registerS++),  RegisterA);
    }
    
    private void PHP()
    {
        MemWriteByte((ushort)(0x100+registerS++), (byte)status);
    }
    
    private void PLA()
    {
        registerA = MemReadByte((ushort)(0x100 + (--registerS)));
        UpdateZeroAndNegativeFlags(RegisterA);
    }
    
    private void PLP()
    {
        status = (CpuFlags)MemReadByte((ushort)(0x100 + (--registerS)));
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

    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Local")]
    private record struct Instruction(
        byte Opcode,
        string Name,
        int Bytes,
        int Cycles,
        AddressingMode AddressingMode,
        Action<Cpu, AddressingMode> Action);
}

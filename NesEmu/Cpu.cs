// ReSharper disable InconsistentNaming Using instruction names as method names

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
    Unused = 32,
    Overflow = 64,
    Negative = 128,
}

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum AddressingMode
{
    Immediate,
    Relative,
    ZeroPage,
    ZeroPage_X,
    ZeroPage_Y,
    Absolute,
    Absolute_X,
    Absolute_Y,
    Indirect,
    Indirect_X,
    Indirect_Y,
    NoAddressing,
    Accumulator,
}

public class Cpu(IBus bus)
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
        // EOR
        new(0x49, "EOR", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.EOR(mode)),
        new(0x45, "EOR", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.EOR(mode)),
        new(0x55, "EOR", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.EOR(mode)),
        new(0x4D, "EOR", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.EOR(mode)),
        new(0x5D, "EOR", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.EOR(mode)),
        new(0x59, "EOR", 3, 4, AddressingMode.Absolute_Y, (cpu, mode) => cpu.EOR(mode)),
        new(0x41, "EOR", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.EOR(mode)),
        new(0x51, "EOR", 2, 5, AddressingMode.Indirect_Y, (cpu, mode) => cpu.EOR(mode)),
        // ORA
        new(0x09, "ORA", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.ORA(mode)),
        new(0x05, "ORA", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.ORA(mode)),
        new(0x15, "ORA", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.ORA(mode)),
        new(0x0D, "ORA", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.ORA(mode)),
        new(0x1D, "ORA", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.ORA(mode)),
        new(0x19, "ORA", 3, 4, AddressingMode.Absolute_Y, (cpu, mode) => cpu.ORA(mode)),
        new(0x01, "ORA", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.ORA(mode)),
        new(0x11, "ORA", 2, 5, AddressingMode.Indirect_Y, (cpu, mode) => cpu.ORA(mode)),
        // CMP
        new(0xC9, "CMP", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.CMP(mode, cpu.RegisterA)),
        new(0xC5, "CMP", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.CMP(mode, cpu.RegisterA)),
        new(0xD5, "CMP", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.CMP(mode, cpu.RegisterA)),
        new(0xCD, "CMP", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.CMP(mode, cpu.RegisterA)),
        new(0xDD, "CMP", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.CMP(mode, cpu.RegisterA)),
        new(0xD9, "CMP", 3, 4, AddressingMode.Absolute_Y, (cpu, mode) => cpu.CMP(mode, cpu.RegisterA)),
        new(0xC1, "CMP", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.CMP(mode, cpu.RegisterA)),
        new(0xD1, "CMP", 2, 5, AddressingMode.Indirect_Y, (cpu, mode) => cpu.CMP(mode, cpu.RegisterA)),
        // CPX
        new(0xE0, "CPX", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.CMP(mode, cpu.RegisterX)),
        new(0xE4, "CPX", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.CMP(mode, cpu.RegisterX)),
        new(0xEC, "CPX", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.CMP(mode, cpu.RegisterX)),
        // CPY
        new(0xC0, "CPY", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.CMP(mode, cpu.RegisterY)),
        new(0xC4, "CPY", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.CMP(mode, cpu.RegisterY)),
        new(0xCC, "CPY", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.CMP(mode, cpu.RegisterY)),
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
        // STX
        new(0x86, "STX", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.STX(mode)),
        new(0x96, "STX", 2, 4, AddressingMode.ZeroPage_Y, (cpu, mode) => cpu.STX(mode)),
        new(0x8E, "STX", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.STX(mode)),
        // STY
        new(0x84, "STY", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.STY(mode)),
        new(0x94, "STY", 2, 4, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.STY(mode)),
        new(0x8C, "STY", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.STY(mode)),
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
        new(0xC6, "DEC", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.DEC(mode)),
        new(0xD6, "DEC", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.DEC(mode)),
        new(0xCE, "DEC", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.DEC(mode)),
        new(0xDE, "DEC", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.DEC(mode)),
        new(0xE6, "INC", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.INC(mode)),
        new(0xF6, "INC", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.INC(mode)),
        new(0xEE, "INC", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.INC(mode)),
        new(0xFE, "INC", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.INC(mode)),
        // Status Flag Changes
        new(0x18, "CLC", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.ResetFlag(CpuFlags.Carry)),
        new(0xD8, "CLD", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.ResetFlag(CpuFlags.DecimalMode)),
        new(0x58, "CLI", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.ResetFlag(CpuFlags.InterruptDisable)),
        new(0xB8, "CLV", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.ResetFlag(CpuFlags.Overflow)),
        new(0x38, "SEC", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.SetFlag(CpuFlags.Carry)),
        new(0xF8, "SED", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.SetFlag(CpuFlags.DecimalMode)),
        new(0x78, "SEI", 1, 2, AddressingMode.NoAddressing, (cpu, _) => cpu.SetFlag(CpuFlags.InterruptDisable)),
        // Jumps and Calls
        new(0x4C, "JMP", 3, 3, AddressingMode.Absolute, (cpu, mode) => cpu.JMP(mode)),
        new(0x6C, "JMP", 3, 5, AddressingMode.Indirect, (cpu, mode) => cpu.JMP(mode)),
        new(0x20, "JSR", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.JSR(mode)),
        new(0x60, "RTS", 1, 6, AddressingMode.NoAddressing, (cpu, _) => cpu.RTS()),
        new(0x40, "RTI", 1, 6, AddressingMode.NoAddressing, (cpu, _) => cpu.RTI()),
        // BIT
        new(0x24, "BIT", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.BIT(mode)),
        new(0x2C, "BIT", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.BIT(mode)),
        // Illegal Opcodes
        // NOP
        new(0x04, "*NOP", 2, 3, AddressingMode.ZeroPage, (_, _) => { }),
        new(0x44, "*NOP", 2, 3, AddressingMode.ZeroPage, (_, _) => { }),
        new(0x64, "*NOP", 2, 3, AddressingMode.ZeroPage, (_, _) => { }),
        new(0x0C, "*NOP", 3, 4, AddressingMode.Absolute, (_, _) => { }),
        new(0x14, "*NOP", 2, 4, AddressingMode.ZeroPage_X, (_, _) => { }),
        new(0x34, "*NOP", 2, 4, AddressingMode.ZeroPage_X, (_, _) => { }),
        new(0x54, "*NOP", 2, 4, AddressingMode.ZeroPage_X, (_, _) => { }),
        new(0x74, "*NOP", 2, 4, AddressingMode.ZeroPage_X, (_, _) => { }),
        new(0xD4, "*NOP", 2, 4, AddressingMode.ZeroPage_X, (_, _) => { }),
        new(0xF4, "*NOP", 2, 4, AddressingMode.ZeroPage_X, (_, _) => { }),
        new(0x1A, "*NOP", 1, 2, AddressingMode.NoAddressing, (_, _) => { }),
        new(0x3A, "*NOP", 1, 2, AddressingMode.NoAddressing, (_, _) => { }),
        new(0x5A, "*NOP", 1, 2, AddressingMode.NoAddressing, (_, _) => { }),
        new(0x7A, "*NOP", 1, 2, AddressingMode.NoAddressing, (_, _) => { }),
        new(0xDA, "*NOP", 1, 2, AddressingMode.NoAddressing, (_, _) => { }),
        new(0xFA, "*NOP", 1, 2, AddressingMode.NoAddressing, (_, _) => { }),
        new(0x80, "*NOP", 2, 2, AddressingMode.Immediate, (_, _) => { }),
        new(0x1C, "*NOP", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.GetOperandAddress(mode, true)),
        new(0x3C, "*NOP", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.GetOperandAddress(mode, true)),
        new(0x5C, "*NOP", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.GetOperandAddress(mode, true)),
        new(0x7C, "*NOP", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.GetOperandAddress(mode, true)),
        new(0xDC, "*NOP", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.GetOperandAddress(mode, true)),
        new(0xFC, "*NOP", 3, 4, AddressingMode.Absolute_X, (cpu, mode) => cpu.GetOperandAddress(mode, true)),
        // LAX
        new(0xA3, "*LAX", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.LAX(mode)),
        new(0xA7, "*LAX", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.LAX(mode)),
        new(0xAF, "*LAX", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.LAX(mode)),
        new(0xB3, "*LAX", 2, 5, AddressingMode.Indirect_Y, (cpu, mode) => cpu.LAX(mode)),
        new(0xB7, "*LAX", 2, 4, AddressingMode.ZeroPage_Y, (cpu, mode) => cpu.LAX(mode)),
        new(0xBF, "*LAX", 3, 4, AddressingMode.Absolute_Y, (cpu, mode) => cpu.LAX(mode)),
        // SAX
        new(0x83, "*SAX", 2, 6, AddressingMode.Indirect_X, (cpu, mode) => cpu.SAX(mode)),
        new(0x87, "*SAX", 2, 3, AddressingMode.ZeroPage, (cpu, mode) => cpu.SAX(mode)),
        new(0x8F, "*SAX", 3, 4, AddressingMode.Absolute, (cpu, mode) => cpu.SAX(mode)),
        new(0x93, "*SAX", 2, 6, AddressingMode.Indirect_Y, (cpu, mode) => cpu.SAX(mode)),
        new(0x97, "*SAX", 2, 4, AddressingMode.ZeroPage_Y, (cpu, mode) => cpu.SAX(mode)),
        new(0x9F, "*SAX", 3, 5, AddressingMode.Absolute_Y, (cpu, mode) => cpu.SAX(mode)),
        // SBC
        new(0xEB, "*SBC", 2, 2, AddressingMode.Immediate, (cpu, mode) => cpu.SBC(mode)),
        // DCP
        new(0xC3, "*DCP", 2, 8, AddressingMode.Indirect_X, (cpu, mode) => cpu.DCP(mode)),
        new(0xC7, "*DCP", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.DCP(mode)),
        new(0xCF, "*DCP", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.DCP(mode)),
        new(0xD3, "*DCP", 2, 8, AddressingMode.Indirect_Y, (cpu, mode) => cpu.DCP(mode)),
        new(0xD7, "*DCP", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.DCP(mode)),
        new(0xDB, "*DCP", 3, 7, AddressingMode.Absolute_Y, (cpu, mode) => cpu.DCP(mode)),
        new(0xDF, "*DCP", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.DCP(mode)),
        // ISB/C
        new(0xE3, "*ISB", 2, 8, AddressingMode.Indirect_X, (cpu, mode) => cpu.ISC(mode)),
        new(0xE7, "*ISB", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.ISC(mode)),
        new(0xEF, "*ISB", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.ISC(mode)),
        new(0xF3, "*ISB", 2, 8, AddressingMode.Indirect_Y, (cpu, mode) => cpu.ISC(mode)),
        new(0xF7, "*ISB", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.ISC(mode)),
        new(0xFB, "*ISB", 3, 7, AddressingMode.Absolute_Y, (cpu, mode) => cpu.ISC(mode)),
        new(0xFF, "*ISB", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.ISC(mode)),
        // SLO
        new(0x03, "*SLO", 2, 8, AddressingMode.Indirect_X, (cpu, mode) => cpu.SLO(mode)),
        new(0x07, "*SLO", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.SLO(mode)),
        new(0x0F, "*SLO", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.SLO(mode)),
        new(0x13, "*SLO", 2, 8, AddressingMode.Indirect_Y, (cpu, mode) => cpu.SLO(mode)),
        new(0x17, "*SLO", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.SLO(mode)),
        new(0x1B, "*SLO", 3, 7, AddressingMode.Absolute_Y, (cpu, mode) => cpu.SLO(mode)),
        new(0x1F, "*SLO", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.SLO(mode)),
        // RLA
        new(0x23, "*RLA", 2, 8, AddressingMode.Indirect_X, (cpu, mode) => cpu.RLA(mode)),
        new(0x27, "*RLA", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.RLA(mode)),
        new(0x2F, "*RLA", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.RLA(mode)),
        new(0x33, "*RLA", 2, 8, AddressingMode.Indirect_Y, (cpu, mode) => cpu.RLA(mode)),
        new(0x37, "*RLA", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.RLA(mode)),
        new(0x3B, "*RLA", 3, 7, AddressingMode.Absolute_Y, (cpu, mode) => cpu.RLA(mode)),
        new(0x3F, "*RLA", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.RLA(mode)),
        // SRE
        new(0x43, "*SRE", 2, 8, AddressingMode.Indirect_X, (cpu, mode) => cpu.SRE(mode)),
        new(0x47, "*SRE", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.SRE(mode)),
        new(0x4F, "*SRE", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.SRE(mode)),
        new(0x53, "*SRE", 2, 8, AddressingMode.Indirect_Y, (cpu, mode) => cpu.SRE(mode)),
        new(0x57, "*SRE", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.SRE(mode)),
        new(0x5B, "*SRE", 3, 7, AddressingMode.Absolute_Y, (cpu, mode) => cpu.SRE(mode)),
        new(0x5F, "*SRE", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.SRE(mode)),
        // RRA
        new(0x63, "*RRA", 2, 8, AddressingMode.Indirect_X, (cpu, mode) => cpu.RRA(mode)),
        new(0x67, "*RRA", 2, 5, AddressingMode.ZeroPage, (cpu, mode) => cpu.RRA(mode)),
        new(0x6F, "*RRA", 3, 6, AddressingMode.Absolute, (cpu, mode) => cpu.RRA(mode)),
        new(0x73, "*RRA", 2, 8, AddressingMode.Indirect_Y, (cpu, mode) => cpu.RRA(mode)),
        new(0x77, "*RRA", 2, 6, AddressingMode.ZeroPage_X, (cpu, mode) => cpu.RRA(mode)),
        new(0x7B, "*RRA", 3, 7, AddressingMode.Absolute_Y, (cpu, mode) => cpu.RRA(mode)),
        new(0x7F, "*RRA", 3, 7, AddressingMode.Absolute_X, (cpu, mode) => cpu.RRA(mode)),
    }.ToDictionary(x => x.Opcode);

    private ushort programCounter;

    private byte registerA;
    private byte registerX;
    private byte registerY;
    private byte registerS;
    private CpuFlags status = CpuFlags.None;

    public byte RegisterA => registerA;
    public byte RegisterX => registerX;
    public byte RegisterY => registerY;
    public byte RegisterS => registerS;
    public CpuFlags Status => status;

    private int cycle = 0;

    public ushort PC
    {
        get => programCounter;
        private set => programCounter = value;
    }

    public override string ToString()
    {
        return GetTrace();
    }

    public string GetTrace()
    {
        var instruction = GetInstruction(PC);
        var bytes = GetInstructionBytes(instruction, PC);
        var bytesFormatted = string.Join(" ", bytes.Select(x => x.ToString("X2"))).PadRight(8);
        var instructionStr = PrintOpcodeWithParameters(instruction, bytes).PadRight(31);
        return
            $"{PC:X4}  {bytesFormatted} {instructionStr}  A:{RegisterA:X2} X:{RegisterX:X2} Y:{RegisterY:X2} P:{(int)Status:X2} SP:{RegisterS:X2}";
    }

    private IReadOnlyList<byte> GetInstructionBytes(Instruction instruction, ushort address)
    {
        List<byte> bytes = [instruction.Opcode];
        if (instruction.Byte >= 2)
            bytes.Add(MemReadByte((ushort)(address + 1)));
        if (instruction.Byte == 3)
            bytes.Add(MemReadByte((ushort)(address + 2)));
        return bytes;
    }

    private string PrintOpcodeWithParameters(Instruction instruction, IReadOnlyList<byte> bytes)
    {
        ushort address = 0;
        if (instruction.AddressingMode is not AddressingMode.NoAddressing and not AddressingMode.Accumulator)
            address = GetOperandAddress(instruction.AddressingMode, (ushort)(PC + 1));
        var parameter = instruction.AddressingMode switch
        {
            AddressingMode.Immediate => $"#${bytes[1]:X2}",
            AddressingMode.Relative => $"${PC + (sbyte)(bytes[1]) + 2:X4}",
            AddressingMode.ZeroPage => $"${bytes[1]:X2} = {MemReadByte(address):X2}",
            AddressingMode.ZeroPage_X => $"${bytes[1]:X2},X @ {address:X2} = {MemReadByte(address):X2}",
            AddressingMode.ZeroPage_Y => $"${bytes[1]:X2},Y @ {address:X2} = {MemReadByte(address):X2}",
            AddressingMode.Absolute => $"${bytes[2]:X2}{bytes[1]:X2}",
            AddressingMode.Absolute_X => $"${bytes[2]:X2}{bytes[1]:X2},X @ {address:X4} = {MemReadByte(address):X2}",
            AddressingMode.Absolute_Y => $"${bytes[2]:X2}{bytes[1]:X2},Y @ {address:X4} = {MemReadByte(address):X2}",
            AddressingMode.Indirect => $"(${bytes[2]:X2}{bytes[1]:X2}) = {address:X4}",
            AddressingMode.Indirect_X =>
                $"(${bytes[1]:X2},X) @ {0xff & bytes[1] + registerX:X2} = {address:X4} = {MemReadByte(address):X2}",
            AddressingMode.Indirect_Y =>
                $"(${bytes[1]:X2}),Y = {MemReadShortZeroPage(bytes[1]):X4} @ {address:X4} = {MemReadByte(address):X2}",
            AddressingMode.Accumulator => "A",
            AddressingMode.NoAddressing => "",
            _ => throw new ArgumentOutOfRangeException(),
        };
        if (instruction.AddressingMode == AddressingMode.Absolute &&
            instruction.Name != "JMP" &&
            instruction.Name != "JSR" &&
            bus.CanDebugRead(address))
        {
            parameter += $" = {MemReadByte(address):X2}";
        }

        return $"{instruction.Name,4} {parameter}";
    }

    public void Interpret(byte[] program)
    {
        Load(program, 0x8000);
        Reset(0x8000);
        
        var brk = false;
        while (!brk)
        {
            (brk, _) = Step();
        }
    }

    private void Load(byte[] rom, ushort location)
    {
        bus.Load(location, rom);
    }

    private void Reset(ushort pcAddress)
    {
        Reset();
        PC = pcAddress;
    }

    public void Reset()
    {
        registerA = 0;
        registerX = 0;
        registerY = 0;
        registerS = 0xFD;
        status = CpuFlags.InterruptDisable | CpuFlags.Unused;

        PC = MemReadShort(0xFFFC);
        cycle = 7;
    }

    public Instruction GetInstruction(ushort address)
    {
        var code = MemReadByte(address);
        if (!Instructions.TryGetValue(code, out var opcode))
        {
            throw new NotSupportedException($"Instruction 0x{code:X} not supported");
        }
        return opcode;
    }

    public (bool Break, int Cycles) Step()
    {
        this.cycle = 0;
        var opcode = GetInstruction(PC++);
        var pcBefore = PC;

        if (opcode.Opcode == 0x00) // BRK
            return (true, opcode.Cycles);

        opcode.Action(this, opcode.AddressingMode);

        // check if we jumped during the action
        if (PC == pcBefore)
            PC += (ushort)(opcode.Byte - 1);

        cycle += opcode.Cycles;
        return (false, cycle);
    }

    public void InterruptNmi()
    {
        StackPush(PC);
        PHP();
        
        SetFlag(CpuFlags.InterruptDisable);
        PC = MemReadShort(0xfffA);
    }

    private void ADC(AddressingMode mode, bool addCycle = true)
    {
        var address = GetOperandAddress(mode, addCycle);
        var param = MemReadByte(address);

        ADCImpl(param);
    }

    private void SBC(AddressingMode mode, bool addCycle = true)
    {
        var address = GetOperandAddress(mode, addCycle);
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

    private void AND(AddressingMode mode, bool addCycle = true)
    {
        var address = GetOperandAddress(mode, addCycle);
        var param = MemReadByte(address);

        registerA &= param;
        UpdateZeroAndNegativeFlags(registerA);
    }

    private void EOR(AddressingMode mode, bool addCycle = true)
    {
        var address = GetOperandAddress(mode, addCycle);
        var param = MemReadByte(address);

        registerA ^= param;
        UpdateZeroAndNegativeFlags(registerA);
    }

    private void ORA(AddressingMode mode, bool addCycle = true)
    {
        var address = GetOperandAddress(mode, addCycle);
        var param = MemReadByte(address);

        registerA |= param;
        UpdateZeroAndNegativeFlags(registerA);
    }

    private void CMP(AddressingMode mode, byte register, bool addCycle = true)
    {
        var address = GetOperandAddress(mode, addCycle);
        var param = MemReadByte(address);

        var value = register - param;
        UpdateZeroAndNegativeFlags((byte)value);
        UpdateFlags(CpuFlags.Carry, value >= 0);
    }

    private void ASL(AddressingMode mode)
    {
        var param = RegisterA;
        ushort address = 0;
        if (mode != AddressingMode.Accumulator)
        {
            address = GetOperandAddress(mode);
            param = MemReadByte(address);
        }

        UpdateFlags(CpuFlags.Carry, (param & 0x80) != 0);
        param <<= 1;

        UpdateZeroAndNegativeFlags(param);

        if (mode != AddressingMode.Accumulator)
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
        if (mode != AddressingMode.Accumulator)
        {
            address = GetOperandAddress(mode);
            param = MemReadByte(address);
        }

        UpdateFlags(CpuFlags.Carry, (param & 0x1) != 0);
        param >>= 1;

        UpdateZeroAndNegativeFlags(param);

        if (mode != AddressingMode.Accumulator)
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
        if (mode != AddressingMode.Accumulator)
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

        if (mode != AddressingMode.Accumulator)
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
        if (mode != AddressingMode.Accumulator)
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

        if (mode != AddressingMode.Accumulator)
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
        var address = GetOperandAddress(mode, true);
        var param = MemReadByte(address);

        registerA = param;
        UpdateZeroAndNegativeFlags(registerA);
    }

    private void LDX(AddressingMode mode)
    {
        var address = GetOperandAddress(mode, true);
        var param = MemReadByte(address);

        registerX = param;
        UpdateZeroAndNegativeFlags(registerX);
    }

    private void LDY(AddressingMode mode)
    {
        var address = GetOperandAddress(mode, true);
        var param = MemReadByte(address);

        registerY = param;
        UpdateZeroAndNegativeFlags(registerY);
    }

    private void LAX(AddressingMode mode)
    {
        LDA(mode);
        registerX = registerA;
        UpdateZeroAndNegativeFlags(registerX);
    }

    private void STA(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        MemWriteByte(address, RegisterA);
    }

    private void STX(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        MemWriteByte(address, RegisterX);
    }

    private void STY(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        MemWriteByte(address, RegisterY);
    }

    private void SAX(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        MemWriteByte(address, (byte)(RegisterA & RegisterX));
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

    private void DEC(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var value = MemReadByte(address);

        value--;

        MemWriteByte(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void INC(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var value = MemReadByte(address);

        value++;

        MemWriteByte(address, value);
        UpdateZeroAndNegativeFlags(value);
    }

    private void BCC()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (!TestFlag(CpuFlags.Carry))
        {
            TakeBranch(value);
        }
    }

    private void BCS()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (TestFlag(CpuFlags.Carry))
        {
            TakeBranch(value);
        }
    }

    private void BEQ()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (TestFlag(CpuFlags.Zero))
        {
            TakeBranch(value);
        }
    }

    private void BNE()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (!TestFlag(CpuFlags.Zero))
        {
            TakeBranch(value);
        }
    }

    private void BMI()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (TestFlag(CpuFlags.Negative))
        {
            TakeBranch(value);
        }
    }

    private void BPL()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (!TestFlag(CpuFlags.Negative))
        {
            TakeBranch(value);
        }
    }

    private void BVC()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (!TestFlag(CpuFlags.Overflow))
        {
            TakeBranch(value);
        }
    }

    private void BVS()
    {
        var address = GetOperandAddress(AddressingMode.Relative);
        var value = (sbyte)MemReadByte(address);

        if (TestFlag(CpuFlags.Overflow))
        {
            TakeBranch(value);
        }
    }
    
    private void TakeBranch(sbyte value)
    {
        var pcBefore = PC + 1;
        PC = (ushort)(PC + value + 1);
        cycle += 1;
        if (value < 0)
            AddCycleIfPageBoundaryCrossed(PC, pcBefore);
        else
            AddCycleIfPageBoundaryCrossed(pcBefore, PC);
    }

    private void JMP(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        PC = address;
    }

    private void JSR(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var returnAddress = (ushort)(PC + 1);
        var low = (byte)returnAddress;
        var high = (byte)(returnAddress >> 8);

        StackPush(high);
        StackPush(low);
        PC = address;
    }

    private void RTS()
    {
        var low = StackPop();
        var high = StackPop();
        var newAddress = (ushort)(high << 8 | low);
        PC = (ushort)(newAddress + 1);
    }

    private void RTI()
    {
        PLP();
        var low = StackPop();
        var high = StackPop();
        var newAddress = (ushort)(high << 8 | low);
        PC = newAddress;
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
        StackPush(RegisterA);
    }

    private void PHP()
    {
        var value = status;
        value |= CpuFlags.BreakCommand;
        StackPush((byte)value);
    }

    private void PLA()
    {
        registerA = StackPop();
        UpdateZeroAndNegativeFlags(RegisterA);
    }

    private void PLP()
    {
        var value = (CpuFlags)StackPop();
        status = (value & ~CpuFlags.BreakCommand) | CpuFlags.Unused;
    }

    private void BIT(AddressingMode mode)
    {
        var address = GetOperandAddress(mode);
        var param = MemReadByte(address);

        UpdateFlags(CpuFlags.Zero, (param & registerA) == 0);
        status = (CpuFlags)((byte)status & 0b00111111 | (param & 0b11000000));
    }

    private void DCP(AddressingMode mode)
    {
        DEC(mode);
        CMP(mode, registerA, false);
    }

    private void ISC(AddressingMode mode)
    {
        INC(mode);
        SBC(mode, false);
    }

    private void SLO(AddressingMode mode)
    {
        ASL(mode);
        ORA(mode, false);
    }

    private void RLA(AddressingMode mode)
    {
        ROL(mode);
        AND(mode, false);
    }

    private void SRE(AddressingMode mode)
    {
        LSR(mode);
        EOR(mode, false);
    }

    private void RRA(AddressingMode mode)
    {
        ROR(mode);
        ADC(mode, false);
    }

    private void UpdateZeroAndNegativeFlags(byte value)
    {
        UpdateFlags(CpuFlags.Zero, value == 0);
        UpdateFlags(CpuFlags.Negative, (sbyte)value < 0);
    }

    private void UpdateFlags(CpuFlags flag, bool value)
    {
        if (value)
        {
            SetFlag(flag);
        }
        else
        {
            ResetFlag(flag);
        }
    }

    private void SetFlag(CpuFlags flag)
    {
        status |= flag;
    }

    private void ResetFlag(CpuFlags flag)
    {
        status &= ~flag;
    }

    private bool TestFlag(CpuFlags flag)
    {
        return (Status & flag) != 0;
    }

    public byte MemReadByte(ushort address)
    {
        return bus.MemRead(address);
    }

    public void MemWriteByte(ushort address, byte value)
    {
        bus.MemWrite(address, value);
    }

    private void StackPush(ushort value)
    {
        var high = (byte)(value >> 8);
        var low = (byte)value;
        MemWriteByte((ushort)(0x100 + registerS--), high);
        MemWriteByte((ushort)(0x100 + registerS--), low);
    }
    
    private void StackPush(byte value)
    {
        MemWriteByte((ushort)(0x100 + registerS--), value);
    }

    private byte StackPop()
    {
        return MemReadByte((ushort)(0x100 + ++registerS));
    }

    [UsedImplicitly]
    public byte StackPeek()
    {
        return MemReadByte((ushort)(0x100 + registerS + 1));
    }

    public ushort MemReadShort(ushort address)
    {
        ushort low = bus.MemRead(address);
        ushort high = bus.MemRead((ushort)(address + 1));

        return (ushort)(high << 8 | low);
    }

    private ushort MemReadShortZeroPage(int address)
    {
        var low = MemReadByte((ushort)(0xff & address));
        // Add 1 but wrap around page boundary
        var highAddress = (ushort)(0xff & (address + 1));
        var high = MemReadByte(highAddress);

        return (ushort)(high << 8 | low);
    }

    public void MemWriteShort(ushort address, ushort value)
    {
        var low = (byte)value;
        var high = (byte)(value >> 8);

        bus.MemWrite(address, low);
        bus.MemWrite((ushort)(address + 1), high);
    }

    public void MemWriteShortZeroPage(int address, ushort value)
    {
        var low = (byte)value;
        var high = (byte)(value >> 8);
        var lowAddress = (ushort)(0xff & address);

        bus.MemWrite(lowAddress, low);
        // Add 1 but wrap around page boundary
        var highAddress = (ushort)(0xff & (address + 1));
        bus.MemWrite(highAddress, high);
    }


    private ushort GetOperandAddress(AddressingMode mode, bool addCycle = false)
    {
        return GetOperandAddress(mode, PC, addCycle);
    }

    private ushort GetOperandAddress(AddressingMode mode, ushort position, bool addCycle = false)
    {
        return mode switch
        {
            AddressingMode.Immediate => position,
            AddressingMode.Relative => position,
            AddressingMode.ZeroPage => MemReadByte(position),
            AddressingMode.Absolute => MemReadShort(position),
            AddressingMode.ZeroPage_X => (byte)(MemReadByte(position) + registerX),
            AddressingMode.ZeroPage_Y => (byte)(MemReadByte(position) + registerY),
            AddressingMode.Absolute_X => Absolute(registerX),
            AddressingMode.Absolute_Y => Absolute(registerY),
            AddressingMode.Indirect => ReadIndirectJmpAddress(MemReadShort(position)),
            AddressingMode.Indirect_X
                => ReadIndirectX(MemReadByte(position)),
            AddressingMode.Indirect_Y
                => ReadIndirectY(MemReadByte(position)),
            AddressingMode.NoAddressing
                => throw new InvalidOperationException($"Mode {mode} is not supported"),
            AddressingMode.Accumulator
                => throw new InvalidOperationException($"Mode {mode} is not supported"),
            _ => throw new ArgumentOutOfRangeException(nameof(mode)),
        };

        ushort Absolute(byte register)
        {
            var baseAddr = MemReadShort(position);
            var target = (ushort)(baseAddr + register);
            if (addCycle)
                AddCycleIfPageBoundaryCrossed(baseAddr, target);
            return target;
        }

        ushort ReadIndirectX(byte operand)
        {
            return MemReadShortZeroPage(operand + registerX);
        }

        ushort ReadIndirectY(byte operand)
        {
            var baseAddr = MemReadShortZeroPage(operand);
            var target = (ushort)(baseAddr + registerY);
            if (addCycle)
                AddCycleIfPageBoundaryCrossed(baseAddr, target);
            return target;
        }

        ushort ReadIndirectJmpAddress(ushort lowAddress)
        {
            var lo = MemReadByte(lowAddress);
            // Add 1 but wrap around page boundary
            var highAddress = (ushort)(0xFF00 & lowAddress | 0x00FF & lowAddress + 1);
            var hi = MemReadByte(highAddress);
            return (ushort)(hi << 8 | lo);
        }
    }

    private void AddCycleIfPageBoundaryCrossed(int before, int after)
    {
        if (after % 0x100 < before % 0x100)
            cycle += 1;
    }

    public void SetPc(ushort value)
    {
        programCounter = value;
    }

    public void SetRegisterA(byte value)
    {
        registerA = value;
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
    public record struct Instruction(
        byte Opcode,
        string Name,
        byte Byte,
        // ReSharper disable once NotAccessedPositionalProperty.Global
        int Cycles,
        AddressingMode AddressingMode,
        Action<Cpu, AddressingMode> Action);
}

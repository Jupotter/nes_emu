var program = new byte[]{0xa9, 0xc0, 0xaa, 0xe8, 0x00};

var cpu = new NesEmu.Cpu();
cpu.Interpret(program);

Console.WriteLine(cpu);

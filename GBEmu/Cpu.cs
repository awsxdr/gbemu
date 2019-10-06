namespace GBEmu
{
    using System;
    using System.Linq.Expressions;

    public class Cpu
    {
        public CpuRegisters Registers { get; } = new CpuRegisters();
        private readonly IBus _bus;

        public Cpu(IBus bus)
        {
            Registers.PC = 0x100;
            _bus = bus;
        }

        public void Next()
        {
            var instruction = ParseNextInstruction();
            instruction.Method();
        }

        private (int ClockCycles, Action Method) ParseNextInstruction() =>
            _bus.Read(Registers.PC++) switch {
                0x01 => (12, Load16IntoRegister(() => Registers.BC, ReadImmediate16)),
                0x02 => (8, LoadRegisterIntoMemory(() => Registers.BC, () => Registers.A)),
                0x06 => (8, LoadNextByteIntoRegister(() => Registers.B)),
                0x08 => (20, LoadStackPointerIntoMemory),
                0x0a => (8, LoadRegisterIntoRegister(() => Registers.A, () => _bus.Read(Registers.BC))),
                0x0e => (8, LoadNextByteIntoRegister(() => Registers.C)),
                0x11 => (12, Load16IntoRegister(() => Registers.DE, ReadImmediate16)),
                0x12 => (8, LoadRegisterIntoMemory(() => Registers.DE, () => Registers.A)),
                0x16 => (8, LoadNextByteIntoRegister(() => Registers.D)),
                0x1a => (8, LoadRegisterIntoRegister(() => Registers.A, () => _bus.Read(Registers.DE))),
                0x1e => (8, LoadNextByteIntoRegister(() => Registers.E)),
                0x21 => (12, Load16IntoRegister(() => Registers.HL, ReadImmediate16)),
                0x22 => (8, LoadRegisterIntoMemory(() => Registers.HL++, () => Registers.A)),
                0x26 => (8, LoadNextByteIntoRegister(() => Registers.H)),
                0x2a => (8, LoadRegisterIntoRegister(() => Registers.A, () => _bus.Read(Registers.HL++))),
                0x2e => (8, LoadNextByteIntoRegister(() => Registers.L)),
                0x31 => (12, Load16IntoRegister(() => Registers.SP, ReadImmediate16)),
                0x32 => (8, LoadRegisterIntoMemory(() => Registers.HL--, () => Registers.A)),
                0x36 => (12, LoadRegisterIntoMemory(() => Registers.HL, () => _bus.Read(Registers.PC++))),
                0x3a => (8, LoadRegisterIntoRegister(() => Registers.A, () => _bus.Read(Registers.HL--))),
                0x3e => (8, LoadRegisterIntoRegister(() => Registers.A, () => _bus.Read(Registers.PC++))),
                0x40 => (4, LoadRegisterIntoRegister(() => Registers.B, () => Registers.B)),
                0x41 => (4, LoadRegisterIntoRegister(() => Registers.B, () => Registers.C)),
                0x42 => (4, LoadRegisterIntoRegister(() => Registers.B, () => Registers.D)),
                0x43 => (4, LoadRegisterIntoRegister(() => Registers.B, () => Registers.E)),
                0x44 => (4, LoadRegisterIntoRegister(() => Registers.B, () => Registers.H)),
                0x45 => (4, LoadRegisterIntoRegister(() => Registers.B, () => Registers.L)),
                0x46 => (8, LoadRegisterIntoRegister(() => Registers.B, () => _bus.Read(Registers.HL))),
                0x47 => (4, LoadRegisterIntoRegister(() => Registers.B, () => Registers.A)),
                0x48 => (4, LoadRegisterIntoRegister(() => Registers.C, () => Registers.B)),
                0x49 => (4, LoadRegisterIntoRegister(() => Registers.C, () => Registers.C)),
                0x4a => (4, LoadRegisterIntoRegister(() => Registers.C, () => Registers.D)),
                0x4b => (4, LoadRegisterIntoRegister(() => Registers.C, () => Registers.E)),
                0x4c => (4, LoadRegisterIntoRegister(() => Registers.C, () => Registers.H)),
                0x4d => (4, LoadRegisterIntoRegister(() => Registers.C, () => Registers.L)),
                0x4e => (8, LoadRegisterIntoRegister(() => Registers.C, () => _bus.Read(Registers.HL))),
                0x4f => (4, LoadRegisterIntoRegister(() => Registers.C, () => Registers.A)),
                0x50 => (4, LoadRegisterIntoRegister(() => Registers.D, () => Registers.B)),
                0x51 => (4, LoadRegisterIntoRegister(() => Registers.D, () => Registers.C)),
                0x52 => (4, LoadRegisterIntoRegister(() => Registers.D, () => Registers.D)),
                0x53 => (4, LoadRegisterIntoRegister(() => Registers.D, () => Registers.E)),
                0x54 => (4, LoadRegisterIntoRegister(() => Registers.D, () => Registers.H)),
                0x55 => (4, LoadRegisterIntoRegister(() => Registers.D, () => Registers.L)),
                0x56 => (8, LoadRegisterIntoRegister(() => Registers.D, () => _bus.Read(Registers.HL))),
                0x57 => (4, LoadRegisterIntoRegister(() => Registers.D, () => Registers.A)),
                0x58 => (4, LoadRegisterIntoRegister(() => Registers.E, () => Registers.B)),
                0x59 => (4, LoadRegisterIntoRegister(() => Registers.E, () => Registers.C)),
                0x5a => (4, LoadRegisterIntoRegister(() => Registers.E, () => Registers.D)),
                0x5b => (4, LoadRegisterIntoRegister(() => Registers.E, () => Registers.E)),
                0x5c => (4, LoadRegisterIntoRegister(() => Registers.E, () => Registers.H)),
                0x5d => (4, LoadRegisterIntoRegister(() => Registers.E, () => Registers.L)),
                0x5e => (8, LoadRegisterIntoRegister(() => Registers.E, () => _bus.Read(Registers.HL))),
                0x5f => (4, LoadRegisterIntoRegister(() => Registers.E, () => Registers.A)),
                0x60 => (4, LoadRegisterIntoRegister(() => Registers.H, () => Registers.B)),
                0x61 => (4, LoadRegisterIntoRegister(() => Registers.H, () => Registers.C)),
                0x62 => (4, LoadRegisterIntoRegister(() => Registers.H, () => Registers.D)),
                0x63 => (4, LoadRegisterIntoRegister(() => Registers.H, () => Registers.E)),
                0x64 => (4, LoadRegisterIntoRegister(() => Registers.H, () => Registers.H)),
                0x65 => (4, LoadRegisterIntoRegister(() => Registers.H, () => Registers.L)),
                0x66 => (8, LoadRegisterIntoRegister(() => Registers.H, () => _bus.Read(Registers.HL))),
                0x67 => (4, LoadRegisterIntoRegister(() => Registers.H, () => Registers.A)),
                0x68 => (4, LoadRegisterIntoRegister(() => Registers.L, () => Registers.B)),
                0x69 => (4, LoadRegisterIntoRegister(() => Registers.L, () => Registers.C)),
                0x6a => (4, LoadRegisterIntoRegister(() => Registers.L, () => Registers.D)),
                0x6b => (4, LoadRegisterIntoRegister(() => Registers.L, () => Registers.E)),
                0x6c => (4, LoadRegisterIntoRegister(() => Registers.L, () => Registers.H)),
                0x6d => (4, LoadRegisterIntoRegister(() => Registers.L, () => Registers.L)),
                0x6e => (8, LoadRegisterIntoRegister(() => Registers.L, () => _bus.Read(Registers.HL))),
                0x6f => (4, LoadRegisterIntoRegister(() => Registers.L, () => Registers.A)),
                0x70 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.B)),
                0x71 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.C)),
                0x72 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.D)),
                0x73 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.E)),
                0x74 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.H)),
                0x75 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.L)),
                0x77 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.A)),
                0x78 => (4, LoadRegisterIntoRegister(() => Registers.A, () => Registers.B)),
                0x79 => (4, LoadRegisterIntoRegister(() => Registers.A, () => Registers.C)),
                0x7a => (4, LoadRegisterIntoRegister(() => Registers.A, () => Registers.D)),
                0x7b => (4, LoadRegisterIntoRegister(() => Registers.A, () => Registers.E)),
                0x7c => (4, LoadRegisterIntoRegister(() => Registers.A, () => Registers.H)),
                0x7d => (4, LoadRegisterIntoRegister(() => Registers.A, () => Registers.L)),
                0x7e => (8, LoadRegisterIntoRegister(() => Registers.A, () => _bus.Read(Registers.HL))),
                0x7f => (4, LoadRegisterIntoRegister(() => Registers.A, () => Registers.A)),
                0x80 => (4, Add(() => Registers.B)),
                0x81 => (4, Add(() => Registers.C)),
                0x82 => (4, Add(() => Registers.D)),
                0x83 => (4, Add(() => Registers.E)),
                0x84 => (4, Add(() => Registers.H)),
                0x85 => (4, Add(() => Registers.L)),
                0x86 => (8, Add(() => _bus.Read(Registers.HL))),
                0x87 => (4, Add(() => Registers.A)),
                0x88 => (4, AddWithCarry(() => Registers.B)),
                0x89 => (4, AddWithCarry(() => Registers.C)),
                0x8a => (4, AddWithCarry(() => Registers.D)),
                0x8b => (4, AddWithCarry(() => Registers.E)),
                0x8c => (4, AddWithCarry(() => Registers.H)),
                0x8d => (4, AddWithCarry(() => Registers.L)),
                0x8e => (8, AddWithCarry(() => _bus.Read(Registers.HL))),
                0x8f => (4, AddWithCarry(() => Registers.A)),
                0x90 => (4, Subtract(() => Registers.B)),
                0x91 => (4, Subtract(() => Registers.C)),
                0x92 => (4, Subtract(() => Registers.D)),
                0x93 => (4, Subtract(() => Registers.E)),
                0x94 => (4, Subtract(() => Registers.H)),
                0x95 => (4, Subtract(() => Registers.L)),
                0x96 => (8, Subtract(() => _bus.Read(Registers.HL))),
                0x97 => (4, Subtract(() => Registers.A)),
                0x98 => (4, SubtractWithCarry(() => Registers.B)),
                0x99 => (4, SubtractWithCarry(() => Registers.C)),
                0x9a => (4, SubtractWithCarry(() => Registers.D)),
                0x9b => (4, SubtractWithCarry(() => Registers.E)),
                0x9c => (4, SubtractWithCarry(() => Registers.H)),
                0x9d => (4, SubtractWithCarry(() => Registers.L)),
                0x9e => (8, SubtractWithCarry(() => _bus.Read(Registers.HL))),
                0x9f => (4, SubtractWithCarry(() => Registers.A)),
                0xc1 => (12, Pop(() => Registers.BC)),
                0xc5 => (16, Push(() => Registers.BC)),
                0xc6 => (8, Add(() => _bus.Read(Registers.PC++))),
                0xce => (8, AddWithCarry(() => _bus.Read(Registers.PC++))),
                0xd1 => (12, Pop(() => Registers.DE)),
                0xd5 => (16, Push(() => Registers.DE)),
                0xd6 => (8, Subtract(() => _bus.Read(Registers.PC++))),
                0xde => (8, SubtractWithCarry(() => _bus.Read(Registers.PC++))),
                0xe0 => (12, LoadAccumulatorIntoIo(() => _bus.Read(Registers.PC++))),
                0xe1 => (12, Pop(() => Registers.HL)),
                0xe2 => (8, LoadAccumulatorIntoIo(() => Registers.C)),
                0xe5 => (16, Push(() => Registers.HL)),
                0xea => (16, LoadAccumulatorIntoImmediatePointer),
                0xf0 => (12, LoadIoIntoAccumulator(() => _bus.Read(Registers.PC++))),
                0xf1 => (12, Pop(() => Registers.AF)),
                0xf2 => (8, LoadIoIntoAccumulator(() => Registers.C)),
                0xf5 => (16, Push(() => Registers.AF)),
                0xf8 => (12, Load16IntoRegister(() => Registers.HL, () => (ushort)(Registers.SP + (sbyte)_bus.Read(Registers.PC++)))),
                0xf9 => (8, Load16IntoRegister(() => Registers.SP, () => Registers.HL)),
                0xfa => (16, LoadImmediatePointerIntoAccumulator),
                _ => (0, Nop)
                };

        private Action LoadNextByteIntoRegister(Expression<Func<byte>> register)
        {
            var loadMethod = Expression.Lambda(Expression.Assign(register.Body, Expression.Constant(_bus.Read(Registers.PC++)))).Compile();

            return () => loadMethod.DynamicInvoke();
        }

        private Action LoadRegisterIntoRegister(Expression<Func<byte>> toRegister, Func<byte> fromRegister)
        {
            var loadMethod = Expression.Lambda(Expression.Assign(toRegister.Body, Expression.Constant(fromRegister()))).Compile();

            return () => loadMethod.DynamicInvoke();
        }

        private Action LoadRegisterIntoMemory(Func<Register16> pointer, Func<byte> register) =>
            () => _bus.Write(pointer(), register());

        private void LoadAccumulatorIntoImmediatePointer() =>
            _bus.Write(ReadImmediate16(), Registers.A);

        private void LoadImmediatePointerIntoAccumulator() =>
            Registers.A = _bus.Read(ReadImmediate16());

        private Action LoadIoIntoAccumulator(Func<byte> register) =>
            () => Registers.A = _bus.Read((ushort)(0xff00 | register()));

        private Action LoadAccumulatorIntoIo(Func<byte> register) =>
            () => _bus.Write((ushort) (0xff00 | register()), Registers.A);

        private Action Load16IntoRegister(Expression<Func<Register16>> register, Func<ushort> value)
        {
            var loadMethod = Expression.Lambda(Expression.Assign(register.Body, Expression.Convert(Expression.Constant(value()), typeof(Register16)))).Compile();

            return () => loadMethod.DynamicInvoke();
        }

        private void LoadStackPointerIntoMemory()
        {
            var address = (ushort) (_bus.Read(Registers.PC++) | (_bus.Read(Registers.PC++) << 8));
            _bus.Write(address, Registers.SP.Lower);
            _bus.Write((ushort)(address + 1), Registers.SP.Upper);
        }

        private Action Push(Func<Register16> register) => () =>
        {
            var registerValue = register();
            _bus.Write(Registers.SP--, registerValue.Lower);
            _bus.Write(Registers.SP--, registerValue.Upper);
        };

        private Action Pop(Expression<Func<Register16>> register) =>
            Load16IntoRegister(register, () => (ushort) (_bus.Read(Registers.SP++) | (_bus.Read(Registers.SP++) << 8)));

        private Action Add(Func<byte> register) => () =>
        {
            var value = register();
            var result = Registers.A + value;

            UpdateFlag(Flags.Carry, result > 0xff);
            UnsetFlag(Flags.Subtract);
            result &= 0xff;
            UpdateFlag(Flags.Zero, result == 0);
            UpdateFlag(Flags.HalfCarry, (value & 0x0f) + (Registers.A & 0x0f) >= 0x10);

            Registers.A = (byte)result;
        };

        private Action AddWithCarry(Func<byte> register) =>
            Add(() => (byte)(register() + (GetFlag(Flags.Carry) ? 1 : 0)));

        private Action Subtract(Func<byte> register) => () =>
        {
            var value = register();
            var result = Registers.A - value;

            UpdateFlag(Flags.Carry, result < 0);
            SetFlag(Flags.Subtract);
            result &= 0xff;
            UpdateFlag(Flags.Zero, result == 0);
            UpdateFlag(Flags.HalfCarry, (Registers.A & 0x0f) - (value & 0x0f) < 0);

            Registers.A = (byte)result;
        };

        private Action SubtractWithCarry(Func<byte> register) =>
            Subtract(() => (byte)(register() + (GetFlag(Flags.Carry) ? 1 : 0)));

        private ushort ReadImmediate16() =>
            (ushort) (_bus.Read(Registers.PC++) | (_bus.Read(Registers.PC++) << 8));

        private void UpdateFlag(Flags flag, bool value)
        {
            if (value) SetFlag(flag);
            else UnsetFlag(flag);
        }

        private bool GetFlag(Flags flag) =>
            (Registers.F & (byte) flag) != 0;

        private void SetFlag(Flags flag)
        {
            Registers.F |= (byte) flag;
        }

        private void UnsetFlag(Flags flag)
        {
            Registers.F &= (byte) ~(byte) flag;
        }

        private void Nop()
        {
        }
    }

    public enum Flags
    {
        Zero = 0b10000000,
        Subtract = 0b01000000,
        HalfCarry = 0b00100000,
        Carry = 0b00010000,
    }
}
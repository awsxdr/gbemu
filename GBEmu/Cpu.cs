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
                0x04 => (4, IncrementRegister(() => Registers.B)),
                0x05 => (4, DecrementRegister(() => Registers.B)),
                0x06 => (8, LoadNextByteIntoRegister(() => Registers.B)),
                0x08 => (20, LoadStackPointerIntoMemory),
                0x0a => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.BC))),
                0x0c => (4, IncrementRegister(() => Registers.C)),
                0x0d => (4, DecrementRegister(() => Registers.C)),
                0x0e => (8, LoadNextByteIntoRegister(() => Registers.C)),
                0x11 => (12, Load16IntoRegister(() => Registers.DE, ReadImmediate16)),
                0x12 => (8, LoadRegisterIntoMemory(() => Registers.DE, () => Registers.A)),
                0x14 => (4, IncrementRegister(() => Registers.D)),
                0x15 => (4, DecrementRegister(() => Registers.D)),
                0x16 => (8, LoadNextByteIntoRegister(() => Registers.D)),
                0x1a => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.DE))),
                0x1c => (4, IncrementRegister(() => Registers.E)),
                0x1d => (4, DecrementRegister(() => Registers.E)),
                0x1e => (8, LoadNextByteIntoRegister(() => Registers.E)),
                0x21 => (12, Load16IntoRegister(() => Registers.HL, ReadImmediate16)),
                0x22 => (8, LoadRegisterIntoMemory(() => Registers.HL++, () => Registers.A)),
                0x24 => (4, IncrementRegister(() => Registers.H)),
                0x25 => (4, DecrementRegister(() => Registers.H)),
                0x26 => (8, LoadNextByteIntoRegister(() => Registers.H)),
                0x2a => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.HL++))),
                0x2c => (4, IncrementRegister(() => Registers.L)),
                0x2d => (4, DecrementRegister(() => Registers.L)),
                0x2e => (8, LoadNextByteIntoRegister(() => Registers.L)),
                0x31 => (12, Load16IntoRegister(() => Registers.SP, ReadImmediate16)),
                0x32 => (8, LoadRegisterIntoMemory(() => Registers.HL--, () => Registers.A)),
                0x34 => (12, IncrementMemory),
                0x35 => (12, DecrementMemory),
                0x36 => (12, LoadRegisterIntoMemory(() => Registers.HL, () => _bus.Read(Registers.PC++))),
                0x3a => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.HL--))),
                0x3c => (4, IncrementRegister(() => Registers.A)),
                0x3d => (4, DecrementRegister(() => Registers.A)),
                0x3e => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.PC++))),
                0x40 => (4, LoadValueIntoRegister(() => Registers.B, () => Registers.B)),
                0x41 => (4, LoadValueIntoRegister(() => Registers.B, () => Registers.C)),
                0x42 => (4, LoadValueIntoRegister(() => Registers.B, () => Registers.D)),
                0x43 => (4, LoadValueIntoRegister(() => Registers.B, () => Registers.E)),
                0x44 => (4, LoadValueIntoRegister(() => Registers.B, () => Registers.H)),
                0x45 => (4, LoadValueIntoRegister(() => Registers.B, () => Registers.L)),
                0x46 => (8, LoadValueIntoRegister(() => Registers.B, () => _bus.Read(Registers.HL))),
                0x47 => (4, LoadValueIntoRegister(() => Registers.B, () => Registers.A)),
                0x48 => (4, LoadValueIntoRegister(() => Registers.C, () => Registers.B)),
                0x49 => (4, LoadValueIntoRegister(() => Registers.C, () => Registers.C)),
                0x4a => (4, LoadValueIntoRegister(() => Registers.C, () => Registers.D)),
                0x4b => (4, LoadValueIntoRegister(() => Registers.C, () => Registers.E)),
                0x4c => (4, LoadValueIntoRegister(() => Registers.C, () => Registers.H)),
                0x4d => (4, LoadValueIntoRegister(() => Registers.C, () => Registers.L)),
                0x4e => (8, LoadValueIntoRegister(() => Registers.C, () => _bus.Read(Registers.HL))),
                0x4f => (4, LoadValueIntoRegister(() => Registers.C, () => Registers.A)),
                0x50 => (4, LoadValueIntoRegister(() => Registers.D, () => Registers.B)),
                0x51 => (4, LoadValueIntoRegister(() => Registers.D, () => Registers.C)),
                0x52 => (4, LoadValueIntoRegister(() => Registers.D, () => Registers.D)),
                0x53 => (4, LoadValueIntoRegister(() => Registers.D, () => Registers.E)),
                0x54 => (4, LoadValueIntoRegister(() => Registers.D, () => Registers.H)),
                0x55 => (4, LoadValueIntoRegister(() => Registers.D, () => Registers.L)),
                0x56 => (8, LoadValueIntoRegister(() => Registers.D, () => _bus.Read(Registers.HL))),
                0x57 => (4, LoadValueIntoRegister(() => Registers.D, () => Registers.A)),
                0x58 => (4, LoadValueIntoRegister(() => Registers.E, () => Registers.B)),
                0x59 => (4, LoadValueIntoRegister(() => Registers.E, () => Registers.C)),
                0x5a => (4, LoadValueIntoRegister(() => Registers.E, () => Registers.D)),
                0x5b => (4, LoadValueIntoRegister(() => Registers.E, () => Registers.E)),
                0x5c => (4, LoadValueIntoRegister(() => Registers.E, () => Registers.H)),
                0x5d => (4, LoadValueIntoRegister(() => Registers.E, () => Registers.L)),
                0x5e => (8, LoadValueIntoRegister(() => Registers.E, () => _bus.Read(Registers.HL))),
                0x5f => (4, LoadValueIntoRegister(() => Registers.E, () => Registers.A)),
                0x60 => (4, LoadValueIntoRegister(() => Registers.H, () => Registers.B)),
                0x61 => (4, LoadValueIntoRegister(() => Registers.H, () => Registers.C)),
                0x62 => (4, LoadValueIntoRegister(() => Registers.H, () => Registers.D)),
                0x63 => (4, LoadValueIntoRegister(() => Registers.H, () => Registers.E)),
                0x64 => (4, LoadValueIntoRegister(() => Registers.H, () => Registers.H)),
                0x65 => (4, LoadValueIntoRegister(() => Registers.H, () => Registers.L)),
                0x66 => (8, LoadValueIntoRegister(() => Registers.H, () => _bus.Read(Registers.HL))),
                0x67 => (4, LoadValueIntoRegister(() => Registers.H, () => Registers.A)),
                0x68 => (4, LoadValueIntoRegister(() => Registers.L, () => Registers.B)),
                0x69 => (4, LoadValueIntoRegister(() => Registers.L, () => Registers.C)),
                0x6a => (4, LoadValueIntoRegister(() => Registers.L, () => Registers.D)),
                0x6b => (4, LoadValueIntoRegister(() => Registers.L, () => Registers.E)),
                0x6c => (4, LoadValueIntoRegister(() => Registers.L, () => Registers.H)),
                0x6d => (4, LoadValueIntoRegister(() => Registers.L, () => Registers.L)),
                0x6e => (8, LoadValueIntoRegister(() => Registers.L, () => _bus.Read(Registers.HL))),
                0x6f => (4, LoadValueIntoRegister(() => Registers.L, () => Registers.A)),
                0x70 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.B)),
                0x71 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.C)),
                0x72 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.D)),
                0x73 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.E)),
                0x74 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.H)),
                0x75 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.L)),
                0x77 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.A)),
                0x78 => (4, LoadValueIntoRegister(() => Registers.A, () => Registers.B)),
                0x79 => (4, LoadValueIntoRegister(() => Registers.A, () => Registers.C)),
                0x7a => (4, LoadValueIntoRegister(() => Registers.A, () => Registers.D)),
                0x7b => (4, LoadValueIntoRegister(() => Registers.A, () => Registers.E)),
                0x7c => (4, LoadValueIntoRegister(() => Registers.A, () => Registers.H)),
                0x7d => (4, LoadValueIntoRegister(() => Registers.A, () => Registers.L)),
                0x7e => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.HL))),
                0x7f => (4, LoadValueIntoRegister(() => Registers.A, () => Registers.A)),
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
                0xa0 => (4, And(() => Registers.B)),
                0xa1 => (4, And(() => Registers.C)),
                0xa2 => (4, And(() => Registers.D)),
                0xa3 => (4, And(() => Registers.E)),
                0xa4 => (4, And(() => Registers.H)),
                0xa5 => (4, And(() => Registers.L)),
                0xa6 => (8, And(() => _bus.Read(Registers.HL))),
                0xa7 => (4, And(() => Registers.A)),
                0xa8 => (4, Xor(() => Registers.B)),
                0xa9 => (4, Xor(() => Registers.C)),
                0xaa => (4, Xor(() => Registers.D)),
                0xab => (4, Xor(() => Registers.E)),
                0xac => (4, Xor(() => Registers.H)),
                0xad => (4, Xor(() => Registers.L)),
                0xae => (8, Xor(() => _bus.Read(Registers.HL))),
                0xaf => (4, Xor(() => Registers.A)),
                0xb0 => (4, Or(() => Registers.B)),
                0xb1 => (4, Or(() => Registers.C)),
                0xb2 => (4, Or(() => Registers.D)),
                0xb3 => (4, Or(() => Registers.E)),
                0xb4 => (4, Or(() => Registers.H)),
                0xb5 => (4, Or(() => Registers.L)),
                0xb6 => (8, Or(() => _bus.Read(Registers.HL))),
                0xb7 => (4, Or(() => Registers.A)),
                0xb8 => (4, Compare(() => Registers.B)),
                0xb9 => (4, Compare(() => Registers.C)),
                0xba => (4, Compare(() => Registers.D)),
                0xbb => (4, Compare(() => Registers.E)),
                0xbc => (4, Compare(() => Registers.H)),
                0xbd => (4, Compare(() => Registers.L)),
                0xbe => (8, Compare(() => _bus.Read(Registers.HL))),
                0xbf => (4, Compare(() => Registers.A)),
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
                0xe6 => (8, And(() => _bus.Read(Registers.PC++))),
                0xea => (16, LoadAccumulatorIntoImmediatePointer),
                0xee => (8, Xor(() => _bus.Read(Registers.PC++))),
                0xf0 => (12, LoadIoIntoAccumulator(() => _bus.Read(Registers.PC++))),
                0xf1 => (12, Pop(() => Registers.AF)),
                0xf2 => (8, LoadIoIntoAccumulator(() => Registers.C)),
                0xf5 => (16, Push(() => Registers.AF)),
                0xf6 => (8, Or(() => _bus.Read(Registers.PC++))),
                0xf8 => (12, Load16IntoRegister(() => Registers.HL, () => (ushort)(Registers.SP + (sbyte)_bus.Read(Registers.PC++)))),
                0xf9 => (8, Load16IntoRegister(() => Registers.SP, () => Registers.HL)),
                0xfa => (16, LoadImmediatePointerIntoAccumulator),
                0xfe => (8, Compare(() => _bus.Read(Registers.PC++))),
                _ => (0, Nop)
                };

        private Action LoadNextByteIntoRegister(Expression<Func<byte>> register)
        {
            var loadMethod = Expression.Lambda(Expression.Assign(register.Body, Expression.Constant(_bus.Read(Registers.PC++)))).Compile();

            return () => loadMethod.DynamicInvoke();
        }

        private Action LoadValueIntoRegister(Expression<Func<byte>> register, Func<byte> value)
        {
            var loadMethod = Expression.Lambda(Expression.Assign(register.Body, Expression.Constant(value()))).Compile();

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

        private Action And(Func<byte> register) => () =>
        {
            Registers.A &= register();

            UpdateFlag(Flags.Zero, Registers.A == 0);
            UnsetFlag(Flags.Subtract);
            SetFlag(Flags.HalfCarry);
            UnsetFlag(Flags.Carry);
        };

        private Action Or(Func<byte> register) => () =>
        {
            Registers.A |= register();

            UpdateFlag(Flags.Zero, Registers.A == 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            UnsetFlag(Flags.Carry);
        };

        private Action Xor(Func<byte> register) => () =>
        {
            Registers.A ^= register();

            UpdateFlag(Flags.Zero, Registers.A == 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            UnsetFlag(Flags.Carry);
        };

        private Action Compare(Func<byte> register) => () =>
        {
            var value = register();
            var result = Registers.A - value;

            UpdateFlag(Flags.Carry, result < 0);
            SetFlag(Flags.Subtract);
            result &= 0xff;
            UpdateFlag(Flags.Zero, result == 0);
            UpdateFlag(Flags.HalfCarry, (Registers.A & 0x0f) - (value & 0x0f) < 0);
        };

        private Action IncrementRegister(Expression<Func<byte>> register)
        {
            var incrementMethod = (Func<byte>)
                Expression.Lambda(
                    Expression.Assign(
                        register.Body,
                        Expression.Convert(
                            Expression.Add(
                                Expression.Convert(register.Body, typeof(int)),
                                Expression.Constant(1)),
                            typeof(byte))))
                    .Compile();
            return () =>
            {
                var result = incrementMethod();

                UpdateFlag(Flags.Zero, result == 0);
                UnsetFlag(Flags.Subtract);
                UpdateFlag(Flags.HalfCarry, (result & 0x0f) == 0);
            };
        }

        private void IncrementMemory()
        {
            var result = (byte) (_bus.Read(Registers.HL) + 1);
            _bus.Write(Registers.HL, result);

            UpdateFlag(Flags.Zero, result == 0);
            UnsetFlag(Flags.Subtract);
            UpdateFlag(Flags.HalfCarry, (result & 0x0f) == 0);
        }

        private Action DecrementRegister(Expression<Func<byte>> register)
        {
            var incrementMethod = (Func<byte>)
                Expression.Lambda(
                        Expression.Assign(
                            register.Body,
                            Expression.Convert(
                                Expression.Subtract(
                                    Expression.Convert(register.Body, typeof(int)),
                                    Expression.Constant(1)),
                                typeof(byte))))
                    .Compile();
            return () =>
            {
                var result = incrementMethod();

                UpdateFlag(Flags.Zero, result == 0);
                UnsetFlag(Flags.Subtract);
                UpdateFlag(Flags.HalfCarry, (result & 0x0f) == 0x0f);
            };
        }

        private void DecrementMemory()
        {
            var result = (byte)(_bus.Read(Registers.HL) - 1);
            _bus.Write(Registers.HL, result);

            UpdateFlag(Flags.Zero, result == 0);
            UnsetFlag(Flags.Subtract);
            UpdateFlag(Flags.HalfCarry, (result & 0x0f) == 0x0f);
        }

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
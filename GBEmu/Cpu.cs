namespace GBEmu
{
    using System;
    using System.Linq.Expressions;

    public class Cpu
    {
        public CpuRegisters Registers { get; } = new CpuRegisters();
        private readonly IBus _bus;
        private bool _interruptsEnabled = true;
        private Action _delayedAction = null;

        public Cpu(IBus bus)
        {
            Registers.PC = 0;
            Registers.SP = 0xfffe;
            _bus = bus;
        }

        public void Next()
        {
            var tempDelayedAction = _delayedAction;
            _delayedAction = null;

            var instruction = ParseNextInstruction();
            instruction.Method();

            tempDelayedAction?.Invoke();
        }

        private (int ClockCycles, Action Method) ParseNextInstruction() =>
            _bus.Read(Registers.PC++) switch {
                0x00 => (4, Nop),
                0x01 => (12, Load16IntoRegister(() => Registers.BC, ReadImmediate16)),
                0x02 => (8, LoadRegisterIntoMemory(() => Registers.BC, () => Registers.A)),
                0x03 => (8, IncrementRegister(() => Registers.BC)),
                0x04 => (4, IncrementRegister(() => Registers.B)),
                0x05 => (4, DecrementRegister(() => Registers.B)),
                0x06 => (8, LoadNextByteIntoRegister(() => Registers.B)),
                0x07 => (4, RotateLeft(() => Registers.A)),
                0x08 => (20, LoadStackPointerIntoMemory),
                0x09 => (8, AddHL(() => Registers.BC)),
                0x0a => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.BC))),
                0x0b => (8, DecrementRegister(() => Registers.BC)),
                0x0c => (4, IncrementRegister(() => Registers.C)),
                0x0d => (4, DecrementRegister(() => Registers.C)),
                0x0e => (8, LoadNextByteIntoRegister(() => Registers.C)),
                0x0f => (4, RotateRight(() => Registers.A)),
                0x10 when _bus.Read(Registers.PC++) == 0x00 => (4, Stop),
                0x11 => (12, Load16IntoRegister(() => Registers.DE, ReadImmediate16)),
                0x12 => (8, LoadRegisterIntoMemory(() => Registers.DE, () => Registers.A)),
                0x13 => (8, IncrementRegister(() => Registers.DE)),
                0x14 => (4, IncrementRegister(() => Registers.D)),
                0x15 => (4, DecrementRegister(() => Registers.D)),
                0x16 => (8, LoadNextByteIntoRegister(() => Registers.D)),
                0x17 => (4, RotateLeftThroughCarry(() => Registers.A)),
                0x18 => (8, JumpRelative),
                0x19 => (8, AddHL(() => Registers.DE)),
                0x1a => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.DE))),
                0x1b => (8, DecrementRegister(() => Registers.DE)),
                0x1c => (4, IncrementRegister(() => Registers.E)),
                0x1d => (4, DecrementRegister(() => Registers.E)),
                0x1e => (8, LoadNextByteIntoRegister(() => Registers.E)),
                0x1f => (4, RotateRightThroughCarry(() => Registers.A)),
                0x20 => (8, JumpRelativeConditional(Flags.Zero, true)),
                0x21 => (12, Load16IntoRegister(() => Registers.HL, ReadImmediate16)),
                0x22 => (8, LoadRegisterIntoMemory(() => Registers.HL++, () => Registers.A)),
                0x23 => (8, IncrementRegister(() => Registers.HL)),
                0x24 => (4, IncrementRegister(() => Registers.H)),
                0x25 => (4, DecrementRegister(() => Registers.H)),
                0x26 => (8, LoadNextByteIntoRegister(() => Registers.H)),
                0x27 => (4, DecimalAdjustAccumulator),
                0x28 => (8, JumpRelativeConditional(Flags.Zero, false)),
                0x29 => (8, AddHL(() => Registers.HL)),
                0x2a => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.HL++))),
                0x2b => (8, DecrementRegister(() => Registers.HL)),
                0x2c => (4, IncrementRegister(() => Registers.L)),
                0x2d => (4, DecrementRegister(() => Registers.L)),
                0x2e => (8, LoadNextByteIntoRegister(() => Registers.L)),
                0x2f => (4, ComplementAccumulator),
                0x30 => (8, JumpRelativeConditional(Flags.Carry, true)),
                0x31 => (12, Load16IntoRegister(() => Registers.SP, ReadImmediate16)),
                0x32 => (8, LoadRegisterIntoMemory(() => Registers.HL--, () => Registers.A)),
                0x33 => (8, IncrementRegister(() => Registers.SP)),
                0x34 => (12, IncrementMemory),
                0x35 => (12, DecrementMemory),
                0x36 => (12, LoadRegisterIntoMemory(() => Registers.HL, () => _bus.Read(Registers.PC++))),
                0x37 => (4, SetCarryFlag),
                0x38 => (8, JumpRelativeConditional(Flags.Carry, false)),
                0x39 => (8, AddHL(() => Registers.SP)),
                0x3a => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.HL--))),
                0x3b => (8, DecrementRegister(() => Registers.SP)),
                0x3c => (4, IncrementRegister(() => Registers.A)),
                0x3d => (4, DecrementRegister(() => Registers.A)),
                0x3e => (8, LoadValueIntoRegister(() => Registers.A, () => _bus.Read(Registers.PC++))),
                0x3f => (4, ComplementCarryFlag),
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
                0x76 => (4, Halt),
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
                0xc0 => (8, ReturnConditional(Flags.Zero, true)),
                0xc1 => (12, Pop(() => Registers.BC)),
                0xc2 => (12, JumpAbsoluteConditional(Flags.Zero, true)),
                0xc3 => (12, JumpAbsolute),
                0xc4 => (12, CallConditional(Flags.Zero, true)),
                0xc5 => (16, Push(() => Registers.BC)),
                0xc6 => (8, Add(() => _bus.Read(Registers.PC++))),
                0xc7 => (32, ResetToAddress(0x00)),
                0xc8 => (8, ReturnConditional(Flags.Zero, false)),
                0xc9 => (8, Return),
                0xca => (12, JumpAbsoluteConditional(Flags.Zero, false)),
                0xcb => ParseExtendedInstruction(),
                0xcc => (12, CallConditional(Flags.Zero, false)),
                0xcd => (12, Call),
                0xce => (8, AddWithCarry(() => _bus.Read(Registers.PC++))),
                0xcf => (32, ResetToAddress(0x08)),
                0xd0 => (8, ReturnConditional(Flags.Carry, true)),
                0xd1 => (12, Pop(() => Registers.DE)),
                0xd2 => (12, JumpAbsoluteConditional(Flags.Carry, true)),
                0xd4 => (12, CallConditional(Flags.Carry, true)),
                0xd5 => (16, Push(() => Registers.DE)),
                0xd6 => (8, Subtract(() => _bus.Read(Registers.PC++))),
                0xd7 => (32, ResetToAddress(0x10)),
                0xd8 => (8, ReturnConditional(Flags.Carry, false)),
                0xd9 => (8, ReturnWithInterruptsEnabled),
                0xda => (12, JumpAbsoluteConditional(Flags.Carry, false)),
                0xdc => (12, CallConditional(Flags.Carry, false)),
                0xde => (8, SubtractWithCarry(() => _bus.Read(Registers.PC++))),
                0xdf => (32, ResetToAddress(0x18)),
                0xe0 => (12, LoadAccumulatorIntoIo(() => _bus.Read(Registers.PC++))),
                0xe1 => (12, Pop(() => Registers.HL)),
                0xe2 => (8, LoadAccumulatorIntoIo(() => Registers.C)),
                0xe5 => (16, Push(() => Registers.HL)),
                0xe6 => (8, And(() => _bus.Read(Registers.PC++))),
                0xe7 => (32, ResetToAddress(0x20)),
                0xe8 => (16, AddSPImmediate8),
                0xe9 => (4, JumpAbsoluteHL),
                0xea => (16, LoadAccumulatorIntoImmediatePointer),
                0xee => (8, Xor(() => _bus.Read(Registers.PC++))),
                0xef => (32, ResetToAddress(0x28)),
                0xf0 => (12, LoadIoIntoAccumulator(() => _bus.Read(Registers.PC++))),
                0xf1 => (12, Pop(() => Registers.AF)),
                0xf2 => (8, LoadIoIntoAccumulator(() => Registers.C)),
                0xf3 => (4, DisableInterrupts),
                0xf5 => (16, Push(() => Registers.AF)),
                0xf6 => (8, Or(() => _bus.Read(Registers.PC++))),
                0xf7 => (32, ResetToAddress(0x30)),
                0xf8 => (12, Load16IntoRegister(() => Registers.HL, () => (ushort)(Registers.SP + (sbyte)_bus.Read(Registers.PC++)))),
                0xf9 => (8, Load16IntoRegister(() => Registers.SP, () => Registers.HL)),
                0xfa => (16, LoadImmediatePointerIntoAccumulator),
                0xfb => (4, EnableInterrupts),
                0xfe => (8, Compare(() => _bus.Read(Registers.PC++))),
                0xff => (32, ResetToAddress(0x38)),
                _ => (0, Nop)
                };

        private (int ClockCycles, Action Method) ParseExtendedInstruction() =>
            _bus.Read(Registers.PC++) switch {
                0x00 => (8, RotateLeft(() => Registers.B)),
                0x01 => (8, RotateLeft(() => Registers.C)),
                0x02 => (8, RotateLeft(() => Registers.D)),
                0x03 => (8, RotateLeft(() => Registers.E)),
                0x04 => (8, RotateLeft(() => Registers.H)),
                0x05 => (8, RotateLeft(() => Registers.L)),
                0x06 => (16, RotateMemoryLeft),
                0x07 => (8, RotateLeft(() => Registers.A)),
                0x08 => (8, RotateRight(() => Registers.B)),
                0x09 => (8, RotateRight(() => Registers.C)),
                0x0a => (8, RotateRight(() => Registers.D)),
                0x0b => (8, RotateRight(() => Registers.E)),
                0x0c => (8, RotateRight(() => Registers.H)),
                0x0d => (8, RotateRight(() => Registers.L)),
                0x0e => (16, RotateMemoryRight),
                0x0f => (8, RotateRight(() => Registers.A)),
                0x10 => (8, RotateLeftThroughCarry(() => Registers.B)),
                0x11 => (8, RotateLeftThroughCarry(() => Registers.C)),
                0x12 => (8, RotateLeftThroughCarry(() => Registers.D)),
                0x13 => (8, RotateLeftThroughCarry(() => Registers.E)),
                0x14 => (8, RotateLeftThroughCarry(() => Registers.H)),
                0x15 => (8, RotateLeftThroughCarry(() => Registers.L)),
                0x16 => (16, RotateMemoryLeftThroughCarry),
                0x17 => (8, RotateLeftThroughCarry(() => Registers.A)),
                0x18 => (8, RotateRightThroughCarry(() => Registers.B)),
                0x19 => (8, RotateRightThroughCarry(() => Registers.C)),
                0x1a => (8, RotateRightThroughCarry(() => Registers.D)),
                0x1b => (8, RotateRightThroughCarry(() => Registers.E)),
                0x1c => (8, RotateRightThroughCarry(() => Registers.H)),
                0x1d => (8, RotateRightThroughCarry(() => Registers.L)),
                0x1e => (16, RotateMemoryRightThroughCarry),
                0x1f => (8, RotateRightThroughCarry(() => Registers.A)),
                0x20 => (8, ShiftLeft(() => Registers.B)),
                0x21 => (8, ShiftLeft(() => Registers.C)),
                0x22 => (8, ShiftLeft(() => Registers.D)),
                0x23 => (8, ShiftLeft(() => Registers.E)),
                0x24 => (8, ShiftLeft(() => Registers.H)),
                0x25 => (8, ShiftLeft(() => Registers.L)),
                0x26 => (16, ShiftMemoryLeft),
                0x27 => (8, ShiftLeft(() => Registers.A)),
                0x28 => (8, ShiftRight(() => Registers.B)),
                0x29 => (8, ShiftRight(() => Registers.C)),
                0x2a => (8, ShiftRight(() => Registers.D)),
                0x2b => (8, ShiftRight(() => Registers.E)),
                0x2c => (8, ShiftRight(() => Registers.H)),
                0x2d => (8, ShiftRight(() => Registers.L)),
                0x2e => (16, ShiftMemoryRight),
                0x2f => (8, ShiftRight(() => Registers.A)),
                0x30 => (8, SwapRegisterNibbles(() => Registers.B)),
                0x31 => (8, SwapRegisterNibbles(() => Registers.C)),
                0x32 => (8, SwapRegisterNibbles(() => Registers.D)),
                0x33 => (8, SwapRegisterNibbles(() => Registers.E)),
                0x34 => (8, SwapRegisterNibbles(() => Registers.H)),
                0x35 => (8, SwapRegisterNibbles(() => Registers.L)),
                0x36 => (16, SwapMemoryNibbles),
                0x37 => (8, SwapRegisterNibbles(() => Registers.A)),
                0x38 => (8, ShiftRightZero(() => Registers.B)),
                0x39 => (8, ShiftRightZero(() => Registers.C)),
                0x3a => (8, ShiftRightZero(() => Registers.D)),
                0x3b => (8, ShiftRightZero(() => Registers.E)),
                0x3c => (8, ShiftRightZero(() => Registers.H)),
                0x3d => (8, ShiftRightZero(() => Registers.L)),
                0x3e => (16, ShiftMemoryRightZero),
                0x3f => (8, ShiftRightZero(() => Registers.A)),
                0x40 => (8, CheckBit(() => Registers.B)),
                0x41 => (8, CheckBit(() => Registers.C)),
                0x42 => (8, CheckBit(() => Registers.D)),
                0x43 => (8, CheckBit(() => Registers.E)),
                0x44 => (8, CheckBit(() => Registers.H)),
                0x45 => (8, CheckBit(() => Registers.L)),
                0x46 => (8, CheckBit(() => _bus.Read(Registers.HL))),
                0x47 => (8, CheckBit(() => Registers.A)),
                0x80 => (8, UnsetBit(() => Registers.B)),
                0x81 => (8, UnsetBit(() => Registers.C)),
                0x82 => (8, UnsetBit(() => Registers.D)),
                0x83 => (8, UnsetBit(() => Registers.E)),
                0x84 => (8, UnsetBit(() => Registers.H)),
                0x85 => (8, UnsetBit(() => Registers.L)),
                0x86 => (16, UnsetMemoryBit),
                0x87 => (8, UnsetBit(() => Registers.A)),
                0xc0 => (8, SetBit(() => Registers.B)),
                0xc1 => (8, SetBit(() => Registers.C)),
                0xc2 => (8, SetBit(() => Registers.D)),
                0xc3 => (8, SetBit(() => Registers.E)),
                0xc4 => (8, SetBit(() => Registers.H)),
                0xc5 => (8, SetBit(() => Registers.L)),
                0xc6 => (16, SetMemoryBit),
                0xc7 => (8, SetBit(() => Registers.A)),
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

        private Action AddHL(Func<Register16> register) => () =>
        {
            var left = Registers.HL;
            var right = register();

            Registers.HL += right;

            UnsetFlag(Flags.Subtract);
            UpdateFlag(Flags.HalfCarry, (left & 0x0f00) != ((left + right) & 0x0f00));
            UpdateFlag(Flags.Carry, left + right > 0xffff);
        };

        private void AddSPImmediate8()
        {
            Registers.SP += _bus.Read(Registers.PC++);
        }

        private Action IncrementRegister(Expression<Func<Register16>> register)
        {
            var incrementMethod = (Func<Register16>)
                Expression.Lambda(
                        Expression.Assign(
                            register.Body,
                            Expression.Convert(
                                Expression.Add(
                                    Expression.Convert(register.Body, typeof(ushort)),
                                    Expression.Constant((ushort)1)),
                                typeof(Register16))))
                    .Compile();
            return () => incrementMethod();
        }


        private Action DecrementRegister(Expression<Func<Register16>> register)
        {
            var decrementMethod = (Func<Register16>)
                Expression.Lambda(
                        Expression.Assign(
                            register.Body,
                            Expression.Convert(
                                Expression.Subtract(
                                    Expression.Convert(register.Body, typeof(ushort)),
                                    Expression.Constant((ushort)1)),
                                typeof(Register16))))
                    .Compile();
            return () => decrementMethod();
        }

        private Action SwapRegisterNibbles(Expression<Func<byte>> register)
        {
            var (getMethod, setMethod) = GetGetterSetterMethods(register);

            return () =>
            {
                var value = getMethod();
                var result = (byte)(((value & 0x0f) << 4) + ((value & 0xf0) >> 4));

                UpdateFlag(Flags.Zero, result == 0);
                UnsetFlag(Flags.Subtract);
                UnsetFlag(Flags.HalfCarry);
                UnsetFlag(Flags.Carry);

                setMethod(result);
            };
        }

        private void SwapMemoryNibbles()
        {
            var value = _bus.Read(Registers.HL);
            var result = (byte)(((value & 0x0f) << 4) + ((value & 0xf0) >> 4));

            UpdateFlag(Flags.Zero, result == 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            UnsetFlag(Flags.Carry);

            _bus.Write(Registers.HL, result);
        }

        private void DecimalAdjustAccumulator()
        {
            Registers.A = (byte) int.Parse(Registers.A.ToString(), System.Globalization.NumberStyles.HexNumber);

            UpdateFlag(Flags.Zero, Registers.A == 0);
        }

        private void ComplementAccumulator()
        {
            Registers.A = (byte) ~Registers.A;

            SetFlag(Flags.Subtract);
            SetFlag(Flags.HalfCarry);
        }

        private void ComplementCarryFlag()
        {
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            UpdateFlag(Flags.Carry, !GetFlag(Flags.Carry));
        }

        private void SetCarryFlag()
        {
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            SetFlag(Flags.Carry);
        }

        private void Nop()
        {
        }

        private void Halt()
        {
        }

        private void Stop()
        {
        }

        private void DisableInterrupts()
        {
            _delayedAction = () => _interruptsEnabled = false;
        }

        private void EnableInterrupts()
        {
            _delayedAction = () => _interruptsEnabled = true;
        }

        private Action RotateLeft(Expression<Func<byte>> register)
        {
            var (getMethod, setMethod) = GetGetterSetterMethods(register);

            return () =>
            {
                var value = getMethod();
                UpdateFlag(Flags.Carry, (value & 0b10000000) > 0);
                UnsetFlag(Flags.Subtract);
                UnsetFlag(Flags.HalfCarry);
                value <<= 1;
                value = (byte) (value | (GetFlag(Flags.Carry) ? 1 : 0));

                UpdateFlag(Flags.Zero, value == 0);

                setMethod(value);
            };
        }

        private void RotateMemoryLeft()
        {
            var value = _bus.Read(Registers.HL);
            UpdateFlag(Flags.Carry, (value & 0b10000000) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value <<= 1;
            value = (byte)(value | (GetFlag(Flags.Carry) ? 1 : 0));

            UpdateFlag(Flags.Zero, value == 0);

            _bus.Write(Registers.HL, value);
        }

        private Action RotateLeftThroughCarry(Expression<Func<byte>> register)
        {
            var (getMethod, setMethod) = GetGetterSetterMethods(register);

            return () =>
            {
                var value = getMethod();
                var oldCarryValue = GetFlag(Flags.Carry);
                UpdateFlag(Flags.Carry, (value & 0b10000000) > 0);
                UnsetFlag(Flags.Subtract);
                UnsetFlag(Flags.HalfCarry);
                value <<= 1;
                value = (byte)(value | (oldCarryValue ? 1 : 0));

                UpdateFlag(Flags.Zero, value == 0);

                setMethod(value);
            };
        }

        private void RotateMemoryLeftThroughCarry()
        {
            var value = _bus.Read(Registers.HL);
            var oldCarryValue = GetFlag(Flags.Carry);
            UpdateFlag(Flags.Carry, (value & 0b10000000) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value <<= 1;
            value = (byte)(value | (oldCarryValue ? 1 : 0));

            UpdateFlag(Flags.Zero, value == 0);

            _bus.Write(Registers.HL, value);
        }

        private Action RotateRight(Expression<Func<byte>> register)
        {
            var (getMethod, setMethod) = GetGetterSetterMethods(register);

            return () =>
            {
                var value = getMethod();
                UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
                UnsetFlag(Flags.Subtract);
                UnsetFlag(Flags.HalfCarry);
                value >>= 1;
                value = (byte)(value | (GetFlag(Flags.Carry) ? 0b10000000 : 0));

                UpdateFlag(Flags.Zero, value == 0);

                setMethod(value);
            };
        }

        private void RotateMemoryRight()
        {
            var value = _bus.Read(Registers.HL);
            UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value >>= 1;
            value = (byte)(value | (GetFlag(Flags.Carry) ? 0b10000000 : 0));

            UpdateFlag(Flags.Zero, value == 0);

            _bus.Write(Registers.HL, value);
        }

        private Action RotateRightThroughCarry(Expression<Func<byte>> register)
        {
            var (getMethod, setMethod) = GetGetterSetterMethods(register);

            return () =>
            {
                var value = getMethod();
                var oldCarryValue = GetFlag(Flags.Carry);
                UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
                UnsetFlag(Flags.Subtract);
                UnsetFlag(Flags.HalfCarry);
                value >>= 1;
                value = (byte) (value | (oldCarryValue ? 0b10000000 : 0));

                UpdateFlag(Flags.Zero, value == 0);

                setMethod(value);
            };
        }

        private void RotateMemoryRightThroughCarry()
        {
            var value = _bus.Read(Registers.HL);
            var oldCarryValue = GetFlag(Flags.Carry);
            UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value >>= 1;
            value = (byte)(value | (oldCarryValue ? 0b10000000 : 0));

            UpdateFlag(Flags.Zero, value == 0);

            _bus.Write(Registers.HL, value);
        }

        private Action ShiftLeft(Expression<Func<byte>> register)
        {
            var (getMethod, setMethod) = GetGetterSetterMethods(register);

            return () =>
            {
                var value = getMethod();
                UpdateFlag(Flags.Carry, (value & 0b10000000) > 0);
                UnsetFlag(Flags.Subtract);
                UnsetFlag(Flags.HalfCarry);
                value <<= 1;

                UpdateFlag(Flags.Zero, value == 0);

                setMethod(value);
            };
        }

        private void ShiftMemoryLeft()
        {
            var value = _bus.Read(Registers.HL);
            UpdateFlag(Flags.Carry, (value & 0b10000000) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value <<= 1;

            UpdateFlag(Flags.Zero, value == 0);

            _bus.Write(Registers.HL, value);
        }

        private Action ShiftRight(Expression<Func<byte>> register)
        {
            var (getMethod, setMethod) = GetGetterSetterMethods(register);

            return () =>
            {
                var value = getMethod();
                UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
                UnsetFlag(Flags.Subtract);
                UnsetFlag(Flags.HalfCarry);
                value >>= 1;
                value = (byte)(value | ((value & 0b01000000) > 0 ? 0b10000000 : 0));

                UpdateFlag(Flags.Zero, value == 0);

                setMethod(value);
            };
        }

        private void ShiftMemoryRight()
        {
            var value = _bus.Read(Registers.HL);
            UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value >>= 1;
            value = (byte)(value | ((value & 0b01000000) > 0 ? 0b10000000 : 0));

            UpdateFlag(Flags.Zero, value == 0);

            _bus.Write(Registers.HL, value);
        }

        private Action ShiftRightZero(Expression<Func<byte>> register)
        {
            var (getMethod, setMethod) = GetGetterSetterMethods(register);

            return () =>
            {
                var value = getMethod();
                UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
                UnsetFlag(Flags.Subtract);
                UnsetFlag(Flags.HalfCarry);
                value >>= 1;

                UpdateFlag(Flags.Zero, value == 0);

                setMethod(value);
            };
        }

        private void ShiftMemoryRightZero()
        {
            var value = _bus.Read(Registers.HL);
            UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value >>= 1;

            UpdateFlag(Flags.Zero, value == 0);

            _bus.Write(Registers.HL, value);
        }

        private Action CheckBit(Func<byte> value) =>
            () =>
            {
                UpdateFlag(Flags.Zero, (value() & (1 << _bus.Read(Registers.PC++))) == 0);
                UnsetFlag(Flags.Subtract);
                SetFlag(Flags.HalfCarry);
            };

        private Action SetBit(Expression<Func<byte>> register)
        {
            var (get, set) = GetGetterSetterMethods(register);

            return () =>
                set((byte)(get() | (1 << _bus.Read(Registers.PC++))));
        }

        private void SetMemoryBit() =>
            _bus.Write(Registers.HL, (byte)(_bus.Read(Registers.HL) | (1 << _bus.Read(Registers.PC++))));

        private Action UnsetBit(Expression<Func<byte>> register)
        {
            var (get, set) = GetGetterSetterMethods(register);

            return () =>
                set((byte)(get() & ~(1 << _bus.Read(Registers.PC++))));
        }

        private void UnsetMemoryBit() =>
            _bus.Write(Registers.HL, (byte)(_bus.Read(Registers.HL) & ~(1 << _bus.Read(Registers.PC++))));

        private void JumpAbsolute()
        {
            Registers.PC = ReadImmediate16();
        }

        private Action JumpAbsoluteConditional(Flags flag, bool invert) => () =>
        {
            var address = ReadImmediate16();
            if (GetFlag(flag) != invert)
                Registers.PC = address;
        };

        private void JumpAbsoluteHL()
        {
            Registers.PC = Registers.HL;
        }

        private void JumpRelative()
        {
            var offset = (sbyte) _bus.Read(Registers.PC++);
            Registers.PC = (ushort)(Registers.PC + offset);
        }

        private Action JumpRelativeConditional(Flags flag, bool invert) => () =>
        {
            var offset = (sbyte) _bus.Read(Registers.PC++);
            if(GetFlag(flag) != invert)
                Registers.PC = (ushort) (Registers.PC + offset);
        };

        private void Call()
        {
            var address = ReadImmediate16();
            Push(() => Registers.PC)();
            Registers.PC = address;
        }

        private Action CallConditional(Flags flag, bool invert) => () =>
        {
            var address = ReadImmediate16();
            if (GetFlag(flag) != invert)
            {
                Push(() => Registers.PC)();
                Registers.PC = address;
            }
        };

        private Action ResetToAddress(byte addressOffset) => () =>
        {
            Push(() => Registers.PC)();
            Registers.PC = addressOffset;
        };

        private void Return() =>
            Pop(() => Registers.PC)();

        private Action ReturnConditional(Flags flag, bool invert) => () =>
        {
            if (GetFlag(flag) != invert)
            {
                Pop(() => Registers.PC)();
            }
        };

        private void ReturnWithInterruptsEnabled()
        {
            Pop(() => Registers.PC)();
            _interruptsEnabled = true;
        }

        private (Func<byte>, Func<byte, byte>) GetGetterSetterMethods(Expression<Func<byte>> register)
        {
            var getMethod = (Func<byte>)Expression.Lambda(register.Body).Compile();

            var valueParameter = Expression.Parameter(typeof(byte));
            var setMethod = (Func<byte, byte>)Expression
                .Lambda(Expression.Assign(register.Body, valueParameter), valueParameter).Compile();

            return (getMethod, setMethod);
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
    }

    public enum Flags
    {
        Zero = 0b10000000,
        Subtract = 0b01000000,
        HalfCarry = 0b00100000,
        Carry = 0b00010000,
    }
}
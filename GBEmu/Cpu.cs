namespace GBEmu
{
    using System;
    using System.Diagnostics;

    public class Cpu
    {
        public CpuRegisters Registers { get; } = new CpuRegisters();
        private readonly IBus _bus;
        private bool _interruptsEnabled = true;
        private Action _delayedAction = null;

        public Cpu(IBus bus)
        {
            Registers.PC = 0;
            Registers.SP = 0xffff;
            _bus = bus;
        }

        public void ConnectClock(Clock clock)
        {
            var ticksUntilNextInstruction = 0;
            clock.Tick += () =>
            {
                if (ticksUntilNextInstruction-- > 0)
                    return;

                ticksUntilNextInstruction = Next();
            };
        }

        public int Next()
        {
            var tempDelayedAction = _delayedAction;
            _delayedAction = null;

            if (Registers.PC == 0xc2c4)
            {
                int a = 0;
            }

            var instruction = ParseNextInstruction();
            instruction.Method();

            tempDelayedAction?.Invoke();

            return instruction.ClockCycles;
        }

        public void Interrupt(Interrupts interrupt)
        {
            if (!_interruptsEnabled) return;
            _interruptsEnabled = false;

            var requestedInterrupts = (Interrupts) _bus.Read(0xffff);
            var interruptsToExecute = requestedInterrupts & interrupt;

            var address =
                interruptsToExecute.HasFlag(Interrupts.Joypad) ? 0x0060
                : interruptsToExecute.HasFlag(Interrupts.Serial) ? 0x0058
                : interruptsToExecute.HasFlag(Interrupts.Timer) ? 0x0050
                : interruptsToExecute.HasFlag(Interrupts.LcdStatus) ? 0x0048
                : interruptsToExecute.HasFlag(Interrupts.VerticalBlank) ? 0x0040
                : -1;

            if (address >= 0)
            {
                Push(() => Registers.PC)();
                Registers.PC = (ushort)address;
                _bus.Write(0xff0f, 0x00);
            }

            _interruptsEnabled = true;
        }

        private (int ClockCycles, Action Method) ParseNextInstruction() =>
            _bus.Read(Registers.PC++) switch
            {
                0x00 => (4, Nop),
                0x01 => (12, Load16IntoRegister(Register16.BC, ReadImmediate16)),
                0x02 => (8, LoadRegisterIntoMemory(() => Registers.BC, () => Registers.A)),
                0x03 => (8, IncrementRegister(Register16.BC)),
                0x04 => (4, IncrementRegister(Register8.B)),
                0x05 => (4, DecrementRegister(Register8.B)),
                0x06 => (8, LoadNextByteIntoRegister(Register8.B)),
                0x07 => (4, RotateLeft(Register8.A, neverZero: true)),
                0x08 => (20, LoadStackPointerIntoMemory),
                0x09 => (8, AddHL(() => Registers.BC)),
                0x0a => (8, LoadValueIntoRegister(Register8.A, () => _bus.Read(Registers.BC))),
                0x0b => (8, DecrementRegister(Register16.BC)),
                0x0c => (4, IncrementRegister(Register8.C)),
                0x0d => (4, DecrementRegister(Register8.C)),
                0x0e => (8, LoadNextByteIntoRegister(Register8.C)),
                0x0f => (4, RotateRight(Register8.A, neverZero: true)),
                0x10 when _bus.Read(Registers.PC++) == 0x00 => (4, Stop),
                0x11 => (12, Load16IntoRegister(Register16.DE, ReadImmediate16)),
                0x12 => (8, LoadRegisterIntoMemory(() => Registers.DE, () => Registers.A)),
                0x13 => (8, IncrementRegister(Register16.DE)),
                0x14 => (4, IncrementRegister(Register8.D)),
                0x15 => (4, DecrementRegister(Register8.D)),
                0x16 => (8, LoadNextByteIntoRegister(Register8.D)),
                0x17 => (4, RotateLeftThroughCarry(Register8.A, neverZero: true)),
                0x18 => (8, JumpRelative),
                0x19 => (8, AddHL(() => Registers.DE)),
                0x1a => (8, LoadValueIntoRegister(Register8.A, () => _bus.Read(Registers.DE))),
                0x1b => (8, DecrementRegister(Register16.DE)),
                0x1c => (4, IncrementRegister(Register8.E)),
                0x1d => (4, DecrementRegister(Register8.E)),
                0x1e => (8, LoadNextByteIntoRegister(Register8.E)),
                0x1f => (4, RotateRightThroughCarry(Register8.A, neverZero: true)),
                0x20 => (8, JumpRelativeConditional(Flags.Zero, true)),
                0x21 => (12, Load16IntoRegister(Register16.HL, ReadImmediate16)),
                0x22 => (8, LoadRegisterIntoMemory(() => Registers.HL++, () => Registers.A)),
                0x23 => (8, IncrementRegister(Register16.HL)),
                0x24 => (4, IncrementRegister(Register8.H)),
                0x25 => (4, DecrementRegister(Register8.H)),
                0x26 => (8, LoadNextByteIntoRegister(Register8.H)),
                0x27 => (4, DecimalAdjustAccumulator),
                0x28 => (8, JumpRelativeConditional(Flags.Zero, false)),
                0x29 => (8, AddHL(() => Registers.HL)),
                0x2a => (8, LoadValueIntoRegister(Register8.A, () => _bus.Read(Registers.HL++))),
                0x2b => (8, DecrementRegister(Register16.HL)),
                0x2c => (4, IncrementRegister(Register8.L)),
                0x2d => (4, DecrementRegister(Register8.L)),
                0x2e => (8, LoadNextByteIntoRegister(Register8.L)),
                0x2f => (4, ComplementAccumulator),
                0x30 => (8, JumpRelativeConditional(Flags.Carry, true)),
                0x31 => (12, Load16IntoRegister(Register16.SP, ReadImmediate16)),
                0x32 => (8, LoadRegisterIntoMemory(() => Registers.HL--, () => Registers.A)),
                0x33 => (8, IncrementRegister(Register16.SP)),
                0x34 => (12, IncrementMemory),
                0x35 => (12, DecrementMemory),
                0x36 => (12, LoadRegisterIntoMemory(() => Registers.HL, () => _bus.Read(Registers.PC++))),
                0x37 => (4, SetCarryFlag),
                0x38 => (8, JumpRelativeConditional(Flags.Carry, false)),
                0x39 => (8, AddHL(() => Registers.SP)),
                0x3a => (8, LoadValueIntoRegister(Register8.A, () => _bus.Read(Registers.HL--))),
                0x3b => (8, DecrementRegister(Register16.SP)),
                0x3c => (4, IncrementRegister(Register8.A)),
                0x3d => (4, DecrementRegister(Register8.A)),
                0x3e => (8, LoadValueIntoRegister(Register8.A, () => _bus.Read(Registers.PC++))),
                0x3f => (4, ComplementCarryFlag),
                0x40 => (4, LoadValueIntoRegister(Register8.B, () => Registers.B)),
                0x41 => (4, LoadValueIntoRegister(Register8.B, () => Registers.C)),
                0x42 => (4, LoadValueIntoRegister(Register8.B, () => Registers.D)),
                0x43 => (4, LoadValueIntoRegister(Register8.B, () => Registers.E)),
                0x44 => (4, LoadValueIntoRegister(Register8.B, () => Registers.H)),
                0x45 => (4, LoadValueIntoRegister(Register8.B, () => Registers.L)),
                0x46 => (8, LoadValueIntoRegister(Register8.B, () => _bus.Read(Registers.HL))),
                0x47 => (4, LoadValueIntoRegister(Register8.B, () => Registers.A)),
                0x48 => (4, LoadValueIntoRegister(Register8.C, () => Registers.B)),
                0x49 => (4, LoadValueIntoRegister(Register8.C, () => Registers.C)),
                0x4a => (4, LoadValueIntoRegister(Register8.C, () => Registers.D)),
                0x4b => (4, LoadValueIntoRegister(Register8.C, () => Registers.E)),
                0x4c => (4, LoadValueIntoRegister(Register8.C, () => Registers.H)),
                0x4d => (4, LoadValueIntoRegister(Register8.C, () => Registers.L)),
                0x4e => (8, LoadValueIntoRegister(Register8.C, () => _bus.Read(Registers.HL))),
                0x4f => (4, LoadValueIntoRegister(Register8.C, () => Registers.A)),
                0x50 => (4, LoadValueIntoRegister(Register8.D, () => Registers.B)),
                0x51 => (4, LoadValueIntoRegister(Register8.D, () => Registers.C)),
                0x52 => (4, LoadValueIntoRegister(Register8.D, () => Registers.D)),
                0x53 => (4, LoadValueIntoRegister(Register8.D, () => Registers.E)),
                0x54 => (4, LoadValueIntoRegister(Register8.D, () => Registers.H)),
                0x55 => (4, LoadValueIntoRegister(Register8.D, () => Registers.L)),
                0x56 => (8, LoadValueIntoRegister(Register8.D, () => _bus.Read(Registers.HL))),
                0x57 => (4, LoadValueIntoRegister(Register8.D, () => Registers.A)),
                0x58 => (4, LoadValueIntoRegister(Register8.E, () => Registers.B)),
                0x59 => (4, LoadValueIntoRegister(Register8.E, () => Registers.C)),
                0x5a => (4, LoadValueIntoRegister(Register8.E, () => Registers.D)),
                0x5b => (4, LoadValueIntoRegister(Register8.E, () => Registers.E)),
                0x5c => (4, LoadValueIntoRegister(Register8.E, () => Registers.H)),
                0x5d => (4, LoadValueIntoRegister(Register8.E, () => Registers.L)),
                0x5e => (8, LoadValueIntoRegister(Register8.E, () => _bus.Read(Registers.HL))),
                0x5f => (4, LoadValueIntoRegister(Register8.E, () => Registers.A)),
                0x60 => (4, LoadValueIntoRegister(Register8.H, () => Registers.B)),
                0x61 => (4, LoadValueIntoRegister(Register8.H, () => Registers.C)),
                0x62 => (4, LoadValueIntoRegister(Register8.H, () => Registers.D)),
                0x63 => (4, LoadValueIntoRegister(Register8.H, () => Registers.E)),
                0x64 => (4, LoadValueIntoRegister(Register8.H, () => Registers.H)),
                0x65 => (4, LoadValueIntoRegister(Register8.H, () => Registers.L)),
                0x66 => (8, LoadValueIntoRegister(Register8.H, () => _bus.Read(Registers.HL))),
                0x67 => (4, LoadValueIntoRegister(Register8.H, () => Registers.A)),
                0x68 => (4, LoadValueIntoRegister(Register8.L, () => Registers.B)),
                0x69 => (4, LoadValueIntoRegister(Register8.L, () => Registers.C)),
                0x6a => (4, LoadValueIntoRegister(Register8.L, () => Registers.D)),
                0x6b => (4, LoadValueIntoRegister(Register8.L, () => Registers.E)),
                0x6c => (4, LoadValueIntoRegister(Register8.L, () => Registers.H)),
                0x6d => (4, LoadValueIntoRegister(Register8.L, () => Registers.L)),
                0x6e => (8, LoadValueIntoRegister(Register8.L, () => _bus.Read(Registers.HL))),
                0x6f => (4, LoadValueIntoRegister(Register8.L, () => Registers.A)),
                0x70 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.B)),
                0x71 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.C)),
                0x72 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.D)),
                0x73 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.E)),
                0x74 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.H)),
                0x75 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.L)),
                0x76 => (4, Halt),
                0x77 => (8, LoadRegisterIntoMemory(() => Registers.HL, () => Registers.A)),
                0x78 => (4, LoadValueIntoRegister(Register8.A, () => Registers.B)),
                0x79 => (4, LoadValueIntoRegister(Register8.A, () => Registers.C)),
                0x7a => (4, LoadValueIntoRegister(Register8.A, () => Registers.D)),
                0x7b => (4, LoadValueIntoRegister(Register8.A, () => Registers.E)),
                0x7c => (4, LoadValueIntoRegister(Register8.A, () => Registers.H)),
                0x7d => (4, LoadValueIntoRegister(Register8.A, () => Registers.L)),
                0x7e => (8, LoadValueIntoRegister(Register8.A, () => _bus.Read(Registers.HL))),
                0x7f => (4, LoadValueIntoRegister(Register8.A, () => Registers.A)),
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
                0xc1 => (12, Pop(Register16.BC)),
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
                0xd1 => (12, Pop(Register16.DE)),
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
                0xe1 => (12, Pop(Register16.HL)),
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
                0xf1 => (12, Pop(Register16.AF)),
                0xf2 => (8, LoadIoIntoAccumulator(() => Registers.C)),
                0xf3 => (4, DisableInterrupts),
                0xf5 => (16, Push(() => Registers.AF)),
                0xf6 => (8, Or(() => _bus.Read(Registers.PC++))),
                0xf7 => (32, ResetToAddress(0x30)),
                0xf8 => (12, LoadSPPlusImmediateIntoHL),
                0xf9 => (8, Load16IntoRegister(Register16.SP, () => Registers.HL)),
                0xfa => (16, LoadImmediatePointerIntoAccumulator),
                0xfb => (4, EnableInterrupts),
                0xfe => (8, Compare(() => _bus.Read(Registers.PC++))),
                0xff => (32, ResetToAddress(0x38)),
                _ => (4, Nop)
            };

        private (int ClockCycles, Action Method) ParseExtendedInstruction() =>
            _bus.Read(Registers.PC++) switch
            {
                0x00 => (8, RotateLeft(Register8.B)),
                0x01 => (8, RotateLeft(Register8.C)),
                0x02 => (8, RotateLeft(Register8.D)),
                0x03 => (8, RotateLeft(Register8.E)),
                0x04 => (8, RotateLeft(Register8.H)),
                0x05 => (8, RotateLeft(Register8.L)),
                0x06 => (16, RotateMemoryLeft),
                0x07 => (8, RotateLeft(Register8.A)),
                0x08 => (8, RotateRight(Register8.B)),
                0x09 => (8, RotateRight(Register8.C)),
                0x0a => (8, RotateRight(Register8.D)),
                0x0b => (8, RotateRight(Register8.E)),
                0x0c => (8, RotateRight(Register8.H)),
                0x0d => (8, RotateRight(Register8.L)),
                0x0e => (16, RotateMemoryRight),
                0x0f => (8, RotateRight(Register8.A)),
                0x10 => (8, RotateLeftThroughCarry(Register8.B)),
                0x11 => (8, RotateLeftThroughCarry(Register8.C)),
                0x12 => (8, RotateLeftThroughCarry(Register8.D)),
                0x13 => (8, RotateLeftThroughCarry(Register8.E)),
                0x14 => (8, RotateLeftThroughCarry(Register8.H)),
                0x15 => (8, RotateLeftThroughCarry(Register8.L)),
                0x16 => (16, RotateMemoryLeftThroughCarry),
                0x17 => (8, RotateLeftThroughCarry(Register8.A)),
                0x18 => (8, RotateRightThroughCarry(Register8.B)),
                0x19 => (8, RotateRightThroughCarry(Register8.C)),
                0x1a => (8, RotateRightThroughCarry(Register8.D)),
                0x1b => (8, RotateRightThroughCarry(Register8.E)),
                0x1c => (8, RotateRightThroughCarry(Register8.H)),
                0x1d => (8, RotateRightThroughCarry(Register8.L)),
                0x1e => (16, RotateMemoryRightThroughCarry),
                0x1f => (8, RotateRightThroughCarry(Register8.A)),
                0x20 => (8, ShiftLeft(Register8.B)),
                0x21 => (8, ShiftLeft(Register8.C)),
                0x22 => (8, ShiftLeft(Register8.D)),
                0x23 => (8, ShiftLeft(Register8.E)),
                0x24 => (8, ShiftLeft(Register8.H)),
                0x25 => (8, ShiftLeft(Register8.L)),
                0x26 => (16, ShiftMemoryLeft),
                0x27 => (8, ShiftLeft(Register8.A)),
                0x28 => (8, ShiftRight(Register8.B)),
                0x29 => (8, ShiftRight(Register8.C)),
                0x2a => (8, ShiftRight(Register8.D)),
                0x2b => (8, ShiftRight(Register8.E)),
                0x2c => (8, ShiftRight(Register8.H)),
                0x2d => (8, ShiftRight(Register8.L)),
                0x2e => (16, ShiftMemoryRight),
                0x2f => (8, ShiftRight(Register8.A)),
                0x30 => (8, SwapRegisterNibbles(Register8.B)),
                0x31 => (8, SwapRegisterNibbles(Register8.C)),
                0x32 => (8, SwapRegisterNibbles(Register8.D)),
                0x33 => (8, SwapRegisterNibbles(Register8.E)),
                0x34 => (8, SwapRegisterNibbles(Register8.H)),
                0x35 => (8, SwapRegisterNibbles(Register8.L)),
                0x36 => (16, SwapMemoryNibbles),
                0x37 => (8, SwapRegisterNibbles(Register8.A)),
                0x38 => (8, ShiftRightZero(Register8.B)),
                0x39 => (8, ShiftRightZero(Register8.C)),
                0x3a => (8, ShiftRightZero(Register8.D)),
                0x3b => (8, ShiftRightZero(Register8.E)),
                0x3c => (8, ShiftRightZero(Register8.H)),
                0x3d => (8, ShiftRightZero(Register8.L)),
                0x3e => (16, ShiftMemoryRightZero),
                0x3f => (8, ShiftRightZero(Register8.A)),
                0x40 => (8, CheckBit(0, () => Registers.B)),
                0x41 => (8, CheckBit(0, () => Registers.C)),
                0x42 => (8, CheckBit(0, () => Registers.D)),
                0x43 => (8, CheckBit(0, () => Registers.E)),
                0x44 => (8, CheckBit(0, () => Registers.H)),
                0x45 => (8, CheckBit(0, () => Registers.L)),
                0x46 => (8, CheckBit(0, () => _bus.Read(Registers.HL))),
                0x47 => (8, CheckBit(0, () => Registers.A)),
                0x48 => (8, CheckBit(1, () => Registers.B)),
                0x49 => (8, CheckBit(1, () => Registers.C)),
                0x4a => (8, CheckBit(1, () => Registers.D)),
                0x4b => (8, CheckBit(1, () => Registers.E)),
                0x4c => (8, CheckBit(1, () => Registers.H)),
                0x4d => (8, CheckBit(1, () => Registers.L)),
                0x4e => (8, CheckBit(1, () => _bus.Read(Registers.HL))),
                0x4f => (8, CheckBit(1, () => Registers.A)),
                0x50 => (8, CheckBit(2, () => Registers.B)),
                0x51 => (8, CheckBit(2, () => Registers.C)),
                0x52 => (8, CheckBit(2, () => Registers.D)),
                0x53 => (8, CheckBit(2, () => Registers.E)),
                0x54 => (8, CheckBit(2, () => Registers.H)),
                0x55 => (8, CheckBit(2, () => Registers.L)),
                0x56 => (8, CheckBit(2, () => _bus.Read(Registers.HL))),
                0x57 => (8, CheckBit(2, () => Registers.A)),
                0x58 => (8, CheckBit(3, () => Registers.B)),
                0x59 => (8, CheckBit(3, () => Registers.C)),
                0x5a => (8, CheckBit(3, () => Registers.D)),
                0x5b => (8, CheckBit(3, () => Registers.E)),
                0x5c => (8, CheckBit(3, () => Registers.H)),
                0x5d => (8, CheckBit(3, () => Registers.L)),
                0x5e => (8, CheckBit(3, () => _bus.Read(Registers.HL))),
                0x5f => (8, CheckBit(3, () => Registers.A)),
                0x60 => (8, CheckBit(4, () => Registers.B)),
                0x61 => (8, CheckBit(4, () => Registers.C)),
                0x62 => (8, CheckBit(4, () => Registers.D)),
                0x63 => (8, CheckBit(4, () => Registers.E)),
                0x64 => (8, CheckBit(4, () => Registers.H)),
                0x65 => (8, CheckBit(4, () => Registers.L)),
                0x66 => (8, CheckBit(4, () => _bus.Read(Registers.HL))),
                0x67 => (8, CheckBit(4, () => Registers.A)),
                0x68 => (8, CheckBit(5, () => Registers.B)),
                0x69 => (8, CheckBit(5, () => Registers.C)),
                0x6a => (8, CheckBit(5, () => Registers.D)),
                0x6b => (8, CheckBit(5, () => Registers.E)),
                0x6c => (8, CheckBit(5, () => Registers.H)),
                0x6d => (8, CheckBit(5, () => Registers.L)),
                0x6e => (8, CheckBit(5, () => _bus.Read(Registers.HL))),
                0x6f => (8, CheckBit(5, () => Registers.A)),
                0x70 => (8, CheckBit(6, () => Registers.B)),
                0x71 => (8, CheckBit(6, () => Registers.C)),
                0x72 => (8, CheckBit(6, () => Registers.D)),
                0x73 => (8, CheckBit(6, () => Registers.E)),
                0x74 => (8, CheckBit(6, () => Registers.H)),
                0x75 => (8, CheckBit(6, () => Registers.L)),
                0x76 => (8, CheckBit(6, () => _bus.Read(Registers.HL))),
                0x77 => (8, CheckBit(6, () => Registers.A)),
                0x78 => (8, CheckBit(7, () => Registers.B)),
                0x79 => (8, CheckBit(7, () => Registers.C)),
                0x7a => (8, CheckBit(7, () => Registers.D)),
                0x7b => (8, CheckBit(7, () => Registers.E)),
                0x7c => (8, CheckBit(7, () => Registers.H)),
                0x7d => (8, CheckBit(7, () => Registers.L)),
                0x7e => (8, CheckBit(7, () => _bus.Read(Registers.HL))),
                0x7f => (8, CheckBit(7, () => Registers.A)),
                0x80 => (8, UnsetBit(0, Register8.B)),
                0x81 => (8, UnsetBit(0, Register8.C)),
                0x82 => (8, UnsetBit(0, Register8.D)),
                0x83 => (8, UnsetBit(0, Register8.E)),
                0x84 => (8, UnsetBit(0, Register8.H)),
                0x85 => (8, UnsetBit(0, Register8.L)),
                0x86 => (16, UnsetMemoryBit(0)),
                0x87 => (8, UnsetBit(0, Register8.A)),
                0x88 => (8, UnsetBit(1, Register8.B)),
                0x89 => (8, UnsetBit(1, Register8.C)),
                0x8a => (8, UnsetBit(1, Register8.D)),
                0x8b => (8, UnsetBit(1, Register8.E)),
                0x8c => (8, UnsetBit(1, Register8.H)),
                0x8d => (8, UnsetBit(1, Register8.L)),
                0x8e => (16, UnsetMemoryBit(1)),
                0x8f => (8, UnsetBit(1, Register8.A)),
                0x90 => (8, UnsetBit(2, Register8.B)),
                0x91 => (8, UnsetBit(2, Register8.C)),
                0x92 => (8, UnsetBit(2, Register8.D)),
                0x93 => (8, UnsetBit(2, Register8.E)),
                0x94 => (8, UnsetBit(2, Register8.H)),
                0x95 => (8, UnsetBit(2, Register8.L)),
                0x96 => (16, UnsetMemoryBit(2)),
                0x97 => (8, UnsetBit(2, Register8.A)),
                0x98 => (8, UnsetBit(3, Register8.B)),
                0x99 => (8, UnsetBit(3, Register8.C)),
                0x9a => (8, UnsetBit(3, Register8.D)),
                0x9b => (8, UnsetBit(3, Register8.E)),
                0x9c => (8, UnsetBit(3, Register8.H)),
                0x9d => (8, UnsetBit(3, Register8.L)),
                0x9e => (16, UnsetMemoryBit(3)),
                0x9f => (8, UnsetBit(3, Register8.A)),
                0xa0 => (8, UnsetBit(4, Register8.B)),
                0xa1 => (8, UnsetBit(4, Register8.C)),
                0xa2 => (8, UnsetBit(4, Register8.D)),
                0xa3 => (8, UnsetBit(4, Register8.E)),
                0xa4 => (8, UnsetBit(4, Register8.H)),
                0xa5 => (8, UnsetBit(4, Register8.L)),
                0xa6 => (16, UnsetMemoryBit(4)),
                0xa7 => (8, UnsetBit(4, Register8.A)),
                0xa8 => (8, UnsetBit(5, Register8.B)),
                0xa9 => (8, UnsetBit(5, Register8.C)),
                0xaa => (8, UnsetBit(5, Register8.D)),
                0xab => (8, UnsetBit(5, Register8.E)),
                0xac => (8, UnsetBit(5, Register8.H)),
                0xad => (8, UnsetBit(5, Register8.L)),
                0xae => (16, UnsetMemoryBit(5)),
                0xaf => (8, UnsetBit(5, Register8.A)),
                0xb0 => (8, UnsetBit(6, Register8.B)),
                0xb1 => (8, UnsetBit(6, Register8.C)),
                0xb2 => (8, UnsetBit(6, Register8.D)),
                0xb3 => (8, UnsetBit(6, Register8.E)),
                0xb4 => (8, UnsetBit(6, Register8.H)),
                0xb5 => (8, UnsetBit(6, Register8.L)),
                0xb6 => (16, UnsetMemoryBit(6)),
                0xb7 => (8, UnsetBit(6, Register8.A)),
                0xb8 => (8, UnsetBit(7, Register8.B)),
                0xb9 => (8, UnsetBit(7, Register8.C)),
                0xba => (8, UnsetBit(7, Register8.D)),
                0xbb => (8, UnsetBit(7, Register8.E)),
                0xbc => (8, UnsetBit(7, Register8.H)),
                0xbd => (8, UnsetBit(7, Register8.L)),
                0xbe => (16, UnsetMemoryBit(7)),
                0xbf => (8, UnsetBit(7, Register8.A)),
                0xc0 => (8, SetBit(0, Register8.B)),
                0xc1 => (8, SetBit(0, Register8.C)),
                0xc2 => (8, SetBit(0, Register8.D)),
                0xc3 => (8, SetBit(0, Register8.E)),
                0xc4 => (8, SetBit(0, Register8.H)),
                0xc5 => (8, SetBit(0, Register8.L)),
                0xc6 => (16, SetMemoryBit(0)),
                0xc7 => (8, SetBit(0, Register8.A)),
                0xc8 => (8, SetBit(1, Register8.B)),
                0xc9 => (8, SetBit(1, Register8.C)),
                0xca => (8, SetBit(1, Register8.D)),
                0xcb => (8, SetBit(1, Register8.E)),
                0xcc => (8, SetBit(1, Register8.H)),
                0xcd => (8, SetBit(1, Register8.L)),
                0xce => (16, SetMemoryBit(1)),
                0xcf => (8, SetBit(1, Register8.A)),
                0xd0 => (8, SetBit(2, Register8.B)),
                0xd1 => (8, SetBit(2, Register8.C)),
                0xd2 => (8, SetBit(2, Register8.D)),
                0xd3 => (8, SetBit(2, Register8.E)),
                0xd4 => (8, SetBit(2, Register8.H)),
                0xd5 => (8, SetBit(2, Register8.L)),
                0xd6 => (16, SetMemoryBit(2)),
                0xd7 => (8, SetBit(2, Register8.A)),
                0xd8 => (8, SetBit(3, Register8.B)),
                0xd9 => (8, SetBit(3, Register8.C)),
                0xda => (8, SetBit(3, Register8.D)),
                0xdb => (8, SetBit(3, Register8.E)),
                0xdc => (8, SetBit(3, Register8.H)),
                0xdd => (8, SetBit(3, Register8.L)),
                0xde => (16, SetMemoryBit(3)),
                0xdf => (8, SetBit(3, Register8.A)),
                0xe0 => (8, SetBit(4, Register8.B)),
                0xe1 => (8, SetBit(4, Register8.C)),
                0xe2 => (8, SetBit(4, Register8.D)),
                0xe3 => (8, SetBit(4, Register8.E)),
                0xe4 => (8, SetBit(4, Register8.H)),
                0xe5 => (8, SetBit(4, Register8.L)),
                0xe6 => (16, SetMemoryBit(4)),
                0xe7 => (8, SetBit(4, Register8.A)),
                0xe8 => (8, SetBit(5, Register8.B)),
                0xe9 => (8, SetBit(5, Register8.C)),
                0xea => (8, SetBit(5, Register8.D)),
                0xeb => (8, SetBit(5, Register8.E)),
                0xec => (8, SetBit(5, Register8.H)),
                0xed => (8, SetBit(5, Register8.L)),
                0xee => (16, SetMemoryBit(5)),
                0xef => (8, SetBit(5, Register8.A)),
                0xf0 => (8, SetBit(6, Register8.B)),
                0xf1 => (8, SetBit(6, Register8.C)),
                0xf2 => (8, SetBit(6, Register8.D)),
                0xf3 => (8, SetBit(6, Register8.E)),
                0xf4 => (8, SetBit(6, Register8.H)),
                0xf5 => (8, SetBit(6, Register8.L)),
                0xf6 => (16, SetMemoryBit(6)),
                0xf7 => (8, SetBit(6, Register8.A)),
                0xf8 => (8, SetBit(7, Register8.B)),
                0xf9 => (8, SetBit(7, Register8.C)),
                0xfa => (8, SetBit(7, Register8.D)),
                0xfb => (8, SetBit(7, Register8.E)),
                0xfc => (8, SetBit(7, Register8.H)),
                0xfd => (8, SetBit(7, Register8.L)),
                0xfe => (16, SetMemoryBit(7)),
                0xff => (8, SetBit(7, Register8.A)),
                _ => (0, Nop)
            };

        private enum Register8
        {
            A,
            B,
            C,
            D,
            E,
            F,
            H,
            L
        }

        private enum Register16
        {
            AF,
            BC,
            DE,
            HL,
            PC,
            SP
        }

        private Action<byte> GetSetterForRegister(Register8 register) =>
            register switch
            {
                Register8.A => v => Registers.A = v,
                Register8.B => v => Registers.B = v,
                Register8.C => v => Registers.C = v,
                Register8.D => v => Registers.D = v,
                Register8.E => v => Registers.E = v,
                Register8.F => v => Registers.F = v,
                Register8.H => v => Registers.H = v,
                Register8.L => v => Registers.L = v,
            };

        private Func<byte> GetGetterForRegister(Register8 register) =>
            register switch
            {
                Register8.A => () => Registers.A,
                Register8.B => () => Registers.B,
                Register8.C => () => Registers.C,
                Register8.D => () => Registers.D,
                Register8.E => () => Registers.E,
                Register8.F => () => Registers.F,
                Register8.H => () => Registers.H,
                Register8.L => () => Registers.L,
            };

        private Action<ushort> GetSetterForRegister(Register16 register) =>
            register switch
            {
                Register16.AF => v => Registers.AF = v,
                Register16.BC => v => Registers.BC = v,
                Register16.DE => v => Registers.DE = v,
                Register16.HL => v => Registers.HL = v,
                Register16.SP => v => Registers.SP = v,
                Register16.PC => v => Registers.PC = v,
            };

        private Func<ushort> GetGetterForRegister(Register16 register) =>
            register switch
            {
                Register16.AF => () => Registers.AF,
                Register16.BC => () => Registers.BC,
                Register16.DE => () => Registers.DE,
                Register16.HL => () => Registers.HL,
                Register16.SP => () => Registers.SP,
                Register16.PC => () => Registers.PC,
            };

        private Action LoadNextByteIntoRegister(Register8 register) => () =>
            GetSetterForRegister(register)(_bus.Read(Registers.PC++));

        private Action LoadValueIntoRegister(Register8 register, Func<byte> value) => () =>
            GetSetterForRegister(register)(value());

        private Action LoadRegisterIntoMemory(Func<DoubleRegister> pointer, Func<byte> register) =>
            () => _bus.Write(pointer(), register());

        private void LoadAccumulatorIntoImmediatePointer() =>
            _bus.Write(ReadImmediate16(), Registers.A);

        private void LoadImmediatePointerIntoAccumulator() =>
            Registers.A = _bus.Read(ReadImmediate16());

        private Action LoadIoIntoAccumulator(Func<byte> register) =>
            () => Registers.A = _bus.Read((ushort) (0xff00 | register()));

        private Action LoadAccumulatorIntoIo(Func<byte> register) =>
            () => _bus.Write((ushort) (0xff00 | register()), Registers.A);

        private Action Load16IntoRegister(Register16 register, Func<ushort> value) => () =>
            GetSetterForRegister(register)(value());

        private void LoadStackPointerIntoMemory()
        {
            var address = (ushort) (_bus.Read(Registers.PC++) | (_bus.Read(Registers.PC++) << 8));
            _bus.Write(address, Registers.SP.Lower);
            _bus.Write((ushort) (address + 1), Registers.SP.Upper);
        }

        private Action Push(Func<DoubleRegister> register) => () =>
        {
            var registerValue = register();
            _bus.Write(--Registers.SP, registerValue.Upper);
            _bus.Write(--Registers.SP, registerValue.Lower);
        };

        private Action Pop(Register16 register) =>
            Load16IntoRegister(register, () => (ushort) (_bus.Read(Registers.SP++) | (_bus.Read(Registers.SP++) << 8)));

        private Action Add(Func<byte> register) => () =>
        {
            var left = Registers.A;
            var right = register();

            var result = left + right;

            UpdateFlag(Flags.Carry, result > 0xff);
            UnsetFlag(Flags.Subtract);
            result &= 0xff;
            UpdateFlag(Flags.Zero, result == 0);
            UpdateFlag(Flags.HalfCarry, (left & 0x0f) + (right & 0x0f) >= 0x10);

            Registers.A = (byte) result;
        };

        private Action AddWithCarry(Func<byte> register) => () =>
        {
            var left = Registers.A;
            var right = register();
            var carry = (GetFlag(Flags.Carry) ? 1 : 0);

            var result = left + right + carry;

            UpdateFlag(Flags.Carry, result > 0xff);
            UnsetFlag(Flags.Subtract);
            result &= 0xff;
            UpdateFlag(Flags.Zero, result == 0);
            UpdateFlag(Flags.HalfCarry, (left & 0x0f) + (right & 0x0f) + carry >= 0x10);

            Registers.A = (byte)result;
        };

        private Action Subtract(Func<byte> register) => () =>
        {
            var value = register();
            var result = Registers.A - value;

            UpdateFlag(Flags.Carry, result < 0);
            SetFlag(Flags.Subtract);
            result &= 0xff;
            UpdateFlag(Flags.Zero, result == 0);
            UpdateFlag(Flags.HalfCarry, (Registers.A & 0x0f) - (value & 0x0f) < 0);

            Registers.A = (byte) result;
        };

        private Action SubtractWithCarry(Func<byte> register) => () =>
        {
            var left = Registers.A;
            var right = register();
            var carry = (GetFlag(Flags.Carry) ? 1 : 0);

            var result = left - right - carry;

            UpdateFlag(Flags.Carry, result < 0);
            SetFlag(Flags.Subtract);
            result &= 0xff;
            UpdateFlag(Flags.Zero, result == 0);
            UpdateFlag(Flags.HalfCarry, (left & 0x0f) - (right & 0x0f) - carry < 0);

            Registers.A = (byte)result;
        };

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

        private Action IncrementRegister(Register8 register) => () =>
        {
            var result = (byte) (GetGetterForRegister(register)() + 1);
            GetSetterForRegister(register)(result);

            UpdateFlag(Flags.Zero, result == 0);
            UnsetFlag(Flags.Subtract);
            UpdateFlag(Flags.HalfCarry, (result & 0x0f) == 0);
        };

        private void IncrementMemory()
        {
            var result = (byte) (_bus.Read(Registers.HL) + 1);
            _bus.Write(Registers.HL, result);

            UpdateFlag(Flags.Zero, result == 0);
            UnsetFlag(Flags.Subtract);
            UpdateFlag(Flags.HalfCarry, (result & 0x0f) == 0);
        }

        private Action DecrementRegister(Register8 register) => () =>
        {
            var result = (byte) (GetGetterForRegister(register)() - 1);
            GetSetterForRegister(register)(result);

            UpdateFlag(Flags.Zero, result == 0);
            SetFlag(Flags.Subtract);
            UpdateFlag(Flags.HalfCarry, (result & 0x0f) == 0x0f);
        };

        private void DecrementMemory()
        {
            var result = (byte) (_bus.Read(Registers.HL) - 1);
            _bus.Write(Registers.HL, result);

            UpdateFlag(Flags.Zero, result == 0);
            SetFlag(Flags.Subtract);
            UpdateFlag(Flags.HalfCarry, (result & 0x0f) == 0x0f);
        }

        private Action AddHL(Func<DoubleRegister> register) => () =>
        {
            var left = Registers.HL;
            var right = register();

            Registers.HL += right;

            UnsetFlag(Flags.Subtract);
            UpdateFlag(Flags.HalfCarry, (((left & 0x0fff) + (right & 0x0fff)) & 0xf000) > 0);
            UpdateFlag(Flags.Carry, left + right > 0xffff);
        };

        private void AddSPImmediate8()
        {
            var left = Registers.SP;
            var right = (sbyte)_bus.Read(Registers.PC++);

            var result = Registers.SP + right;

            UnsetFlag(Flags.Zero);
            UnsetFlag(Flags.Subtract);
            if (right >= 0)
            {
                UpdateFlag(Flags.Carry, (left & 0xff) + right > 0xff);
                UpdateFlag(Flags.HalfCarry, (left & 0xf) + (right & 0xf) > 0xf);
            }
            else
            {
                UpdateFlag(Flags.Carry, (result & 0xff) <= (left & 0xff));
                UpdateFlag(Flags.HalfCarry, (result & 0xf) <= (left & 0xf));
            }

            Registers.SP = (ushort)result;
        }

        private Action IncrementRegister(Register16 register) => () =>
        {
            var result = GetGetterForRegister(register)() + 1;
            GetSetterForRegister(register)((ushort)result);
        };

        private Action DecrementRegister(Register16 register) => () =>
        {
            var result = (ushort) (GetGetterForRegister(register)() - 1);
            GetSetterForRegister(register)(result);
        };

        private Action SwapRegisterNibbles(Register8 register) => () =>
        {
            var value = GetGetterForRegister(register)();
            var result = (byte)(((value & 0x0f) << 4) + ((value & 0xf0) >> 4));

            UpdateFlag(Flags.Zero, result == 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            UnsetFlag(Flags.Carry);

            GetSetterForRegister(register)(result);
        };

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
            if(GetFlag(Flags.Subtract))
            {
                if (GetFlag(Flags.Carry)) Registers.A -= 0x60;
                if (GetFlag(Flags.HalfCarry)) Registers.A -= 0x06;
            }
            else
            {
                if(GetFlag(Flags.Carry) || Registers.A > 0x99)
                {
                    Registers.A += 0x60;
                    SetFlag(Flags.Carry);
                }

                if (GetFlag(Flags.HalfCarry) || (Registers.A & 0x0f) > 0x09)
                    Registers.A += 0x06;
            }

            UpdateFlag(Flags.Zero, Registers.A == 0);
            UnsetFlag(Flags.HalfCarry);
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

        private Action RotateLeft(Register8 register, bool neverZero = false) => () =>
        {
            var value = GetGetterForRegister(register)();
            UpdateFlag(Flags.Carry, (value & 0b10000000) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value <<= 1;
            value = (byte) (value | (GetFlag(Flags.Carry) ? 1 : 0));

            UpdateFlag(Flags.Zero, !neverZero && value == 0);

            GetSetterForRegister(register)(value);
        };

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

        private Action RotateLeftThroughCarry(Register8 register, bool neverZero = false) => () =>
        {
            var value = GetGetterForRegister(register)();
            var oldCarryValue = GetFlag(Flags.Carry);
            UpdateFlag(Flags.Carry, (value & 0b10000000) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value <<= 1;
            value = (byte)(value | (oldCarryValue ? 1 : 0));

            UpdateFlag(Flags.Zero, !neverZero && value == 0);

            GetSetterForRegister(register)(value);
        };

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

        private Action RotateRight(Register8 register, bool neverZero = false) => () =>
        {
            var value = GetGetterForRegister(register)();
            UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value >>= 1;
            value = (byte)(value | (GetFlag(Flags.Carry) ? 0b10000000 : 0));

            UpdateFlag(Flags.Zero, !neverZero && value == 0);

            GetSetterForRegister(register)(value);
        };

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

        private Action RotateRightThroughCarry(Register8 register, bool neverZero = false) => () =>
        {
            var value = GetGetterForRegister(register)();
            var oldCarryValue = GetFlag(Flags.Carry);
            UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value >>= 1;
            value = (byte) (value | (oldCarryValue ? 0b10000000 : 0));

            UpdateFlag(Flags.Zero, !neverZero && value == 0);

            GetSetterForRegister(register)(value);
        };

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

        private Action ShiftLeft(Register8 register) => () =>
        {
            var value = GetGetterForRegister(register)();
            UpdateFlag(Flags.Carry, (value & 0b10000000) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value <<= 1;

            UpdateFlag(Flags.Zero, value == 0);

            GetSetterForRegister(register)(value);
        };

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

        private Action ShiftRight(Register8 register) => () =>
        {
            var value = GetGetterForRegister(register)();
            UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value >>= 1;
            value = (byte)(value | ((value & 0b01000000) > 0 ? 0b10000000 : 0));

            UpdateFlag(Flags.Zero, value == 0);

            GetSetterForRegister(register)(value);
        };

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

        private Action ShiftRightZero(Register8 register) => () =>
        {
            var value = GetGetterForRegister(register)();
            UpdateFlag(Flags.Carry, (value & 0b00000001) > 0);
            UnsetFlag(Flags.Subtract);
            UnsetFlag(Flags.HalfCarry);
            value >>= 1;

            UpdateFlag(Flags.Zero, value == 0);

            GetSetterForRegister(register)(value);
        };

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

        private Action CheckBit(byte bit, Func<byte> value) =>
            () =>
            {
                UpdateFlag(Flags.Zero, (value() & (1 << bit)) == 0);
                UnsetFlag(Flags.Subtract);
                SetFlag(Flags.HalfCarry);
            };

        private Action SetBit(byte bit, Register8 register) => () =>
            GetSetterForRegister(register)((byte)(GetGetterForRegister(register)() | (1 << bit)));

        private Action SetMemoryBit(byte bit) => () =>
            _bus.Write(Registers.HL, (byte)(_bus.Read(Registers.HL) | (1 << bit)));

        private Action UnsetBit(byte bit, Register8 register) => () =>
            GetSetterForRegister(register)((byte)(GetGetterForRegister(register)() & ~(1 << bit)));

        private Action UnsetMemoryBit(byte bit) => () =>
            _bus.Write(Registers.HL, (byte)(_bus.Read(Registers.HL) & ~(1 << bit)));

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

        private void LoadSPPlusImmediateIntoHL()
        {
            var left = Registers.SP;
            var right = (sbyte)_bus.Read(Registers.PC++);

            var result = left + right;

            UnsetFlag(Flags.Zero);
            UnsetFlag(Flags.Subtract);

            if(right >= 0)
            {
                UpdateFlag(Flags.Carry, (left & 0xff) + right > 0xff);
                UpdateFlag(Flags.HalfCarry, (left & 0xf) + (right & 0xf) > 0xf);
            }
            else
            {
                UpdateFlag(Flags.Carry, (result & 0xff) <= (left & 0xff));
                UpdateFlag(Flags.HalfCarry, (result & 0xf) <= (left & 0xf));
            }

            Registers.HL = (ushort)(result & 0xffff);
        }

        private void Return() =>
            Pop(Register16.PC)();

        private Action ReturnConditional(Flags flag, bool invert) => () =>
        {
            if (GetFlag(flag) != invert)
                Pop(Register16.PC)();
        };

        private void ReturnWithInterruptsEnabled()
        {
            Pop(Register16.PC)();
            _interruptsEnabled = true;
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
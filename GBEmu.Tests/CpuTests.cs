namespace GBEmu.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Moq;
    using NUnit.Framework;

    [TestFixture]
    public class CpuTests : UnitTestBase<Cpu>
    {
        [Test]
        public void Cpu_OnConstruct_SetsCorrectStartPoint()
        {
            ((ushort) ClassUnderTest.Registers.PC).Should().Be(0x100);
        }

        [Test]
        [TestCase(Registers.B, 0x06)]
        [TestCase(Registers.C, 0x0e)]
        [TestCase(Registers.D, 0x16)]
        [TestCase(Registers.E, 0x1e)]
        [TestCase(Registers.H, 0x26)]
        [TestCase(Registers.L, 0x2e)]
        public void LoadMemoryToRegister_SetsExpectedValue(Registers register, byte opcode)
        {
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x42);

            ClassUnderTest.Next();

            GetRegister(register).Should().Be(0x42);
        }

        [Test]
        [TestCase(Registers.B, 0x06)]
        [TestCase(Registers.C, 0x0e)]
        [TestCase(Registers.D, 0x16)]
        [TestCase(Registers.E, 0x1e)]
        [TestCase(Registers.H, 0x26)]
        [TestCase(Registers.L, 0x2e)]
        public void LoadMemoryToRegister_DoesNotAlterOtherRegisters(Registers register, byte opcode)
        {
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x42);

            ClassUnderTest.Next();

            Enum.GetValues(typeof(Registers))
                .Cast<Registers>()
                .Except(new [] { register })
                .Select(GetRegister)
                .Should().AllBeEquivalentTo(0);
        }

        [Test]
        [TestCase(Registers.B, Registers.B, 0x40)]
        [TestCase(Registers.B, Registers.C, 0x41)]
        [TestCase(Registers.B, Registers.D, 0x42)]
        [TestCase(Registers.B, Registers.E, 0x43)]
        [TestCase(Registers.B, Registers.H, 0x44)]
        [TestCase(Registers.B, Registers.L, 0x45)]
        [TestCase(Registers.B, Registers.A, 0x47)]
        [TestCase(Registers.C, Registers.B, 0x48)]
        [TestCase(Registers.C, Registers.C, 0x49)]
        [TestCase(Registers.C, Registers.D, 0x4a)]
        [TestCase(Registers.C, Registers.E, 0x4b)]
        [TestCase(Registers.C, Registers.H, 0x4c)]
        [TestCase(Registers.C, Registers.L, 0x4d)]
        [TestCase(Registers.C, Registers.A, 0x4f)]
        [TestCase(Registers.D, Registers.B, 0x50)]
        [TestCase(Registers.D, Registers.C, 0x51)]
        [TestCase(Registers.D, Registers.D, 0x52)]
        [TestCase(Registers.D, Registers.E, 0x53)]
        [TestCase(Registers.D, Registers.H, 0x54)]
        [TestCase(Registers.D, Registers.L, 0x55)]
        [TestCase(Registers.D, Registers.A, 0x57)]
        [TestCase(Registers.E, Registers.B, 0x58)]
        [TestCase(Registers.E, Registers.C, 0x59)]
        [TestCase(Registers.E, Registers.D, 0x5a)]
        [TestCase(Registers.E, Registers.E, 0x5b)]
        [TestCase(Registers.E, Registers.H, 0x5c)]
        [TestCase(Registers.E, Registers.L, 0x5d)]
        [TestCase(Registers.E, Registers.A, 0x5f)]
        [TestCase(Registers.H, Registers.B, 0x60)]
        [TestCase(Registers.H, Registers.C, 0x61)]
        [TestCase(Registers.H, Registers.D, 0x62)]
        [TestCase(Registers.H, Registers.E, 0x63)]
        [TestCase(Registers.H, Registers.H, 0x64)]
        [TestCase(Registers.H, Registers.L, 0x65)]
        [TestCase(Registers.H, Registers.A, 0x67)]
        [TestCase(Registers.L, Registers.B, 0x68)]
        [TestCase(Registers.L, Registers.C, 0x69)]
        [TestCase(Registers.L, Registers.D, 0x6a)]
        [TestCase(Registers.L, Registers.E, 0x6b)]
        [TestCase(Registers.L, Registers.H, 0x6c)]
        [TestCase(Registers.L, Registers.L, 0x6d)]
        [TestCase(Registers.L, Registers.A, 0x6f)]
        [TestCase(Registers.A, Registers.B, 0x78)]
        [TestCase(Registers.A, Registers.C, 0x79)]
        [TestCase(Registers.A, Registers.D, 0x7a)]
        [TestCase(Registers.A, Registers.E, 0x7b)]
        [TestCase(Registers.A, Registers.H, 0x7c)]
        [TestCase(Registers.A, Registers.L, 0x7d)]
        [TestCase(Registers.A, Registers.A, 0x7f)]
        public void LoadRegisterToRegister_SetsExpectedValue(Registers to, Registers from, byte opcode)
        {
            SetRegister(from, 0x42);

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            GetRegister(to).Should().Be(0x42);
        }

        [Test]
        [TestCase(Registers.B, 0x46)]
        [TestCase(Registers.C, 0x4e)]
        [TestCase(Registers.D, 0x56)]
        [TestCase(Registers.E, 0x5e)]
        [TestCase(Registers.H, 0x66)]
        [TestCase(Registers.L, 0x6e)]
        [TestCase(Registers.A, 0x7e)]
        public void LoadMemoryPointerToRegister_SetsExpectedValue(Registers register, byte opcode)
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0x42);

            ClassUnderTest.Next();

            GetRegister(register).Should().Be(0x42);
        }

        [Test]
        [TestCase(Registers.B, 0x70)]
        [TestCase(Registers.C, 0x71)]
        [TestCase(Registers.D, 0x72)]
        [TestCase(Registers.E, 0x73)]
        [TestCase(Registers.H, 0x74)]
        [TestCase(Registers.L, 0x75)]
        public void LoadRegisterToMemoryPointer_SetsExpectedValue(Registers register, byte opcode)
        {
            ClassUnderTest.Registers.HL = 0x1234;
            SetRegister(register, 0x42);

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();
            
            GetMock<IBus>().Verify(m => m.Write(ClassUnderTest.Registers.HL, 0x42), Times.Once);
        }

        [Test]
        public void LoadImmediate8IntoMemory_SetsMemory()
        {
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x36);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x42);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0x1234, 0x42), Times.Once);
        }

        [Test]
        public void LoadImmediatePointerIntoAccumulator_SetsAccumulator()
        {
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xfa);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x34);
            GetMock<IBus>().Setup(m => m.Read(0x102)).Returns(0x12);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0x42);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x42);
        }

        [Test]
        public void LoadAccumulatorIntoImmediatePointer_SetsMemory()
        {
            ClassUnderTest.Registers.A = 0x42;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xea);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x34);
            GetMock<IBus>().Setup(m => m.Read(0x102)).Returns(0x12);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0x1234, 0x42), Times.Once);
        }

        [Test]
        public void LoadAccumulatorIntoBCPointer_SetsMemory()
        {
            ClassUnderTest.Registers.A = 0x42;
            ClassUnderTest.Registers.BC = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x02);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0x1234, 0x42), Times.Once);
        }

        [Test]
        public void LoadAccumulatorIntoDEPointer_SetsMemory()
        {
            ClassUnderTest.Registers.A = 0x42;
            ClassUnderTest.Registers.DE = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x12);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0x1234, 0x42), Times.Once);
        }

        [Test]
        public void LoadIoIntoAccumulator_SetsAccumulator()
        {
            ClassUnderTest.Registers.C = 0x12;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xf2);
            GetMock<IBus>().Setup(m => m.Read(0xff12)).Returns(0x42);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x42);
        }

        [Test]
        public void LoadAccumulatorIntoIo_SetsIoMemory()
        {
            ClassUnderTest.Registers.A = 0x42;
            ClassUnderTest.Registers.C = 0x12;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xe2);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0xff12, 0x42), Times.Once);
        }

        [Test]
        public void LoadImmediateIoIntoAccumulator_SetsAccumulator()
        {
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xf0);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x12);
            GetMock<IBus>().Setup(m => m.Read(0xff12)).Returns(0x42);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x42);
        }

        [Test]
        public void LoadAccumulatorIntoImmediateIo_SetsIoMemory()
        {
            ClassUnderTest.Registers.A = 0x42;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xe0);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x12);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0xff12, 0x42), Times.Once);
        }

        [Test]
        public void LoadMemoryIntoAccumulatorWithDecrement_SetsAccumulator()
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x3a);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0x42);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x42);
        }

        [Test]
        public void LoadMemoryIntoAccumulatorWithDecrement_DecrementsRegister()
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x3a);

            ClassUnderTest.Next();

            ((ushort)ClassUnderTest.Registers.HL).Should().Be(0x1233);
        }

        [Test]
        public void LoadAccumulatorIntoMemoryWithDecrement_SetsMemory()
        {
            ClassUnderTest.Registers.A = 0x42;
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x32);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0x1234, 0x42), Times.Once);
        }

        [Test]
        public void LoadAccumulatorIntoMemoryWithDecrement_DecrementsRegister()
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x32);

            ClassUnderTest.Next();

            ((ushort)ClassUnderTest.Registers.HL).Should().Be(0x1233);
        }

        [Test]
        public void LoadMemoryIntoAccumulatorWithIncrement_SetsAccumulator()
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x2a);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0x42);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x42);
        }

        [Test]
        public void LoadMemoryIntoAccumulatorWithIncrement_IncrementsRegister()
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x2a);

            ClassUnderTest.Next();

            ((ushort)ClassUnderTest.Registers.HL).Should().Be(0x1235);
        }

        [Test]
        public void LoadAccumulatorIntoMemoryWithIncrement_SetsMemory()
        {
            ClassUnderTest.Registers.A = 0x42;
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x22);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0x1234, 0x42), Times.Once);
        }

        [Test]
        public void LoadAccumulatorIntoMemoryWithIncrement_IncrementsRegister()
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x22);

            ClassUnderTest.Next();

            ((ushort)ClassUnderTest.Registers.HL).Should().Be(0x1235);
        }

        [Test]
        [TestCase(Register16s.BC, 0x01)]
        [TestCase(Register16s.DE, 0x11)]
        [TestCase(Register16s.HL, 0x21)]
        [TestCase(Register16s.SP, 0x31)]
        public void LoadImmediate16IntoRegister_SetsRegister(Register16s register, byte opcode)
        {
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x34);
            GetMock<IBus>().Setup(m => m.Read(0x102)).Returns(0x12);

            ClassUnderTest.Next();

            GetRegister(register).Should().Be(0x1234);
        }

        [Test]
        public void LoadHLIntoStackPointer_SetsRegister()
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xf9);

            ClassUnderTest.Next();

            ((ushort)ClassUnderTest.Registers.SP).Should().Be(0x1234);
        }

        [Test]
        [TestCase(0x10)]
        [TestCase(-0x10)]
        public void LoadOffsetStackPointerIntoHL_SetsRegister(sbyte offset)
        {
            ClassUnderTest.Registers.SP = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xf8);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns((byte)offset);

            ClassUnderTest.Next();

            ((ushort)ClassUnderTest.Registers.HL).Should().Be((ushort)(0x1234 + offset));
        }

        [Test]
        public void LoadStackPointerIntoMemory()
        {
            ClassUnderTest.Registers.SP = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x08);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x34);
            GetMock<IBus>().Setup(m => m.Read(0x102)).Returns(0x12);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0x1234, 0x34), Times.Once);
            GetMock<IBus>().Verify(m => m.Write(0x1235, 0x12), Times.Once);
        }

        [Test]
        [TestCase(Register16s.AF, 0xf5)]
        [TestCase(Register16s.BC, 0xc5)]
        [TestCase(Register16s.DE, 0xd5)]
        [TestCase(Register16s.HL, 0xe5)]
        public void Push_SetsMemory(Register16s register, byte opcode)
        {
            ClassUnderTest.Registers.SP = 0xff80;
            SetRegister(register, 0x1234);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0xff80, 0x34), Times.Once);
            GetMock<IBus>().Verify(m => m.Write(0xff7f, 0x12), Times.Once);
        }

        [Test]
        [TestCase(0xf5)]
        [TestCase(0xc5)]
        [TestCase(0xd5)]
        [TestCase(0xe5)]
        public void Push_DecrementsStackPointer(byte opcode)
        {
            ClassUnderTest.Registers.SP = 0xff80;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            ((ushort) ClassUnderTest.Registers.SP).Should().Be(0xff7e);
        }

        [Test]
        [TestCase(Register16s.AF, 0xf1)]
        [TestCase(Register16s.BC, 0xc1)]
        [TestCase(Register16s.DE, 0xd1)]
        [TestCase(Register16s.HL, 0xe1)]
        public void Pop_SetsRegister(Register16s register, byte opcode)
        {
            ClassUnderTest.Registers.SP = 0xff7e;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);
            GetMock<IBus>().Setup(m => m.Read(0xff7e)).Returns(0x34);
            GetMock<IBus>().Setup(m => m.Read(0xff7f)).Returns(0x12);

            ClassUnderTest.Next();

            GetRegister(register).Should().Be(0x1234);
        }

        [Test]
        [TestCase(0xf1)]
        [TestCase(0xc1)]
        [TestCase(0xd1)]
        [TestCase(0xe1)]
        public void Pop_IncrementsStackPointer(byte opcode)
        {
            ClassUnderTest.Registers.SP = 0xff7e;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            ((ushort)ClassUnderTest.Registers.SP).Should().Be(0xff80);
        }

        [Test]
        [TestCase(Registers.B, 0x80)]
        [TestCase(Registers.C, 0x81)]
        [TestCase(Registers.D, 0x82)]
        [TestCase(Registers.E, 0x83)]
        [TestCase(Registers.H, 0x84)]
        [TestCase(Registers.L, 0x85)]
        [TestCase(Registers.A, 0x87)]
        public void Add_SetsAccumulator(Registers register, byte opcode)
        {
            ClassUnderTest.Registers.A = 0x20;
            SetRegister(register, 0x20);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x40);
        }

        [Test]
        [TestCase(Registers.B, 0x80, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.B, 0x80, 0x00, 0x00, true, false, false, false)]
        [TestCase(Registers.B, 0x80, 0x0f, 0x01, false, false, true, false)]
        [TestCase(Registers.B, 0x80, 0xf0, 0x20, false, false, false, true)]
        [TestCase(Registers.B, 0x80, 0xf0, 0x10, true, false, false, true)]
        [TestCase(Registers.C, 0x81, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.C, 0x81, 0x00, 0x00, true, false, false, false)]
        [TestCase(Registers.C, 0x81, 0x0f, 0x01, false, false, true, false)]
        [TestCase(Registers.C, 0x81, 0xf0, 0x20, false, false, false, true)]
        [TestCase(Registers.C, 0x81, 0xf0, 0x10, true, false, false, true)]
        [TestCase(Registers.D, 0x82, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.D, 0x82, 0x00, 0x00, true, false, false, false)]
        [TestCase(Registers.D, 0x82, 0x0f, 0x01, false, false, true, false)]
        [TestCase(Registers.D, 0x82, 0xf0, 0x20, false, false, false, true)]
        [TestCase(Registers.D, 0x82, 0xf0, 0x10, true, false, false, true)]
        [TestCase(Registers.E, 0x83, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.E, 0x83, 0x00, 0x00, true, false, false, false)]
        [TestCase(Registers.E, 0x83, 0x0f, 0x01, false, false, true, false)]
        [TestCase(Registers.E, 0x83, 0xf0, 0x20, false, false, false, true)]
        [TestCase(Registers.E, 0x83, 0xf0, 0x10, true, false, false, true)]
        [TestCase(Registers.H, 0x84, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.H, 0x84, 0x00, 0x00, true, false, false, false)]
        [TestCase(Registers.H, 0x84, 0x0f, 0x01, false, false, true, false)]
        [TestCase(Registers.H, 0x84, 0xf0, 0x20, false, false, false, true)]
        [TestCase(Registers.H, 0x84, 0xf0, 0x10, true, false, false, true)]
        [TestCase(Registers.L, 0x85, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.L, 0x85, 0x00, 0x00, true, false, false, false)]
        [TestCase(Registers.L, 0x85, 0x0f, 0x01, false, false, true, false)]
        [TestCase(Registers.L, 0x85, 0xf0, 0x20, false, false, false, true)]
        [TestCase(Registers.L, 0x85, 0xf0, 0x10, true, false, false, true)]
        [TestCase(Registers.A, 0x87, 0x10, 0x10, false, false, false, false)]
        [TestCase(Registers.A, 0x87, 0x00, 0x00, true, false, false, false)]
        [TestCase(Registers.A, 0x87, 0x08, 0x08, false, false, true, false)]
        [TestCase(Registers.A, 0x87, 0x90, 0x90, false, false, false, true)]
        [TestCase(Registers.A, 0x87, 0x80, 0x80, true, false, false, true)]
        public void Add_SetsExpectedFlags(Registers register, byte opcode, byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            SetRegister(register, value2);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));
            
            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void AddFromMemory_SetsAccumulator()
        {
            ClassUnderTest.Registers.A = 0x30;
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x86);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0x10);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x40);
        }

        [Test]
        [TestCase(0x10, 0x00, false, false, false, false)]
        [TestCase(0x00, 0x00, true, false, false, false)]
        [TestCase(0x0f, 0x01, false, false, true, false)]
        [TestCase(0xf0, 0x20, false, false, false, true)]
        [TestCase(0xf0, 0x10, true, false, false, true)]
        public void AddFromMemory_SetsExpectedFlags(byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x86);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void AddImmediate_SetsAccumulator()
        {
            ClassUnderTest.Registers.A = 0x30;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xc6);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x10);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x40);
        }

        [Test]
        [TestCase(0x10, 0x00, false, false, false, false)]
        [TestCase(0x00, 0x00, true, false, false, false)]
        [TestCase(0x0f, 0x01, false, false, true, false)]
        [TestCase(0xf0, 0x20, false, false, false, true)]
        [TestCase(0xf0, 0x10, true, false, false, true)]
        public void AddImmediate_SetsExpectedFlags(byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xc6);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Registers.B, 0x88)]
        [TestCase(Registers.C, 0x89)]
        [TestCase(Registers.D, 0x8a)]
        [TestCase(Registers.E, 0x8b)]
        [TestCase(Registers.H, 0x8c)]
        [TestCase(Registers.L, 0x8d)]
        [TestCase(Registers.A, 0x8f)]
        public void AddWithCarry_SetsAccumulator(Registers register, byte opcode)
        {
            ClassUnderTest.Registers.A = 0x20;
            ClassUnderTest.Registers.F = 0b00010000;
            SetRegister(register, 0x20);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x41);
        }

        [Test]
        [TestCase(Registers.B, 0x88, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.B, 0x88, 0xff, 0x00, true, false, true, true)]
        [TestCase(Registers.B, 0x88, 0x0e, 0x01, false, false, true, false)]
        [TestCase(Registers.C, 0x89, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.C, 0x89, 0xff, 0x00, true, false, true, true)]
        [TestCase(Registers.C, 0x89, 0x0e, 0x01, false, false, true, false)]
        [TestCase(Registers.D, 0x8a, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.D, 0x8a, 0xff, 0x00, true, false, true, true)]
        [TestCase(Registers.D, 0x8a, 0x0e, 0x01, false, false, true, false)]
        [TestCase(Registers.E, 0x8b, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.E, 0x8b, 0xff, 0x00, true, false, true, true)]
        [TestCase(Registers.E, 0x8b, 0x0e, 0x01, false, false, true, false)]
        [TestCase(Registers.H, 0x8c, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.H, 0x8c, 0xff, 0x00, true, false, true, true)]
        [TestCase(Registers.H, 0x8c, 0x0e, 0x01, false, false, true, false)]
        [TestCase(Registers.L, 0x8d, 0x10, 0x00, false, false, false, false)]
        [TestCase(Registers.L, 0x8d, 0xff, 0x00, true, false, true, true)]
        [TestCase(Registers.L, 0x8d, 0x0e, 0x01, false, false, true, false)]
        public void AddWithCarry_SetsExpectedFlags(Registers register, byte opcode, byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            ClassUnderTest.Registers.F = 0b00010000;
            SetRegister(register, value2);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void AddWithCarryFromMemory_SetsAccumulator()
        {
            ClassUnderTest.Registers.A = 0x30;
            ClassUnderTest.Registers.F = 0b00010000;
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x8e);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0x10);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x41);
        }

        [Test]
        [TestCase(0x10, 0x00, false, false, false, false)]
        [TestCase(0xff, 0x00, true, false, true, true)]
        [TestCase(0x0e, 0x01, false, false, true, false)]
        public void AddWithCarryFromMemory_SetsExpectedFlags(byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            ClassUnderTest.Registers.F = 0b00010000;
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x8e);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Registers.B, 0x90)]
        [TestCase(Registers.C, 0x91)]
        [TestCase(Registers.D, 0x92)]
        [TestCase(Registers.E, 0x93)]
        [TestCase(Registers.H, 0x94)]
        [TestCase(Registers.L, 0x95)]
        [TestCase(Registers.A, 0x97)]
        public void Subtract_SetsAccumulator(Registers register, byte opcode)
        {
            ClassUnderTest.Registers.A = 0x20;
            SetRegister(register, 0x20);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x00);
        }

        [Test]
        [TestCase(Registers.B, 0x90, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.B, 0x90, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.B, 0x90, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.B, 0x90, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.C, 0x91, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.C, 0x91, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.C, 0x91, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.C, 0x91, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.D, 0x92, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.D, 0x92, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.D, 0x92, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.D, 0x92, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.E, 0x93, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.E, 0x93, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.E, 0x93, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.E, 0x93, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.H, 0x94, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.H, 0x94, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.H, 0x94, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.H, 0x94, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.L, 0x95, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.L, 0x95, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.L, 0x95, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.L, 0x95, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.A, 0x97, 0x10, 0x10, true, true, false, false)]
        public void Subtract_SetsExpectedFlags(Registers register, byte opcode, byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            SetRegister(register, value2);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void SubtractFromMemory_SetsAccumulator()
        {
            ClassUnderTest.Registers.A = 0x30;
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x96);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0x10);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x20);
        }

        [Test]
        [TestCase(0x10, 0x00, false, true, false, false)]
        [TestCase(0x10, 0x10, true, true, false, false)]
        [TestCase(0x10, 0x01, false, true, true, false)]
        [TestCase(0x10, 0x20, false, true, false, true)]
        public void SubtractFromMemory_SetsExpectedFlags(byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x96);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void SubtractImmediate_SetsAccumulator()
        {
            ClassUnderTest.Registers.A = 0x30;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xd6);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0x10);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x20);
        }

        [Test]
        [TestCase(0x10, 0x00, false, true, false, false)]
        [TestCase(0x10, 0x10, true, true, false, false)]
        [TestCase(0x10, 0x01, false, true, true, false)]
        [TestCase(0x10, 0x20, false, true, false, true)]
        public void SubtractImmediate_SetsExpectedFlags(byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xd6);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Registers.B, 0x98)]
        [TestCase(Registers.C, 0x99)]
        [TestCase(Registers.D, 0x9a)]
        [TestCase(Registers.E, 0x9b)]
        [TestCase(Registers.H, 0x9c)]
        [TestCase(Registers.L, 0x9d)]
        [TestCase(Registers.A, 0x9f)]
        public void SubtractWithCarry_SetsAccumulator(Registers register, byte opcode)
        {
            ClassUnderTest.Registers.A = 0x20;
            ClassUnderTest.Registers.F = 0b00010000;
            SetRegister(register, 0x20);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0xff);
        }

        [Test]
        [TestCase(Registers.B, 0x98, 0x11, 0x00, false, true, false, false)]
        [TestCase(Registers.B, 0x98, 0x01, 0x00, true, true, false, false)]
        [TestCase(Registers.B, 0x98, 0x11, 0x01, false, true, true, false)]
        [TestCase(Registers.B, 0x98, 0x11, 0x20, false, true, false, true)]
        [TestCase(Registers.C, 0x99, 0x11, 0x00, false, true, false, false)]
        [TestCase(Registers.C, 0x99, 0x01, 0x00, true, true, false, false)]
        [TestCase(Registers.C, 0x99, 0x11, 0x01, false, true, true, false)]
        [TestCase(Registers.C, 0x99, 0x11, 0x20, false, true, false, true)]
        [TestCase(Registers.D, 0x9a, 0x11, 0x00, false, true, false, false)]
        [TestCase(Registers.D, 0x9a, 0x01, 0x00, true, true, false, false)]
        [TestCase(Registers.D, 0x9a, 0x11, 0x01, false, true, true, false)]
        [TestCase(Registers.D, 0x9a, 0x11, 0x20, false, true, false, true)]
        [TestCase(Registers.E, 0x9b, 0x11, 0x00, false, true, false, false)]
        [TestCase(Registers.E, 0x9b, 0x01, 0x00, true, true, false, false)]
        [TestCase(Registers.E, 0x9b, 0x11, 0x01, false, true, true, false)]
        [TestCase(Registers.E, 0x9b, 0x11, 0x20, false, true, false, true)]
        [TestCase(Registers.H, 0x9c, 0x11, 0x00, false, true, false, false)]
        [TestCase(Registers.H, 0x9c, 0x01, 0x00, true, true, false, false)]
        [TestCase(Registers.H, 0x9c, 0x11, 0x01, false, true, true, false)]
        [TestCase(Registers.H, 0x9c, 0x11, 0x20, false, true, false, true)]
        [TestCase(Registers.L, 0x9d, 0x11, 0x00, false, true, false, false)]
        [TestCase(Registers.L, 0x9d, 0x01, 0x00, true, true, false, false)]
        [TestCase(Registers.L, 0x9d, 0x11, 0x01, false, true, true, false)]
        [TestCase(Registers.L, 0x9d, 0x11, 0x20, false, true, false, true)]
        public void SubtractWithCarry_SetsExpectedFlags(Registers register, byte opcode, byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            ClassUnderTest.Registers.F = 0b00010000;
            SetRegister(register, value2);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void SubtractWithCarryFromMemory_SetsAccumulator()
        {
            ClassUnderTest.Registers.A = 0x30;
            ClassUnderTest.Registers.F = 0b00010000;
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x9e);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0x10);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0x1f);
        }

        [Test]
        [TestCase(0x11, 0x00, false, true, false, false)]
        [TestCase(0x01, 0x00, true, true, false, false)]
        [TestCase(0x11, 0x01, false, true, true, false)]
        [TestCase(0x01, 0x01, false, true, true, true)]
        public void SubtractWithCarryFromMemory_SetsExpectedFlags(byte value1, byte value2, bool zero, bool subtract, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            ClassUnderTest.Registers.F = 0b00010000;
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x9e);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (subtract ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Registers.B, 0xa0)]
        [TestCase(Registers.C, 0xa1)]
        [TestCase(Registers.D, 0xa2)]
        [TestCase(Registers.E, 0xa3)]
        [TestCase(Registers.H, 0xa4)]
        [TestCase(Registers.L, 0xa5)]
        [TestCase(Registers.A, 0xa7)]
        public void And_UpdatesAccumulator(Registers register, byte opcode)
        {
            ClassUnderTest.Registers.A = 0b10101010;
            SetRegister(register, 0b11110000);
            var expectedResult = (byte)(ClassUnderTest.Registers.A & GetRegister(register));

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(expectedResult);
        }

        [Test]
        [TestCase(Registers.B, 0b00001111, 0b00001111, false, 0xa0)]
        [TestCase(Registers.B, 0b11110000, 0b00001111, true, 0xa0)]
        [TestCase(Registers.C, 0b00001111, 0b00001111, false, 0xa1)]
        [TestCase(Registers.C, 0b11110000, 0b00001111, true, 0xa1)]
        [TestCase(Registers.D, 0b00001111, 0b00001111, false, 0xa2)]
        [TestCase(Registers.D, 0b11110000, 0b00001111, true, 0xa2)]
        [TestCase(Registers.E, 0b00001111, 0b00001111, false, 0xa3)]
        [TestCase(Registers.E, 0b11110000, 0b00001111, true, 0xa3)]
        [TestCase(Registers.H, 0b00001111, 0b00001111, false, 0xa4)]
        [TestCase(Registers.H, 0b11110000, 0b00001111, true, 0xa4)]
        [TestCase(Registers.L, 0b00001111, 0b00001111, false, 0xa5)]
        [TestCase(Registers.L, 0b11110000, 0b00001111, true, 0xa5)]
        [TestCase(Registers.A, 0b00001111, 0b00001111, false, 0xa7)]
        [TestCase(Registers.A, 0b00001111, 0b00000000, true, 0xa7)]
        public void And_SetsExpectedFlags(Registers register, byte value1, byte value2, bool zero, byte opcode)
        {
            ClassUnderTest.Registers.A = value1;
            SetRegister(register, value2);

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte) ((zero ? 0b10000000 : 0) | 0b00100000);

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void AndMemory_UpdatesAccumulator()
        {
            ClassUnderTest.Registers.A = 0b10101010;
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xa6);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0b11110000);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0b10100000);
        }

        [Test]
        [TestCase(0b00001111, 0b00001111, false)]
        [TestCase(0b11110000, 0b00001111, true)]
        public void AndMemory_SetsExpectedFlags(byte value1, byte value2, bool zero)
        {
            ClassUnderTest.Registers.A = value1;
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xa6);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)((zero ? 0b10000000 : 0) | 0b00100000);

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void AndImmediate_UpdatesAccumulator()
        {
            ClassUnderTest.Registers.A = 0b10101010;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xe6);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0b11110000);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0b10100000);
        }

        [Test]
        [TestCase(0b00001111, 0b00001111, false)]
        [TestCase(0b11110000, 0b00001111, true)]
        public void AndImmediate_SetsExpectedFlags(byte value1, byte value2, bool zero)
        {
            ClassUnderTest.Registers.A = value1;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xe6);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)((zero ? 0b10000000 : 0) | 0b00100000);

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Registers.B, 0xb0)]
        [TestCase(Registers.C, 0xb1)]
        [TestCase(Registers.D, 0xb2)]
        [TestCase(Registers.E, 0xb3)]
        [TestCase(Registers.H, 0xb4)]
        [TestCase(Registers.L, 0xb5)]
        [TestCase(Registers.A, 0xb7)]
        public void Or_UpdatesAccumulator(Registers register, byte opcode)
        {
            ClassUnderTest.Registers.A = 0b10101010;
            SetRegister(register, 0b11110000);
            var expectedResult = (byte)(ClassUnderTest.Registers.A | GetRegister(register));

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(expectedResult);
        }

        [Test]
        [TestCase(Registers.B, 0b00001111, false, 0xb0)]
        [TestCase(Registers.B, 0b00000000, true, 0xb0)]
        [TestCase(Registers.C, 0b00001111, false, 0xb1)]
        [TestCase(Registers.C, 0b00000000, true, 0xb1)]
        [TestCase(Registers.D, 0b00001111, false, 0xb2)]
        [TestCase(Registers.D, 0b00000000, true, 0xb2)]
        [TestCase(Registers.E, 0b00001111, false, 0xb3)]
        [TestCase(Registers.E, 0b00000000, true, 0xb3)]
        [TestCase(Registers.H, 0b00001111, false, 0xb4)]
        [TestCase(Registers.H, 0b00000000, true, 0xb4)]
        [TestCase(Registers.L, 0b00001111, false, 0xb5)]
        [TestCase(Registers.L, 0b00000000, true, 0xb5)]
        [TestCase(Registers.A, 0b00000000, true, 0xb7)]
        public void Or_SetsExpectedFlags(Registers register, byte value1, bool zero, byte opcode)
        {
            ClassUnderTest.Registers.A = value1;
            SetRegister(register, 0);

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(zero ? 0b10000000 : 0);

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void OrMemory_UpdatesAccumulator()
        {
            ClassUnderTest.Registers.A = 0b10101010;
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xb6);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0b11110000);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0b11111010);
        }

        [Test]
        [TestCase(0b00001111, false)]
        [TestCase(0b00000000, true)]
        public void OrMemory_SetsExpectedFlags(byte value, bool zero)
        {
            ClassUnderTest.Registers.A = value;
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xb6);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(zero ? 0b10000000 : 0);

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void OrImmediate_UpdatesAccumulator()
        {
            ClassUnderTest.Registers.A = 0b10101010;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xf6);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0b11110000);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0b11111010);
        }

        [Test]
        [TestCase(0b00001111, false)]
        [TestCase(0b00000000, true)]
        public void OrImmediate_SetsExpectedFlags(byte value, bool zero)
        {
            ClassUnderTest.Registers.A = value;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xf6);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(zero ? 0b10000000 : 0);

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Registers.B, 0xa8)]
        [TestCase(Registers.C, 0xa9)]
        [TestCase(Registers.D, 0xaa)]
        [TestCase(Registers.E, 0xab)]
        [TestCase(Registers.H, 0xac)]
        [TestCase(Registers.L, 0xad)]
        [TestCase(Registers.A, 0xaf)]
        public void Xor_UpdatesAccumulator(Registers register, byte opcode)
        {
            ClassUnderTest.Registers.A = 0b10101010;
            SetRegister(register, 0b11110000);
            var expectedResult = (byte) (ClassUnderTest.Registers.A ^ GetRegister(register));

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(expectedResult);
        }

        [Test]
        [TestCase(Registers.B, 0b11111111, false, 0xa8)]
        [TestCase(Registers.B, 0b11110000, true, 0xa8)]
        [TestCase(Registers.C, 0b11111111, false, 0xa9)]
        [TestCase(Registers.C, 0b11110000, true, 0xa9)]
        [TestCase(Registers.D, 0b11111111, false, 0xaa)]
        [TestCase(Registers.D, 0b11110000, true, 0xaa)]
        [TestCase(Registers.E, 0b11111111, false, 0xab)]
        [TestCase(Registers.E, 0b11110000, true, 0xab)]
        [TestCase(Registers.H, 0b11111111, false, 0xac)]
        [TestCase(Registers.H, 0b11110000, true, 0xac)]
        [TestCase(Registers.L, 0b11111111, false, 0xad)]
        [TestCase(Registers.L, 0b11110000, true, 0xad)]
        [TestCase(Registers.A, 0b11110000, true, 0xaf)]
        public void Xor_SetsExpectedFlags(Registers register, byte value1, bool zero, byte opcode)
        {
            ClassUnderTest.Registers.A = value1;
            SetRegister(register, 0b11110000);

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(zero ? 0b10000000 : 0);

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void XorMemory_UpdatesAccumulator()
        {
            ClassUnderTest.Registers.A = 0b10101010;
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xae);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0b11110000);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0b01011010);
        }

        [Test]
        [TestCase(0b11111111, false)]
        [TestCase(0b11110000, true)]
        public void XorMemory_SetsExpectedFlags(byte value, bool zero)
        {
            ClassUnderTest.Registers.A = value;
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xae);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(0b11110000);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(zero ? 0b10000000 : 0);

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void XorImmediate_UpdatesAccumulator()
        {
            ClassUnderTest.Registers.A = 0b10101010;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xee);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0b11110000);

            ClassUnderTest.Next();

            ClassUnderTest.Registers.A.Should().Be(0b01011010);
        }

        [Test]
        [TestCase(0b11111111, false)]
        [TestCase(0b11110000, true)]
        public void XorImmediate_SetsExpectedFlags(byte value, bool zero)
        {
            ClassUnderTest.Registers.A = value;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xee);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(0b11110000);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(zero ? 0b10000000 : 0);

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Registers.B, 0xb8, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.B, 0xb8, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.B, 0xb8, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.B, 0xb8, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.C, 0xb9, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.C, 0xb9, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.C, 0xb9, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.C, 0xb9, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.D, 0xba, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.D, 0xba, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.D, 0xba, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.D, 0xba, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.E, 0xbb, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.E, 0xbb, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.E, 0xbb, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.E, 0xbb, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.H, 0xbc, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.H, 0xbc, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.H, 0xbc, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.H, 0xbc, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.L, 0xbd, 0x10, 0x00, false, true, false, false)]
        [TestCase(Registers.L, 0xbd, 0x00, 0x00, true, true, false, false)]
        [TestCase(Registers.L, 0xbd, 0x10, 0x01, false, true, true, false)]
        [TestCase(Registers.L, 0xbd, 0x10, 0x20, false, true, false, true)]
        [TestCase(Registers.A, 0xbf, 0x10, 0x10, true, true, false, false)]
        public void Compare_SetsExpectedFlags(Registers register, byte opcode, byte value1, byte value2, bool zero, bool Compare, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            SetRegister(register, value2);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (Compare ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(0x10, 0x00, false, true, false, false)]
        [TestCase(0x10, 0x10, true, true, false, false)]
        [TestCase(0x10, 0x01, false, true, true, false)]
        [TestCase(0x10, 0x20, false, true, false, true)]
        public void CompareFromMemory_SetsExpectedFlags(byte value1, byte value2, bool zero, bool Compare, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            ClassUnderTest.Registers.HL = 0x1234;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xbe);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (Compare ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(0x10, 0x00, false, true, false, false)]
        [TestCase(0x10, 0x10, true, true, false, false)]
        [TestCase(0x10, 0x01, false, true, true, false)]
        [TestCase(0x10, 0x20, false, true, false, true)]
        public void CompareImmediate_SetsExpectedFlags(byte value1, byte value2, bool zero, bool Compare, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.A = value1;
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0xfe);
            GetMock<IBus>().Setup(m => m.Read(0x101)).Returns(value2);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (Compare ? 0b01000000 : 0)
                | (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Registers.B, 0x04)]
        [TestCase(Registers.C, 0x0c)]
        [TestCase(Registers.D, 0x14)]
        [TestCase(Registers.E, 0x1c)]
        [TestCase(Registers.H, 0x24)]
        [TestCase(Registers.L, 0x2c)]
        [TestCase(Registers.A, 0x3c)]
        public void Increment_UpdatesRegister(Registers register, byte opcode)
        {
            SetRegister(register, 41);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            GetRegister(register).Should().Be(42);
        }

        [Test]
        [TestCase(Registers.B, 0x04, 0x00, false, false)]
        [TestCase(Registers.B, 0x04, 0xff, true, true)]
        [TestCase(Registers.B, 0x04, 0x0f, false, true)]
        [TestCase(Registers.C, 0x0c, 0x00, false, false)]
        [TestCase(Registers.C, 0x0c, 0xff, true, true)]
        [TestCase(Registers.C, 0x0c, 0x0f, false, true)]
        [TestCase(Registers.D, 0x14, 0x00, false, false)]
        [TestCase(Registers.D, 0x14, 0xff, true, true)]
        [TestCase(Registers.D, 0x14, 0x0f, false, true)]
        [TestCase(Registers.E, 0x1c, 0x00, false, false)]
        [TestCase(Registers.E, 0x1c, 0xff, true, true)]
        [TestCase(Registers.E, 0x1c, 0x0f, false, true)]
        [TestCase(Registers.H, 0x24, 0x00, false, false)]
        [TestCase(Registers.H, 0x24, 0xff, true, true)]
        [TestCase(Registers.H, 0x24, 0x0f, false, true)]
        [TestCase(Registers.L, 0x2c, 0x00, false, false)]
        [TestCase(Registers.L, 0x2c, 0xff, true, true)]
        [TestCase(Registers.L, 0x2c, 0x0f, false, true)]
        [TestCase(Registers.A, 0x3c, 0x00, false, false)]
        [TestCase(Registers.A, 0x3c, 0xff, true, true)]
        [TestCase(Registers.A, 0x3c, 0x0f, false, true)]
        public void Increment_SetsExpectedFlags(Registers register, byte opcode, byte startValue, bool zero, bool halfCarry)
        {
            SetRegister(register, startValue);

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte) (
                (zero ? 0b10000000 : 0)
                | (halfCarry ? 0b00100000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void IncrementMemory_SetsValue()
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x34);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(41);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0x1234, 42), Times.Once);
        }

        [Test]
        [TestCase(0x00, false, false)]
        [TestCase(0xff, true, true)]
        [TestCase(0x0f, false, true)]
        public void IncrementMemory_SetsExpectedFlags(byte startValue, bool zero, bool halfCarry)
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x34);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(startValue);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (halfCarry ? 0b00100000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Registers.B, 0x05)]
        [TestCase(Registers.C, 0x0d)]
        [TestCase(Registers.D, 0x15)]
        [TestCase(Registers.E, 0x1d)]
        [TestCase(Registers.H, 0x25)]
        [TestCase(Registers.L, 0x2d)]
        [TestCase(Registers.A, 0x3d)]
        public void Decrement_UpdatesRegister(Registers register, byte opcode)
        {
            SetRegister(register, 43);
            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            GetRegister(register).Should().Be(42);
        }

        [Test]
        [TestCase(Registers.B, 0x05, 0x02, false, false)]
        [TestCase(Registers.B, 0x05, 0x01, true, false)]
        [TestCase(Registers.B, 0x05, 0x10, false, true)]
        [TestCase(Registers.C, 0x0d, 0x02, false, false)]
        [TestCase(Registers.C, 0x0d, 0x01, true, false)]
        [TestCase(Registers.C, 0x0d, 0x10, false, true)]
        [TestCase(Registers.D, 0x15, 0x02, false, false)]
        [TestCase(Registers.D, 0x15, 0x01, true, false)]
        [TestCase(Registers.D, 0x15, 0x10, false, true)]
        [TestCase(Registers.E, 0x1d, 0x02, false, false)]
        [TestCase(Registers.E, 0x1d, 0x01, true, false)]
        [TestCase(Registers.E, 0x1d, 0x10, false, true)]
        [TestCase(Registers.H, 0x25, 0x02, false, false)]
        [TestCase(Registers.H, 0x25, 0x01, true, false)]
        [TestCase(Registers.H, 0x25, 0x10, false, true)]
        [TestCase(Registers.L, 0x2d, 0x02, false, false)]
        [TestCase(Registers.L, 0x2d, 0x01, true, false)]
        [TestCase(Registers.L, 0x2d, 0x10, false, true)]
        [TestCase(Registers.A, 0x3d, 0x02, false, false)]
        [TestCase(Registers.A, 0x3d, 0x01, true, false)]
        [TestCase(Registers.A, 0x3d, 0x10, false, true)]
        public void Decrement_SetsExpectedFlags(Registers register, byte opcode, byte startValue, bool zero, bool halfCarry)
        {
            SetRegister(register, startValue);

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (halfCarry ? 0b00100000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        public void DecrementMemory_SetsValue()
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x35);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(43);

            ClassUnderTest.Next();

            GetMock<IBus>().Verify(m => m.Write(0x1234, 42), Times.Once);
        }

        [Test]
        [TestCase(0x02, false, false)]
        [TestCase(0x01, true, false)]
        [TestCase(0x10, false, true)]
        public void DecrementMemory_SetsExpectedFlags(byte startValue, bool zero, bool halfCarry)
        {
            ClassUnderTest.Registers.HL = 0x1234;

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(0x35);
            GetMock<IBus>().Setup(m => m.Read(0x1234)).Returns(startValue);

            ClassUnderTest.Next();

            var expectedFlags = (byte)(
                (zero ? 0b10000000 : 0)
                | (halfCarry ? 0b00100000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        [Test]
        [TestCase(Register16s.BC, 0x09)]
        [TestCase(Register16s.DE, 0x19)]
        [TestCase(Register16s.HL, 0x29)]
        [TestCase(Register16s.SP, 0x39)]
        public void AddHL_SetsValue(Register16s register, byte opcode)
        {
            ClassUnderTest.Registers.HL = 0x1000;
            SetRegister(register, 42);
            var expectedResult = ClassUnderTest.Registers.HL + GetRegister(register);

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            ClassUnderTest.Registers.HL.Should().Be(expectedResult);
        }

        [Test]
        [TestCase(Register16s.BC, 0x09, (ushort)0x1234, (ushort)0x0001, false, false)]
        [TestCase(Register16s.BC, 0x09, (ushort)0x1fff, (ushort)0x0001, true, false)]
        [TestCase(Register16s.BC, 0x09, (ushort)0xffff, (ushort)0x0001, true, true)]
        [TestCase(Register16s.DE, 0x19, (ushort)0x1234, (ushort)0x0001, false, false)]
        [TestCase(Register16s.DE, 0x19, (ushort)0x1fff, (ushort)0x0001, true, false)]
        [TestCase(Register16s.DE, 0x19, (ushort)0xffff, (ushort)0x0001, true, true)]
        [TestCase(Register16s.HL, 0x29, (ushort)0x1234, (ushort)0x0001, false, false)]
        [TestCase(Register16s.HL, 0x29, (ushort)0x1fff, (ushort)0x0001, true, false)]
        [TestCase(Register16s.HL, 0x29, (ushort)0xffff, (ushort)0x0001, true, true)]
        [TestCase(Register16s.SP, 0x39, (ushort)0x1234, (ushort)0x0001, false, false)]
        [TestCase(Register16s.SP, 0x39, (ushort)0x1fff, (ushort)0x0001, true, false)]
        [TestCase(Register16s.SP, 0x39, (ushort)0xffff, (ushort)0x0001, true, true)]
        public void AddHL_SetsExpectedFlags(Register16s register, byte opcode, ushort value1, ushort value2, bool halfCarry, bool carry)
        {
            ClassUnderTest.Registers.HL = value1;
            SetRegister(register, value2);
            var expectedResult = ClassUnderTest.Registers.HL + GetRegister(register);

            GetMock<IBus>().Setup(m => m.Read(0x100)).Returns(opcode);

            var expectedFlags = (byte) (
                (halfCarry ? 0b00100000 : 0)
                | (carry ? 0b00010000 : 0));

            ClassUnderTest.Registers.F.Should().Be(expectedFlags);
        }

        private byte GetRegister(Registers register) =>
            register switch
            {
                Registers.A => ClassUnderTest.Registers.A,
                Registers.B => ClassUnderTest.Registers.B,
                Registers.C => ClassUnderTest.Registers.C,
                Registers.D => ClassUnderTest.Registers.D,
                Registers.E => ClassUnderTest.Registers.E,
                Registers.H => ClassUnderTest.Registers.H,
                Registers.L => ClassUnderTest.Registers.L,
                _ => throw new ArgumentException("Invalid register")
            };

        private ushort GetRegister(Register16s register) =>
            register switch
            {
                Register16s.AF => ClassUnderTest.Registers.AF,
                Register16s.BC => ClassUnderTest.Registers.BC,
                Register16s.DE => ClassUnderTest.Registers.DE,
                Register16s.HL => ClassUnderTest.Registers.HL,
                Register16s.PC => ClassUnderTest.Registers.PC,
                Register16s.SP => ClassUnderTest.Registers.SP,
                _ => throw new ArgumentException("Invalid register")
            };

        private byte SetRegister(Registers register, byte value) =>
            register switch
            {
                Registers.A => ClassUnderTest.Registers.A = value,
                Registers.B => ClassUnderTest.Registers.B = value,
                Registers.C => ClassUnderTest.Registers.C = value,
                Registers.D => ClassUnderTest.Registers.D = value,
                Registers.E => ClassUnderTest.Registers.E = value,
                Registers.H => ClassUnderTest.Registers.H = value,
                Registers.L => ClassUnderTest.Registers.L = value,
                _ => throw new ArgumentException("Invalid register")
            };

        private ushort SetRegister(Register16s register, ushort value) =>
            register switch
            {
                Register16s.AF => ClassUnderTest.Registers.AF = value,
                Register16s.BC => ClassUnderTest.Registers.BC = value,
                Register16s.DE => ClassUnderTest.Registers.DE = value,
                Register16s.HL => ClassUnderTest.Registers.HL = value,
                Register16s.PC => ClassUnderTest.Registers.PC = value,
                Register16s.SP => ClassUnderTest.Registers.SP = value,
                _ => throw new ArgumentException("Invalid register")
            };
    }

    public enum Registers
    {
        A,
        B,
        C,
        D,
        E,
        H,
        L,
    }

    public enum Register16s
    {
        AF,
        BC,
        DE,
        HL,
        PC,
        SP,
    }
}
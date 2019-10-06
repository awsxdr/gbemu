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
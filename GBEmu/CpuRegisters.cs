namespace GBEmu
{
    public class CpuRegisters
    {
        private DoubleRegister _af;
        public DoubleRegister AF
        {
            get => _af;
            set => _af = (DoubleRegister)(value & 0xfff0); // Only the top 4 bits of the F register should ever be set
        }
        public byte A
        {
            get => _af.Upper;
            set => _af.Upper = value;
        }
        public byte F
        {
            get => _af.Lower;
            set => _af.Lower = (byte)(value & 0xf0); // Only the top 4 bits should ever be set
        }

        private DoubleRegister _bc;
        public DoubleRegister BC
        {
            get => _bc;
            set => _bc = value;
        }
        public byte B
        {
            get => _bc.Upper;
            set => _bc.Upper = value;
        }
        public byte C
        {
            get => _bc.Lower;
            set => _bc.Lower = value;
        }

        private DoubleRegister _de;
        public DoubleRegister DE
        {
            get => _de;
            set => _de = value;
        }
        public byte D
        {
            get => _de.Upper;
            set => _de.Upper = value;
        }
        public byte E
        {
            get => _de.Lower;
            set => _de.Lower = value;
        }

        private DoubleRegister _hl;
        public DoubleRegister HL
        {
            get => _hl;
            set => _hl = value;
        }
        public byte H
        {
            get => _hl.Upper;
            set => _hl.Upper = value;
        }
        public byte L
        {
            get => _hl.Lower;
            set => _hl.Lower = value;
        }

        public DoubleRegister PC { get; set; }
        public DoubleRegister SP { get; set; }
    }
}
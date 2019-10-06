namespace GBEmu
{
    public class CpuRegisters
    {
        private Register16 _af;
        public Register16 AF
        {
            get => _af;
            set => _af = value;
        }
        public byte A
        {
            get => _af.Upper;
            set => _af.Upper = value;
        }
        public byte F
        {
            get => _af.Lower;
            set => _af.Lower = value;
        }

        private Register16 _bc;
        public Register16 BC
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

        private Register16 _de;
        public Register16 DE
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

        private Register16 _hl;
        public Register16 HL
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

        public Register16 PC { get; set; }
        public Register16 SP { get; set; }
    }
}
namespace GBEmu
{
    public struct DoubleRegister
    {
        public byte Upper { get; set; }
        public byte Lower { get; set; }

        public static implicit operator DoubleRegister(ushort value) =>
            new DoubleRegister
            {
                Upper = (byte)((value & 0xff00) >> 8),
                Lower = (byte)(value & 0xff),
            };

        public static implicit operator ushort(DoubleRegister value) =>
            (ushort)((value.Upper << 8) | value.Lower);

        public override string ToString() => ((ushort) this).ToString("x");
    }
}
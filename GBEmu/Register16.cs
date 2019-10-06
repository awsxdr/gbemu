namespace GBEmu
{
    public struct Register16
    {
        public byte Upper { get; set; }
        public byte Lower { get; set; }

        public static implicit operator Register16(ushort value) =>
            new Register16
            {
                Upper = (byte)((value & 0xff00) >> 8),
                Lower = (byte)(value & 0xff),
            };

        public static implicit operator ushort(Register16 value) =>
            (ushort)((value.Upper << 8) | value.Lower);

        public override string ToString() => ((ushort) this).ToString("x");
    }
}
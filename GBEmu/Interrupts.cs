namespace GBEmu
{
    using System;

    [Flags]
    public enum Interrupts
    {
        VerticalBlank = 0b00000001,
        LcdStatus = 0b00000010,
        Timer = 0b00000100,
        Serial = 0b00001000,
        Joypad = 0b00010000,
    }
}
using System;

namespace GBEmu
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameboy = new GameBoy();
            gameboy.LoadCartridge(@"C:\Users\Daniel Errington\Downloads\cpu_instrs.gb");
            gameboy.Start();
        }
    }
}

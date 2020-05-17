using System;

namespace GBEmu
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameboy = new GameBoy();
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\cpu_instrs.gb");
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\01-special.gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\02-interrupts.gb");
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\03-op sp,hl.gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\04-op r,imm.gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\05-op rp.gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\06-ld r,r.gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\07-jr,jp,call,ret,rst.gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\08-misc instrs.gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\09-op r,r.gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\10-bit ops.gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\gb-test-roms\cpu_instrs\individual\11-op a,(hl).gb"); // Passed
            //gameboy.LoadCartridge(@"..\..\..\..\..\merken.gb");
            //gameboy.LoadCartridge(@"..\..\..\..\..\pac-man.gb");
            //gameboy.LoadCartridge(@"..\..\..\..\..\pokemon.gb");
            //gameboy.LoadCartridge(@"..\..\..\..\..\space-invaders.gb");
            gameboy.LoadCartridge(@"..\..\..\..\..\super-mario.gb");
            //gameboy.LoadCartridge(@"..\..\..\..\..\tetris.gb");
            gameboy.Start();
        }
    }
}

namespace GBEmu
{
    using ConsoleGraphics;

    public class GameBoy
    {
        private readonly Cpu _cpu;
        private readonly Display _display;

        public GameBoy()
        {
            var bus = new Bus();
            var bios = Rom.FromFile("dmg_boot.bin");
            var internalRam = new Ram(8 * 1024);
            var highRam = new Ram(0x7f);

            const int screenWidth = 160;
            const int screenHeight = 144;
            var consoleOutput = new ConsoleOutput(screenWidth, screenHeight);
            _display = new Display(consoleOutput);

            bus.AttachDevice(0x0000, 0x0100, bios);
            bus.AttachDevice(0x8000, 0x2000, _display);
            bus.AttachDevice(0xc000, 0x2000, internalRam);
            bus.AttachDevice(0xe000, 0x1e00, internalRam);
            bus.AttachDevice(0xff40, 0x000b, _display.DisplayControl);
            bus.AttachDevice(0xff80, 0x7f, highRam);

            _cpu = new Cpu(bus);
        }

        public void Start()
        {
            var i = 0;

            while (true)
            {
                _cpu.Next();
                if(++i == 1000)
                {
                    i = 0;
                    _display.Refresh();
                }
            }
        }
    }
}
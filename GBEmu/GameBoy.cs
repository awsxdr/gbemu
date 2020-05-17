namespace GBEmu
{
    using ConsoleGraphics;

    public class GameBoy
    {
        private readonly Bus _bus;
        private Rom _loadedCartridge;
        private readonly Clock _clock;
        private readonly Rom _bios;

        public GameBoy()
        {
            _bus = new Bus();
            var cpu = new Cpu(_bus);
            _bios = Rom.FromFile("dmg_boot.bin");
            var internalRam = new Ram(8 * 1024);
            var highRam = new Ram(0x7f);
            var biosDisableRam = new Ram(0x01);
            var interruptEnable = new Ram(0x01);
            var interruptFlag = new Ram(0x01);
            var ioPorts = new Ram(0x08);
            var divider = new Divider();
            var timer = new Timer(cpu);
            _clock = new Clock();

            const int screenWidth = 160;
            const int screenHeight = 144;
            var consoleOutput = new ConsoleOutput(screenWidth, screenHeight);
            var display = new Display(_bus, consoleOutput);

            _bus.AttachDevice(0x0000, 0x0100, _bios);
            _bus.AttachDevice(0x8000, 0x2000, display);
            _bus.AttachDevice(0xc000, 0x2000, internalRam);
            _bus.AttachDevice(0xe000, 0x1e00, internalRam);
            _bus.AttachDevice(0xff00, 0x0005, ioPorts);
            _bus.AttachDevice(0xff05, 0x0003, timer);
            _bus.AttachDevice(0xff0f, 0x0001, interruptFlag);
            _bus.AttachDevice(0xff40, 0x000b, display.DisplayControl);
            _bus.AttachDevice(0xff50, 0x0001, biosDisableRam);
            _bus.AttachDevice(0xff80, 0x7f, highRam);
            _bus.AttachDevice(0xffff, 0x0001, interruptEnable);

            biosDisableRam.WatchWrite(0x0000, (a, v) => _bus.RemoveDevice(_bios));
            interruptFlag.WatchWrite(0x0000, (a, v) => cpu.Interrupt((Interrupts)v));

            cpu.ConnectClock(_clock);
            display.ConnectClock(_clock);
            divider.ConnectClock(_clock);
            timer.ConnectClock(_clock);
        }

        public void Start()
        {
            _clock.Start();
        }

        public void LoadCartridge(string filePath)
        {
            if (_loadedCartridge != null)
                _bus.RemoveDevice(_loadedCartridge);

            _loadedCartridge = Rom.FromFile(filePath);
            _bus.AttachDevice(0x0000, 0x8000, _loadedCartridge);
            _bus.AttachDevice(0x0000, 0x0100, _bios);
        }
    }
}
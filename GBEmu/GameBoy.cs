namespace GBEmu
{
    using System.IO;

    public class GameBoy
    {
        private readonly Cpu _cpu;

        public GameBoy()
        {
            var bus = new Bus();
            var bios = Rom.FromFile("dmg_boot.bin");
            var internalRam = new Ram(8 * 1024);
            var videoRam = new Ram(8 * 1024);
            var highRam = new Ram(0x7f);

            bus.AttachDevice(0x0000, 0x0100, bios);
            bus.AttachDevice(0x8000, 0x2000, videoRam);
            bus.AttachDevice(0xc000, 0x2000, internalRam);
            bus.AttachDevice(0xe000, 0x1e00, internalRam);
            bus.AttachDevice(0xff80, 0x7f, highRam);

            _cpu = new Cpu(bus);
        }

        public void Start()
        {

            while (true)
            {
                _cpu.Next();
            }
        }
    }
}
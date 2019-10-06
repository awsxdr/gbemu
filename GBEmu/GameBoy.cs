namespace GBEmu
{
    public class GameBoy
    {
        public GameBoy()
        {
            var bus = new Bus();
            var internalRam = new Ram(8 * 1024);
            var videoRam = new Ram(8 * 1024);

            bus.AttachDevice(0x8000, 0x9fff, videoRam);
            bus.AttachDevice(0xc000, 0xdfff, internalRam);
            bus.AttachDevice(0xe000, 0xfe00, internalRam);

        }
    }
}
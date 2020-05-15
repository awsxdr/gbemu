namespace GBEmu
{
    using System.Linq;
    using ConsoleGraphics;

    public class Display : Ram
    {
        private readonly IConsoleOutput _consoleOutput;

        public DisplayControl DisplayControl { get; }

        public Display(IConsoleOutput consoleOutput) : base(8 * 1024)
        {
            _consoleOutput = consoleOutput;

            _consoleOutput.SetPalette(new uint[]
            {
                0x0fbc9b,
                0x0fac8b,
                0x306230,
                0x0f380f,
                0xffffff,
                0xb0b0b0,
                0x808080,
                0x404040,
                0x000000,
                0xff0000,
                0x00ff00,
                0x0000ff,
                0xffff00,
                0xff00ff,
                0x00ffff,
                0xff0088,
            });

            DisplayControl = new DisplayControl();
        }

        public void Refresh()
        {
            _consoleOutput.WriteImage(GetImage());
        }

        public byte[][] GetImage()
        {
            var tiles = 
            _memory.Take(0x1000)
                .Select((x, i) => (Value: x, Index: i / 2))
                .GroupBy(x => x.Index)
                .Select(x => x.Select(y => y.Value).ToArray())
                .SelectMany(x =>
                    Enumerable.Range(0, 8).Reverse().Select(b => (((x[0] & (1 << b)) << 1) | (x[1] & (1 << b))) >> b))
                .Select((x, i) => (Value: (byte) x, Row: i / 64))
                .GroupBy(x => x.Row)
                .Select(x => x.Select(y => y.Value).ToArray())
                .ToArray();

            var image = new byte[160][];
            for (var x = 0; x < 160; ++x)
            {
                image[x] = new byte[144];
                for (var y = 0; y < 144; ++y)
                {
                    var tileIndex = _memory[0x1800 + x / 8 + (y / 8) * 32];
                    image[x][y] = tiles[tileIndex][x % 8 + (y % 8) * 8];
                }
            }

            return image;
        }

        public void ConnectClock(Clock clock)
        {
            var tickCount = 0;
            clock.Tick += () =>
            {
                if (++tickCount > 100000)
                {
                    tickCount = 0;
                    Refresh();
                }
            };
        }
    }

    public class DisplayControl : Ram
    {
        public DisplayControl() : base(0x0b)
        {
        }
    }
}
namespace GBEmu.ConsoleGraphics
{
    using System;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class ConsoleOutput : IConsoleOutput
    {
        private readonly short _width;
        private readonly short _height;
        private static readonly IntPtr OutputHandle = ConsoleExternalMethods.GetStdHandle(-11);

        public ConsoleOutput(short width, short height)
        {
            _width = width;
            _height = height;

            ConsoleExternalMethods.SetConsoleMode(OutputHandle, 0x80);

            var fontInfo = new ConsoleExternalMethods.CONSOLE_FONT_INFO_EX();
            fontInfo.cbSize = (uint)Marshal.SizeOf(fontInfo);
            fontInfo.dwFontSize = new ConsoleExternalMethods.Coord(5, 5);

            ConsoleExternalMethods.SetCurrentConsoleFontEx(OutputHandle, false, ref fontInfo);
            
            Console.SetWindowSize(width, height);
            Console.SetBufferSize(width, height);

            ConsoleExternalMethods.WriteConsoleOutputCharacter(
                OutputHandle,
                new string(Enumerable.Repeat((char)219, _width * _height).ToArray()),
                (ushort)(_width * _height),
                new ConsoleExternalMethods.Coord(0, 0),
                out _);

            ConsoleExternalMethods.WriteConsoleOutputAttribute(
                OutputHandle,
                Enumerable.Repeat((ushort)0, _width * _height).ToArray(),
                (uint)(_width * _height),
                new ConsoleExternalMethods.Coord(0, 0),
                out _);

            Console.CursorVisible = false;
        }

        public void SetPalette(uint[] colors)
        {
            if(colors.Length != 16) throw new ArgumentException($"{nameof(colors)} must have exactly 16 elements.", nameof(colors));

            var info = new ConsoleExternalMethods.CONSOLE_SCREEN_BUFFER_INFO_EX { ColorTable = new uint[16] };
            info.cbSize = Marshal.SizeOf(info);

            var result = ConsoleExternalMethods.GetConsoleScreenBufferInfoEx(OutputHandle, ref info);

            info.ColorTable = colors;

            result = ConsoleExternalMethods.SetConsoleScreenBufferInfoEx(OutputHandle, ref info);
        }

        public void WriteImage(byte[][] image)
        {
            var colors = image
                    .SelectMany(x => x.Select((y, i) => (y, i)))
                    .GroupBy(x => x.i)
                    .SelectMany(x => x.Select(y => y.y))
                    .Select(x => (ushort)x)
                    .ToArray();

            ConsoleExternalMethods.WriteConsoleOutputAttribute(
                OutputHandle,
                colors,
                (uint)colors.Length,
                new ConsoleExternalMethods.Coord(0, 0),
                out _);
        }
    }
}

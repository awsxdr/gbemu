namespace GBEmu.ConsoleGraphics
{
    public interface IConsoleOutput
    {
        void SetPalette(uint[] colors);
        void WriteImage(byte[][] image);
    }
}
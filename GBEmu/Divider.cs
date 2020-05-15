namespace GBEmu
{
    public class Divider : Ram
    {
        public Divider() : base(1)
        {

        }

        public override byte Read(ushort address) => _memory[0];
        public override void Write(ushort address, byte data)
        {
            base.Write(address, data);
            _memory[0] = 0;
        }

        public void ConnectClock(Clock clock)
        {
            var tickCount = 0;
            clock.Tick += () =>
            {
                if (++tickCount == 256)
                {
                    _memory[0]++;
                    tickCount = 0;
                }
            };
        }
    }
}
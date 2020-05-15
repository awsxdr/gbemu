namespace GBEmu
{
    public class Ram : BaseIoDevice
    {
        protected readonly byte[] _memory;

        public Ram(ushort size) =>
            _memory = new byte[size];

        public override void Write(ushort address, byte data)
        {
            base.Write(address, data);
            _memory[address] = data;
        }

        public override byte Read(ushort address) =>
            _memory[address];
    }
}
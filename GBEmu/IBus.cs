namespace GBEmu
{
    public interface IBus : IIoDevice
    {
        void AttachDevice(ushort startAddress, ushort endAddress, IIoDevice device);
    }
}
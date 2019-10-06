namespace GBEmu
{
    public interface IBus : IIoDevice
    {
        void AttachDevice(uint startAddress, uint endAddress, IIoDevice device);
    }
}
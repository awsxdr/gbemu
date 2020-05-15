namespace GBEmu
{
    using System;

    public interface IBus : IIoDevice
    {
        void AttachDevice(ushort startAddress, ushort endAddress, IIoDevice device);
        void RemoveDevice(IIoDevice device);
        void RemoveDevice(Guid deviceId);
    }
}
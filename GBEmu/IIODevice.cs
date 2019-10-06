namespace GBEmu
{
    using System.Collections.Generic;

    public interface IIoDevice
    {
        void Write(ushort address, byte data);
        void Write(ushort address, IEnumerable<byte> data);

        byte Read(ushort address);
        IReadOnlyCollection<byte> Read(ushort address, uint length);
    }
}
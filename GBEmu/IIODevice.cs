namespace GBEmu
{
    using System;
    using System.Collections.Generic;

    public interface IIoDevice
    {
        public Guid Id { get; }

        void Write(ushort address, byte data);
        void Write(ushort address, IEnumerable<byte> data);
        void WatchWrite(ushort address, Action<ushort, byte> onWrite);

        byte Read(ushort address);
        IReadOnlyCollection<byte> Read(ushort address, uint length);
    }
}
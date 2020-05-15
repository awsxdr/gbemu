namespace GBEmu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class BaseIoDevice : IIoDevice
    {
        protected readonly Dictionary<ushort, Action<ushort, byte>> WriteWatchList = new Dictionary<ushort, Action<ushort, byte>>();

        public Guid Id { get; } = Guid.NewGuid();

        public virtual void Write(ushort address, byte data)
        {
            if (WriteWatchList.ContainsKey(address))
                WriteWatchList[address](address, data);
        }

        public virtual void Write(ushort address, IEnumerable<byte> data) =>
            data.Select((x, i) =>
            {
                Write((ushort)(address + i), x);
                return x;
            }).ToArray();

        public virtual void WatchWrite(ushort address, Action<ushort, byte> onWrite) =>
            WriteWatchList[address] = onWrite;

        public abstract byte Read(ushort address);

        public virtual IReadOnlyCollection<byte> Read(ushort address, uint length) =>
            Enumerable.Range(address, (int) length)
                .Select(x => (ushort) x)
                .Select(Read)
                .ToArray();
    }
}
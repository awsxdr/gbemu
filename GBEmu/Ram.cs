namespace GBEmu
{
    using System.Collections.Generic;
    using System.Linq;

    public class Ram : IIoDevice
    {
        protected readonly byte[] _memory;

        public Ram(ushort size) =>
            _memory = new byte[size];

        public virtual void Write(ushort address, byte data) =>
            _memory[address] = data;

        public virtual void Write(ushort address, IEnumerable<byte> data) =>
            data.ToArray().CopyTo(_memory, address);

        public virtual byte Read(ushort address) =>
            _memory[address];

        public virtual IReadOnlyCollection<byte> Read(ushort address, uint length) =>
            _memory.Skip(address).Take((int) length).ToArray();
    }
}
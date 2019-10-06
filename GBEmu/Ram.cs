namespace GBEmu
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class Ram : IIoDevice
    {
        private readonly byte[] _memory;

        public Ram(ushort size) =>
            _memory = new byte[size];

        public void Write(ushort address, byte data) =>
            _memory[address] = data;

        public void Write(ushort address, IEnumerable<byte> data) =>
            data.ToArray().CopyTo(_memory, address);

        public byte Read(ushort address) =>
            _memory[address];

        public IReadOnlyCollection<byte> Read(ushort address, uint length) =>
            _memory.Skip(address).Take((int) length).ToArray();
    }
}
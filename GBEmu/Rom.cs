namespace GBEmu
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class Rom : IIoDevice
    {
        private readonly byte[] _data;

        public static Rom FromFile(string path) =>
            new Rom(File.ReadAllBytes(path));

        private Rom(byte[] data)
        {
            _data = data;
        }

        public void Write(ushort address, byte data) => throw new InvalidOperationException();
        public void Write(ushort address, IEnumerable<byte> data) => throw new InvalidOperationException();

        public byte Read(ushort address) => _data[address];

        public IReadOnlyCollection<byte> Read(ushort address, uint length) =>
            _data.Skip(address).Take((int)length).ToArray();
    }
}
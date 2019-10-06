namespace GBEmu
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class Rom : IIoDevice
    {
        public static Rom FromFile(string path) =>
            new Rom(File.ReadAllBytes(path));

        private Rom(byte[] data)
        {

        }

        public void Write(ushort address, byte data) => throw new InvalidOperationException();
        public void Write(ushort address, IEnumerable<byte> data) => throw new InvalidOperationException();

        public byte Read(ushort address)
        {
            throw new System.NotImplementedException();
        }

        public IReadOnlyCollection<byte> Read(ushort address, uint length)
        {
            throw new System.NotImplementedException();
        }
    }
}
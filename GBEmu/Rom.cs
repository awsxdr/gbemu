namespace GBEmu
{
    using System.Collections.Generic;
    using System.IO;

    public class Rom : BaseIoDevice
    {
        private readonly byte[] _data;

        public static Rom FromFile(string path) =>
            new Rom(File.ReadAllBytes(path));

        private Rom(byte[] data)
        {
            _data = data;
        }

        public override void Write(ushort address, byte data)
        {
        }

        public override void Write(ushort address, IEnumerable<byte> data)
        {
        }

        public override byte Read(ushort address) => _data[address];
    }
}
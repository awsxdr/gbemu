namespace GBEmu
{
    using System.Collections.Generic;

    public class Bus : IBus
    {
        public void AttachDevice(uint startAddress, uint endAddress, IIoDevice device)
        {
        }

        public void Write(ushort address, byte data)
        {
            throw new System.NotImplementedException();
        }

        public void Write(ushort address, IEnumerable<byte> data)
        {
            throw new System.NotImplementedException();
        }

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
namespace GBEmu
{
    using System.Collections.Generic;
    using System.Linq;

    public class Bus : IBus
    {
        private readonly List<AttachedDeviceDetails> _devices = new List<AttachedDeviceDetails>();

        public void AttachDevice(ushort startAddress, ushort size, IIoDevice device)
        {
            _devices.Add(new AttachedDeviceDetails
            {
                StartAddress = startAddress,
                Size = size,
                Device = device,
            });
        }

        public void Write(ushort address, byte data)
        {
            var device = GetDeviceAtAddress(address);

            device?.Device.Write((ushort)(address - device.StartAddress), data);
        }

        public void Write(ushort address, IEnumerable<byte> data)
        {
            var device = GetDeviceAtAddress(address);

            device?.Device.Write((ushort)(address - device.StartAddress), data);
        }

        public byte Read(ushort address)
        {
            var device = GetDeviceAtAddress(address);

            return device?.Device.Read((ushort)(address - device.StartAddress)) ?? 0x00;
        }

        public IReadOnlyCollection<byte> Read(ushort address, uint length)
        {
            var device = GetDeviceAtAddress(address);

            return device?.Device.Read((ushort)(address - device.StartAddress), length) ?? new byte[0];
        }

        private AttachedDeviceDetails GetDeviceAtAddress(ushort address) =>
            _devices.FirstOrDefault(x => x.StartAddress <= address && x.StartAddress + x.Size >= address);

        private class AttachedDeviceDetails
        {
            public ushort StartAddress { get; set; }
            public ushort Size { get; set; }
            public IIoDevice Device { get; set; }
        }
    }
}
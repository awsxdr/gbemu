namespace GBEmu
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Bus : BaseIoDevice, IBus
    {
        private readonly List<AttachedDeviceDetails> _devices = new List<AttachedDeviceDetails>();
        private readonly byte[] _memoryMap = new byte[0x10000];
        private readonly Dictionary<byte, AttachedDeviceDetails> _deviceMap = new Dictionary<byte, AttachedDeviceDetails>();

        public Bus()
        {
            ClearMemoryMap();
        }

        public void AttachDevice(ushort startAddress, ushort size, IIoDevice device)
        {
            var details = new AttachedDeviceDetails
            {
                StartAddress = startAddress,
                Size = size,
                Device = device,
            };
            _devices.Add(details);
            MapDevice(details);
        }

        public void RemoveDevice(IIoDevice device) => RemoveDevice(device.Id);

        public void RemoveDevice(Guid deviceId)
        {
            _devices.RemoveAll(x => x.Device.Id == deviceId);
            RebuildDeviceMap();
        }

        public override void Write(ushort address, byte data)
        {
            var device = GetDeviceAtAddress(address);

            device?.Device.Write((ushort)(address - device.StartAddress), data);
        }

        public override void Write(ushort address, IEnumerable<byte> data)
        {
            var device = GetDeviceAtAddress(address);

            device?.Device.Write((ushort)(address - device.StartAddress), data);
        }

        public override byte Read(ushort address)
        {
            var device = GetDeviceAtAddress(address);

            return device?.Device.Read((ushort)(address - device.StartAddress)) ?? 0x00;
        }

        public override IReadOnlyCollection<byte> Read(ushort address, uint length)
        {
            var device = GetDeviceAtAddress(address);

            return device?.Device.Read((ushort)(address - device.StartAddress), length) ?? new byte[0];
        }

        private AttachedDeviceDetails GetDeviceAtAddress(ushort address)
        {
            var deviceId = _memoryMap[address];

            return deviceId < 0xff
                ? _deviceMap[deviceId]
                : null;
        }
            
            //_devices.FirstOrDefault(x => x.StartAddress <= address && x.StartAddress + (int)x.Size >= address);


        private byte GetDeviceIndexForDevice(AttachedDeviceDetails device)
        {
            var existingDevice = _deviceMap.FirstOrDefault(x => x.Value.Device.Id.Equals(device.Device.Id) && x.Value.StartAddress.Equals(device.StartAddress));

            if (existingDevice.Value != null)
                return existingDevice.Key;

            var newId = (byte)(_deviceMap.Keys.Any() ? _deviceMap.Keys.Max() + 1 : 0);
            _deviceMap[newId] = device;

            return newId;
        }

        private void ClearMemoryMap()
        {
            for (var i = 0; i < _memoryMap.Length; ++i)
                _memoryMap[i] = 0xff;
        }

        private void RebuildDeviceMap()
        {
            _deviceMap.Clear();
            ClearMemoryMap();

            foreach (var device in _devices)
            {
                MapDevice(device);
            }
        }

        private void MapDevice(AttachedDeviceDetails deviceDetails)
        {
            var index = GetDeviceIndexForDevice(deviceDetails);

            for (var i = 0; i < deviceDetails.Size; ++i)
                _memoryMap[deviceDetails.StartAddress + i] = index;
        }

        private class AttachedDeviceDetails
        {
            public ushort StartAddress { get; set; }
            public ushort Size { get; set; }
            public IIoDevice Device { get; set; }
        }
    }
}
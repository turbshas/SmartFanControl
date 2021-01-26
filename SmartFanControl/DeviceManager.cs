using SmartFanControl.Config;
using SmartFanControl.Devices;
using SmartFanControl.Hardware;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartFanControl
{
    internal class DeviceManager : IDeviceManager, IDisposable
    {
        private readonly static DeviceManager _instance;

        static DeviceManager()
        {
            _instance = new DeviceManager();
        }

        public static DeviceManager Instance { get => _instance; }

        private readonly IConfigNotifier _notifier;
        private readonly ConcurrentDictionary<string, IHardwareDevice> _hardwareDevices;
        private readonly ConcurrentDictionary<string, List<string>> _hardwareMappings;
        private readonly ConcurrentDictionary<string, IDevice> _devices;
        private bool disposedValue;

        private DeviceManager()
        {
            _notifier = new FileConfigNotifier();
            _devices = new ConcurrentDictionary<string, IDevice>();
            _hardwareMappings = new ConcurrentDictionary<string, List<string>>();

            List<IHardwareDevice> hardwareDevices = FindHardwareDevices();
            _hardwareDevices = new ConcurrentDictionary<string, IHardwareDevice>(hardwareDevices.Select(h => new KeyValuePair<string, IHardwareDevice>(h.Id, h)));

            List<IDeviceConfig> configs = _notifier.GetDeviceConfigs();
            foreach (IDeviceConfig config in configs)
            {
                IDevice device = CreateDeviceFromConfig(config);
                _devices.AddOrUpdate(config.Id, device, (id, existingDevice) => { return device; });
            }
        }

        public IDevice AddDevice(IDeviceConfig config)
        {
            List<string> deviceIdsUsingHw = _hardwareMappings.GetOrAdd(config.HardwareId, new List<string>(new string[] { config.Id }));
            if (deviceIdsUsingHw != null && config.Type == DeviceType.Fan && deviceIdsUsingHw.Count > 1)
            {
                // Hardware ID is a fan and is already mapped to a device
                return null;
            }

            // Hardware ID is free to use
            deviceIdsUsingHw.Add(config.Id);
            IDevice device = CreateDeviceFromConfig(config);
            _devices.TryAdd(device.Id, device);
            return device;
        }

        public void UpdateDevice(IDeviceConfig config)
        {
            _notifier.SetConfig(config.Id, config);
        }

        public IDevice GetDevice(string id)
        {
            _devices.TryGetValue(id, out IDevice device);
            return device;
        }

        public void RemoveDevice(string id)
        {
            _devices.TryRemove(id, out IDevice device);
            if (device != null)
            {
                device.Dispose();
                _notifier.RemoveConfig(id);
            }
        }

        private List<IHardwareDevice> FindHardwareDevices()
        {
            using HardwareDiscoverer discoverer = new HardwareDiscoverer();
            return discoverer.GetHardwareDevices();
        }

        private IDevice CreateDeviceFromConfig(IDeviceConfig config)
        {
            _hardwareDevices.TryGetValue(config.HardwareId, out IHardwareDevice hardwareDevice);
            return config.Type switch
            {
                DeviceType.Fan => new FanController(this, config as FanConfig, _notifier, hardwareDevice as FanDevice),
                DeviceType.TemperatureSensor => new TemperaturePoller(config as TemperatureSensorConfig, _notifier, hardwareDevice as TemperatureSensor),
                _ => null,
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _notifier.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

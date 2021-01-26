using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Config
{
    internal class TemperatureSensorConfig : IDeviceConfig
    {
        public string Id { get; set; }

        public string HardwareId { get; set; }

        public DeviceType Type { get => DeviceType.TemperatureSensor; }

        public double TargetTemp { get; set; }

        public TimeSpan PollingRate { get; set; }
    }
}

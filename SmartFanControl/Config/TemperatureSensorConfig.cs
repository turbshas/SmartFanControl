using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Config
{
    internal class TemperatureSensorConfig : IDeviceConfig
    {
        public string Id { get; set; }

        public string HardwareId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public DeviceType Type { get => DeviceType.TemperatureSensor; }

        public double TargetTemp { get; set; }

        public TimeSpan PollingRate { get; set; }
    }
}

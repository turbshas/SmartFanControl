using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Config
{
    internal class FanConfig : IDeviceConfig
    {
        public string Id { get; set; }

        public string HardwareId { get; set; }

        public DeviceType Type { get => DeviceType.Fan; }

        public string TemperatureSensorId { get; set; }

        public float TargetTemperature { get; set; }

        public int MinFanSpeedPercent { get; set; }

        public int MaxFanSpeedPercent { get; set; }

        public TimeSpan FanSpeedStepDelay { get; set; }

        public int FanSpeedStepAmount { get; set; }

        public float Sensitivity { get; set; }
    }
}

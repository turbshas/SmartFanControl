using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Config
{
    internal interface IDeviceConfig
    {
        string Id { get; set; }

        string HardwareId { get; set; }

        DeviceType Type { get; }
    }

    internal enum DeviceType
    {
        TemperatureSensor,
        Fan,
    }
}

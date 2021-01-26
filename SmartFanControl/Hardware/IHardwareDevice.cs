using SmartFanControl.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Hardware
{
    internal interface IHardwareDevice
    {
        string Id { get; }

        DeviceType Type { get; }
    }
}

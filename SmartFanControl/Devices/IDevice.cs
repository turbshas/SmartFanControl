using SmartFanControl.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Devices
{
    internal interface IDevice : IDisposable
    {
        string Id { get; }

        DeviceType Type { get; }
    }
}

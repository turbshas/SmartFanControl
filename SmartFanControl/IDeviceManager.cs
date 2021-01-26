using SmartFanControl.Config;
using SmartFanControl.Devices;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl
{
    internal interface IDeviceManager
    {
        IDevice AddDevice(IDeviceConfig config);

        IDevice GetDevice(string id);

        void RemoveDevice(string id);
    }
}

using LibreHardwareMonitor.Hardware.Motherboard;
using LibreHardwareMonitor.Hardware.Motherboard.Lpc;
using SmartFanControl.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Hardware
{
    internal class FanDevice : IHardwareDevice
    {
        private readonly SuperIOHardware _superIoDevice;
        private readonly int _index;

        public FanDevice(SuperIOHardware superIoDevice, int index)
        {
            _superIoDevice = superIoDevice;
            _index = index;
        }

        public string Id { get => $"{_superIoDevice.Identifier}/fancontrol/{_index}"; }

        public DeviceType Type { get => DeviceType.Fan; }

        public int GetFanSpeedRpm()
        {
            if (_superIoDevice == null)
            {
                return 0;
            }

            return (int)_superIoDevice.SuperIO.Fans[_index];
        }

        public int GetFanSpeedPercent()
        {
            if (_superIoDevice == null)
            {
                return 0;
            }

            return (int)_superIoDevice.SuperIO.Controls[_index];
        }

        public void SetSpeed(int percent)
        {
            if (_superIoDevice == null)
            {
                return;
            }

            byte bytePercent;
            if (percent > 100)
            {
                bytePercent = 100;
            }
            else if (percent < 0)
            {
                bytePercent = 0;
            }
            else
            {
                bytePercent = (byte)percent;
            }
            _superIoDevice.SuperIO.SetControl(_index, bytePercent);
        }
    }
}

﻿using LibreHardwareMonitor.Hardware.Motherboard;
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

            _superIoDevice.Update();
            return (int)_superIoDevice.SuperIO.Fans[_index];
        }

        public int GetFanSpeedPercent()
        {
            if (_superIoDevice == null)
            {
                return 0;
            }

            _superIoDevice.Update();
            return (int)_superIoDevice.SuperIO.Controls[_index];
        }

        public void SetSpeed(int percent)
        {
            if (_superIoDevice == null)
            {
                return;
            }

            if (percent > 100)
            {
                percent = 100;
            }
            else if (percent < 0)
            {
                percent = 0;
            }

            int DIVISOR_MIN = 1; // Corresponds with 100%
            int DIVISOR_MAX = 128; // Corresponds with 0%
            int scaledPercent = (int)Math.Round(percent * 128.0 / 100.0);
            int requestedDivisor = DIVISOR_MAX - scaledPercent - 1;
            int testDiv = scaledPercent - 1;
            Console.WriteLine($"Setting divisor to {testDiv}");
            _superIoDevice.SuperIO.SetControl(_index, (byte)(testDiv | (0 << 7)));
        }
    }
}

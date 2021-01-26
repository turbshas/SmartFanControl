using LibreHardwareMonitor.Hardware;
using SmartFanControl.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Hardware
{
    internal class TemperatureSensor : IHardwareDevice
    {
        private readonly ISensor _tempSensor;
        
        public TemperatureSensor(ISensor tempSensor)
        {
            _tempSensor = tempSensor;
        }

        public string Id { get => _tempSensor.Identifier.ToString(); }

        public DeviceType Type { get => DeviceType.TemperatureSensor; }

        public float? Value { get => _tempSensor.Value; }
    }
}

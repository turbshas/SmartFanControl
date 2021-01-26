using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.CPU;
using LibreHardwareMonitor.Hardware.Motherboard;
using SmartFanControl.Hardware;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl
{
    internal class HardwareDiscoverer : IDisposable
    {
        private readonly Computer _computer;
        private readonly List<GenericCpu> _cpus;
        private readonly List<Motherboard> _mobos;
        private readonly List<ISensor> _tempSensors;
        private readonly List<ISensor> _fans;
        private readonly List<SuperIOHardware> _fanControls;
        private bool disposedValue;

        public HardwareDiscoverer()
        {
            _computer = new Computer
            {
                IsCpuEnabled = true,
                IsMotherboardEnabled = true,
            };
            _cpus = new List<GenericCpu>();
            _mobos = new List<Motherboard>();
            _tempSensors = new List<ISensor>();
            _fans = new List<ISensor>();
            _fanControls = new List<SuperIOHardware>();

            _computer.Open();
            _computer.Accept(new UpdateVisitor());

            GetCpusAndMobos();
            GetSensorsAndFans();
        }

        public List<IHardwareDevice> GetHardwareDevices()
        {
            List<IHardwareDevice> hardwareDevices = new List<IHardwareDevice>();
            foreach (ISensor tempSensor in _tempSensors)
            {
                hardwareDevices.Add(new TemperatureSensor(tempSensor));
            }
            foreach (SuperIOHardware fanControl in _fanControls)
            {
                for (int i = 0; i < fanControl.SuperIO.Fans.Length; i++)
                {
                    hardwareDevices.Add(new FanDevice(fanControl, i));
                }
            }

            return hardwareDevices;
        }

        private void GetCpusAndMobos()
        {
            // Get CPUs and Mobos
            foreach (IHardware hardware in _computer.Hardware)
            {
                switch (hardware.HardwareType)
                {
                    case HardwareType.Cpu:
                        _cpus.Add(hardware as GenericCpu);
                        break;
                    case HardwareType.Motherboard:
                        _mobos.Add(hardware as Motherboard);
                        break;
                    default: break;
                }
            }
        }

        private void GetSensorsAndFans()
        {
            // Get Temp sensors, fans, and fan controllers
            foreach (GenericCpu cpu in _cpus)
            {
                _tempSensors.AddRange(GetSensors(cpu, SensorType.Temperature));
            }
            foreach (Motherboard mobo in _mobos)
            {
                _tempSensors.AddRange(GetSensors(mobo, SensorType.Temperature));
                _fans.AddRange(GetSensors(mobo, SensorType.Fan));
                foreach (IHardware subhardware in mobo.SubHardware)
                {
                    if (subhardware.HardwareType == HardwareType.SuperIO)
                    {
                        SuperIOHardware superIo = subhardware as SuperIOHardware;
                        _fanControls.Add(superIo);
                    }
                }
            }
        }

        private List<ISensor> GetSensors(IHardware hardware, SensorType sensorType)
        {
            List<ISensor> sensors = new List<ISensor>();
            if (hardware == null)
            {
                return sensors;
            }

            foreach (IHardware subhardware in hardware.SubHardware)
            {
                foreach (ISensor sensor in subhardware.Sensors)
                {
                    if (sensor.SensorType == sensorType)
                    {
                        sensors.Add(sensor);
                    }
                }
            }

            foreach (ISensor sensor in hardware.Sensors)
            {
                if (sensor.SensorType == sensorType)
                {
                    sensors.Add(sensor);
                }
            }

            return sensors;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _computer.Close();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }
        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
        }
        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}

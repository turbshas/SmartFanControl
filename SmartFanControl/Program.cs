using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.CPU;
using LibreHardwareMonitor.Hardware.Motherboard;
using LibreHardwareMonitor.Hardware.Motherboard.Lpc;
using System;
using System.Collections.Generic;

namespace SmartFanControl
{
    class Program
    {
        /* Modified LibreHardwareMonitorLib files:
         *  - Model: changed from internal to public
         *  - Manufacturer: changed from internal to public
         *  - Chip: changed from internal to public
         *  - SuperIOHardware:
         *      - changed from internal to public
         *      - added public SuperIO getter to return internal ISuperIO
         *  - ISuperIO: changed from internal to public
         */
        /* Plans:
         *  - Can setup multiple fans
         *  - Can setup multiple temperature sensors
         *  - Temp sensors are mapped 1-to-1 to fans
         *  - Each temp sensor has a target temperature and polling rate
         *  - Each fan has min/max fan speeds. step amount, and sensitivity value
         *  - Can have more than 1 temp sensor mapped to the same hardware sensor
         *  - Temp sensor can be mapped to multiple fans
         *  - Can only have 1 fan mapped to each hardware fan
         */
        static void Main(string[] args)
        {
            GetRequiredHardware();
            //Monitor();
            Console.ReadLine();
        }

        static void GetRequiredHardware()
        {
            Computer computer = new Computer
            {
                IsCpuEnabled = true,
                IsMotherboardEnabled = true,
            };

            computer.Open();
            computer.Accept(new UpdateVisitor());
            //Console.WriteLine(computer.GetReport());
            //return;

            List<GenericCpu> cpus = new List<GenericCpu>();
            List<Motherboard> mobos = new List<Motherboard>();

            foreach (IHardware hardware in computer.Hardware)
            {
                switch (hardware.HardwareType)
                {
                    case HardwareType.Cpu:
                        cpus.Add(hardware as GenericCpu);
                        break;
                    case HardwareType.Motherboard:
                        mobos.Add(hardware as Motherboard);
                        break;
                    default: break;
                }
            }

            List<ISensor> tempSensors = new List<ISensor>();
            List<ISensor> fans = new List<ISensor>();
            List<SuperIOHardware> fanControls = new List<SuperIOHardware>();

            foreach (GenericCpu cpu in cpus)
            {
                tempSensors.AddRange(GetSensors(cpu, SensorType.Temperature));
            }
            foreach (Motherboard mobo in mobos)
            {
                tempSensors.AddRange(GetSensors(mobo, SensorType.Temperature));
                fans.AddRange(GetSensors(mobo, SensorType.Fan));
                foreach (IHardware subhardware in mobo.SubHardware)
                {
                    if (subhardware.HardwareType == HardwareType.SuperIO)
                    {
                        SuperIOHardware superIo = subhardware as SuperIOHardware;
                        fanControls.Add(superIo);
                    }
                }
            }

            Console.WriteLine("Temperature sensors:");
            foreach (ISensor tempSensor in tempSensors)
            {
                Console.WriteLine($"\t{tempSensor.Name} (Parent: {tempSensor.Hardware.Name}, ID: {tempSensor.Identifier}), Value: {tempSensor.Value}");
            }
            Console.WriteLine("Fans:");
            foreach (ISensor fan in fans)
            {
                Console.WriteLine($"\t{fan.Name} (Parent: {fan.Hardware.Name}, ID: {fan.Identifier}), Value: {fan.Value}");
            }
            Console.WriteLine("Fan controls:");
            foreach (SuperIOHardware fanControl in fanControls)
            {
                Console.WriteLine($"\t{fanControl.Name}, {fanControl.Sensors}, {fanControl.Identifier}");
                fanControl.SuperIO.SetControl(2, 70);
                foreach (var control in fanControl.SuperIO.Controls)
                {
                    Console.WriteLine($"\t\t{control}");
                }
                foreach (var fan in fanControl.SuperIO.Fans)
                {
                    Console.WriteLine($"\t\t{fan}");
                }
            }

            computer.Close();
        }

        static List<ISensor> GetSensors(IHardware hardware, SensorType sensorType)
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

        static void Monitor()
        {
            Computer computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
                IsControllerEnabled = true,
                IsNetworkEnabled = true,
                IsStorageEnabled = true,
            };

            computer.Open();
            computer.Accept(new UpdateVisitor());

            foreach (IHardware hardware in computer.Hardware)
            {
                Console.WriteLine("Hardware: {0}", hardware.Name);

                foreach (IHardware subhardware in hardware.SubHardware)
                {
                    Console.WriteLine("\tSubhardware: {0}", subhardware.Name);

                    foreach (ISensor sensor in subhardware.Sensors)
                    {
                        Console.WriteLine("\t\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                    }
                }

                foreach (ISensor sensor in hardware.Sensors)
                {
                    Console.WriteLine("\tSensor: {0}, value: {1}", sensor.Name, sensor.Value);
                }
            }

            computer.Close();
        }
    }
}

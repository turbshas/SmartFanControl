using LibreHardwareMonitor.Hardware;
using SmartFanControl.Config;
using SmartFanControl.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SmartFanControl.Devices
{
    internal class TemperaturePoller : IDevice, IDisposable
    {
        private TemperatureSensorConfig _config;
        private bool disposedValue;
        private readonly IConfigNotifier _notifier;
        private readonly TemperatureSensor _tempSensor;
        private readonly TemperatureSmoother _tempSmoother;
        private readonly Timer _timer;

        public TemperaturePoller(TemperatureSensorConfig config, IConfigNotifier notifier, TemperatureSensor tempSensor)
        {
            _config = config;
            _notifier = notifier;
            _tempSensor = tempSensor;

            notifier.ConfigChanged += OnConfigChanged;
            _timer = new Timer(OnTimerTick);
            _timer.Change(_config.PollingRate, _config.PollingRate);
            _tempSmoother = new TemperatureSmoother();
        }

        public string Id { get => _config.Id; }

        public DeviceType Type { get => DeviceType.TemperatureSensor; }

        public float TemperatureValue { get => _tempSmoother.Average; }

        private void OnConfigChanged(object sender, ConfigChangedEventArgs eventArgs)
        {
            if (eventArgs.Id == _config.Id)
            {
                if (eventArgs.Config == null)
                {
                    // Config was removed, ignore the event as we are probably being disposed
                    return;
                }
                _config = eventArgs.Config as TemperatureSensorConfig;
                _timer.Change(_config.PollingRate, _config.PollingRate);
            }
        }

        private void OnTimerTick(object state)
        {
            float? value = _tempSensor.Value;
            if (value.HasValue)
            {
                _tempSmoother.AddValue(value.Value);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _notifier.ConfigChanged -= OnConfigChanged;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _timer.Dispose();
                    _tempSmoother.Dispose();
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
}

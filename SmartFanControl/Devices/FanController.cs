using LibreHardwareMonitor.Hardware.Motherboard.Lpc;
using SmartFanControl.Config;
using SmartFanControl.Controllers;
using SmartFanControl.Hardware;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SmartFanControl.Devices
{
    internal class FanController : IDevice, IDisposable
    {
        private const float TEMPERATURE_STEP = 3.0f;

        private FanConfig _config;
        private TemperaturePoller _tempPoller;
        private bool disposedValue;
        private readonly IDeviceManager _deviceManager;
        private readonly IConfigNotifier _notifier;
        private readonly FanDevice _fanDevice;
        private readonly Timer _timer;
        private int _currentFanPercent;
        private readonly FanSpeedCalculator _fanSpeedCalculator;
        private readonly FanSpeedController _fanSpeedController;

        public FanController(IDeviceManager deviceManager, FanConfig config, IConfigNotifier notifier, FanDevice fanDevice)
        {
            _deviceManager = deviceManager;
            _config = config;
            _notifier = notifier;
            _fanDevice = fanDevice;

            _tempPoller = _deviceManager.GetDevice(_config.TemperatureSensorId) as TemperaturePoller;
            _timer = new Timer(OnTimerTick);
            _timer.Change(config.FanSpeedStepDelay, config.FanSpeedStepDelay);
            notifier.ConfigChanged += OnConfigChange;
            _currentFanPercent = fanDevice.GetFanSpeedPercent();
            _fanSpeedCalculator = new FanSpeedCalculator(fanDevice.GetFanSpeedPercent(), config.TargetTemperature, config.FanSpeedStepAmount, config.Sensitivity, TEMPERATURE_STEP);
            _fanSpeedController = new FanSpeedController();

            SetFanSpeed(_currentFanPercent);
        }

        public string Id { get => _config.Id; }

        public DeviceType Type { get => DeviceType.Fan; }

        public int FanSpeedPercent { get => _fanDevice.GetFanSpeedRpm(); }

        private void OnConfigChange(object sender, ConfigChangedEventArgs eventArgs)
        {
            if (eventArgs.Id == _config.Id)
            {
                if (eventArgs.Config == null)
                {
                    // Config was removed, ignore the event as we are probably being disposed
                    return;
                }
                _config = eventArgs.Config as FanConfig;
                _timer.Change(_config.FanSpeedStepDelay, _config.FanSpeedStepDelay);

                _tempPoller = _deviceManager.GetDevice(_config.TemperatureSensorId) as TemperaturePoller;
            }
        }

        private void OnTimerTick(object state)
        {
            // TODO: Sensitivity should only affect when the temperature changes significantly, and fan speed should slow down when temperature remains constant
            /*
            float currentTemp = _tempPoller.TemperatureValue;
            float diff = currentTemp - _config.TargetTemperature;
            bool isPositiveChange = diff > 0.0f;

            float absDiff = Math.Abs(diff);
            int tempStepsChanged = (int)Math.Floor(absDiff / TEMPERATURE_STEP);
            int requiredFanSteps = (int)(tempStepsChanged * _config.Sensitivity);

            int fanPercentChange = _config.FanSpeedStepAmount * requiredFanSteps;
            _currentFanPercent += isPositiveChange ? fanPercentChange : -fanPercentChange;
            Console.WriteLine($"diff is {diff}, temp steps changed is {tempStepsChanged}, required fan steps is {requiredFanSteps}");
            */
            float currentTemp = _tempPoller.TemperatureValue;
            _currentFanPercent = _fanSpeedCalculator.GetNextFanSpeedPercent(currentTemp);
            Console.WriteLine($"Current temperature is: {currentTemp}, setting fan speed to: {_currentFanPercent}, new controller requested fan speed change of: {_fanSpeedController.ComputeFanSpeedChange(_config.TargetTemperature, currentTemp)}");
            SetFanSpeed(_currentFanPercent);
        }

        private void SetFanSpeed(int percent)
        {
            if (percent > _config.MaxFanSpeedPercent)
            {
                percent = _config.MaxFanSpeedPercent;
            }
            else if (percent < _config.MinFanSpeedPercent)
            {
                percent = _config.MinFanSpeedPercent;
            }

            _fanDevice.SetSpeed(percent);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _notifier.ConfigChanged -= OnConfigChange;
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    _timer.Dispose();
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

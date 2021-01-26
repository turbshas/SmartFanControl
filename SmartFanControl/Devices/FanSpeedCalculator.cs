using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Devices
{
    internal class FanSpeedCalculator : IDisposable
    {
        private const int TEMPERATURE_VALUES_TO_KEEP = 4;

        private int _currentFanSpeed;
        private readonly float _targetTemperature;
        private readonly int _fanSpeedPercentStep;
        private readonly float _sensitivity;
        private readonly float _tempChangeThreshold;
        private readonly Queue<float> _pastTemperatures;
        private bool disposedValue;

        public FanSpeedCalculator(int initialFanSpeed, float targetTemperature, int fanSpeedPercentStep, float sensitivity, float tempChangeThreshold)
        {
            _currentFanSpeed = initialFanSpeed;
            _targetTemperature = targetTemperature;
            _fanSpeedPercentStep = fanSpeedPercentStep;
            _sensitivity = sensitivity;
            _tempChangeThreshold = tempChangeThreshold;
            _pastTemperatures = new Queue<float>(TEMPERATURE_VALUES_TO_KEEP);

            for (int i = 0; i < TEMPERATURE_VALUES_TO_KEEP; i++)
            {
                _pastTemperatures.Enqueue(_targetTemperature);
            }
        }

        public int GetNextFanSpeedPercent(float newTemperature)
        {
            float mostRecentTemp = _pastTemperatures.Peek();
            if (Math.Abs(newTemperature - mostRecentTemp) > _tempChangeThreshold)
            {
                return GetFanSpeedTempChanged(newTemperature);
            }
            else
            {
                return GetFanSpeedNoTempChange(newTemperature);
            }
        }

        private int GetFanSpeedTempChanged(float newTemperature)
        {
            // TODO: sensitivity should only come into effect after large changes following periods of small changes in temperature
            float diff = _targetTemperature - newTemperature;
            bool isPositiveChange = diff > 0.0f;

            float absDiff = Math.Abs(diff);
            int tempStepsChanged = (int)Math.Floor(absDiff / _tempChangeThreshold);
            int requiredFanSteps = (int)(tempStepsChanged * _sensitivity);

            int fanPercentChange = _fanSpeedPercentStep * requiredFanSteps;
            _currentFanSpeed += isPositiveChange ? fanPercentChange : -fanPercentChange;
            return _currentFanSpeed;
        }

        private int GetFanSpeedNoTempChange(float newTemperature)
        {
            // TODO: When temperature stabilizes and we decrease fan speed, eventually temps go back up.
            //         need to recongnize this and not kick the fans up a lot, only slightly increment them
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _pastTemperatures.Clear();
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

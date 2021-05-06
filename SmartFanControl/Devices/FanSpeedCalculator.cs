using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SmartFanControl.Devices
{
    internal class FanSpeedCalculator : IDisposable
    {
        /* Gonna try to describe the state machine here:
         *     S1: stay at stable = false, found stable = false
         *     S2: stay at stable = true, found stable = false
         *     S3: stay at stable = true, found stable = true
         *
         *     Start in S1
         *         If temperature changes, stay in S1
         *         If temperature stays the same for a while, move to S2
         *     S2: Decrease fan speed each tick
         *         If temperature increases drastically, go to S1
         *         If temperature stays the same or decreases, stay in S2 and set last stable value
         *         If temperature increases slightly, go to S3
         *     S3: Keep fan speed stable, when entering S3 set fan speed to last stable value
         *         If temperature increases drastically, go to S1
         *         If temperature increases slightly, increase fan speed slightly
         *         If temperature stays the same, stay at last stable value
         *         If temperature decreases, go to S2
         */
        private enum TemperatureState
        {
            Unstable,
            FindingStability,
            Stable,
        }

        private const int TEMPERATURE_VALUES_TO_KEEP = 4;
        private readonly TemperatureValue ZERO;

        private int _currentFanSpeed;
        private readonly TemperatureValue _targetTemperature;
        private readonly int _fanSpeedPercentStep;
        private readonly float _sensitivity;
        private readonly float _tempChangeThreshold;
        private readonly TemperatureQueue<TemperatureValue> _pastTemperatures;
        private bool disposedValue;

        // Used for when we start decreasing fan speed and find a stable speed/temperature
        private TemperatureValue _lastStableTemp;
        private int _lastStableFanSpeed;
        private TemperatureState _state;

        public FanSpeedCalculator(int initialFanSpeed, float targetTemperature, int fanSpeedPercentStep, float sensitivity, float tempChangeThreshold)
        {
            _currentFanSpeed = initialFanSpeed;
            _targetTemperature = new TemperatureValue(targetTemperature, tempChangeThreshold);
            _fanSpeedPercentStep = fanSpeedPercentStep;
            _sensitivity = sensitivity;
            _tempChangeThreshold = tempChangeThreshold;
            _pastTemperatures = new TemperatureQueue<TemperatureValue>(TEMPERATURE_VALUES_TO_KEEP,
                new TemperatureValue(targetTemperature, tempChangeThreshold),
                tempChangeThreshold);

            _lastStableTemp = _targetTemperature;
            _lastStableFanSpeed = initialFanSpeed;
            _state = TemperatureState.Unstable;
            ZERO = new TemperatureValue(0.0f, tempChangeThreshold);
        }

        public int GetNextFanSpeedPercent(float newTemperature)
        {
            TemperatureValue newTemperatureValue = new TemperatureValue(newTemperature, _tempChangeThreshold);
            TemperatureValue mostRecentTemp = _pastTemperatures.NewestValue;
            if (newTemperatureValue > mostRecentTemp)
            {
                return GetFanSpeedTempIncreased(newTemperatureValue);
            }
            else if (newTemperatureValue < mostRecentTemp)
            {
                return GetFanSpeedTempDecreased(newTemperatureValue);
            }
            else
            {
                // Record last stable temp/speed in case decreasing the fan speed bumps the temps
                _lastStableFanSpeed = _currentFanSpeed;
                _lastStableTemp = newTemperatureValue;
                _state = TemperatureState.FindingStability;

                return GetFanSpeedNoTempChange(newTemperatureValue);
            }
        }

        private int GetFanSpeedTempIncreased(TemperatureValue newTemperature)
        {
            // TODO: take into account distance from target temp
            TemperatureValue diff = _pastTemperatures.NewestValue - newTemperature;
            float absDiff = Math.Abs(diff.Value);
            int tempStepsChanged = (int)Math.Floor(absDiff / _tempChangeThreshold);
            float fanPercentChange = _fanSpeedPercentStep * tempStepsChanged;

            if (tempStepsChanged > 2)
            {
                // Only apply sensitivity if we see a large change in temp
                fanPercentChange *= _sensitivity;
                // Large change in temp, need to find a new stable value
                _state = TemperatureState.Unstable;
            }
            else if (_state == TemperatureState.FindingStability)
            {
                // Decreasing the fan speed probably caused us to increase temps
                _currentFanSpeed = _lastStableFanSpeed;
                _state = TemperatureState.Stable;
                return _currentFanSpeed;
            }
            else if (_state == TemperatureState.Stable)
            {
                // Increase fan speed slightly
                _currentFanSpeed += _fanSpeedPercentStep;
                return _currentFanSpeed;
            }

            int percentChangeInt = (int)Math.Floor(fanPercentChange);
            _currentFanSpeed += percentChangeInt;
            return _currentFanSpeed;
        }

        private int GetFanSpeedTempDecreased(TemperatureValue newTemperature)
        {
            bool isBelowTarget = newTemperature < _targetTemperature;

            if (isBelowTarget)
            {
                _currentFanSpeed -= _fanSpeedPercentStep;
            }
            else
            {
                // TODO: take into account the distance from the target temp
                TemperatureValue diff = _pastTemperatures.NewestValue - newTemperature;
                float absDiff = Math.Abs(diff.Value);
                int tempStepsChanged = (int)Math.Floor(absDiff / _tempChangeThreshold);
                float fanPercentChange = _fanSpeedPercentStep * tempStepsChanged;
                _currentFanSpeed -= (int)fanPercentChange;
            }
            return _currentFanSpeed;
        }

        private int GetFanSpeedNoTempChange(TemperatureValue newTemperature)
        {
            // Drop the fan speed 1 step to try and slow down the fans if we haven't found the stable speed
            if (_state != TemperatureState.Stable)
            {
                _currentFanSpeed -= _fanSpeedPercentStep;
            }
            return _currentFanSpeed;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _pastTemperatures.Dispose();
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

        private class TemperatureQueue<T> : IDisposable
        {
            private readonly int _valuesToKeep;
            private readonly float _tempChangeThreshold;
            // List head is the oldest value
            private int _listHead;
            private bool disposedValue;
            private readonly List<T> _values;

            public TemperatureQueue(int valuesToKeep, T initialValue, float tempChangeThreshold)
            {
                _valuesToKeep = valuesToKeep;
                _tempChangeThreshold = tempChangeThreshold;

                _values = new List<T>(valuesToKeep);
                for (int i = 0; i < valuesToKeep; i++)
                {
                    _values.Add(initialValue);
                }
                _listHead = 0;
            }

            public T OldestValue
            {
                get
                {
                    return GetValueAt(_listHead);
                }
            }

            public T NewestValue
            {
                get
                {
                    return GetValueAt(_listHead + _valuesToKeep - 1);
                }
            }

            public void Enqueue(T value)
            {
                _values[_listHead] = value;
                _listHead = (_listHead + 1) % _valuesToKeep;
            }

            public T GetValueAt(int index)
            {
                int listIndex = (_listHead + index - 1) % _valuesToKeep;
                return _values[listIndex];
            }

            public bool HasStabilized()
            {
                for (int i = _listHead; i < _values.Count - 1; i++)
                {
                    if (!GetValueAt(i).Equals(GetValueAt(i + 1)))
                    {
                        return false;
                    }
                }
                return true;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _values.Clear();
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

        private class TemperatureValue : IEquatable<TemperatureValue>
        {
            private readonly float _temperature;
            private readonly float _tempChangeThreshold;

            public TemperatureValue(float temperature, float tempChangeThreshold)
            {
                _temperature = temperature;
                _tempChangeThreshold = tempChangeThreshold;
            }

            public float Value { get { return _temperature; } }

            public bool Equals([AllowNull] TemperatureValue other)
            {
                if (ReferenceEquals(this, other)) return true;
                if (other == null) return false;

                float threshold = GetThresholdForComparison(other);
                return IsOverlapping(_temperature, other._temperature, threshold);
            }

            public override bool Equals(object obj)
            {
                if (obj is TemperatureValue) return Equals(obj as TemperatureValue);
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            private float GetThresholdForComparison(TemperatureValue other)
            {
                return Math.Min(_tempChangeThreshold, other._tempChangeThreshold);
            }
            
            private bool IsOverlapping(float temp1, float temp2, float threshold)
            {
                float temp1Min = temp1 - threshold;
                float temp1Max = temp1 + threshold;
                float temp2Min = temp2 - threshold;
                float temp2Max = temp2 + threshold;

                return (temp1Max >= temp2Min && temp1Max <= temp2Max)
                    || (temp2Max >= temp1Min && temp2Max <= temp1Max);
            }

            public static bool operator ==(TemperatureValue left, TemperatureValue right)
            {
                if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
                return left.Equals(right);
            }

            public static bool operator !=(TemperatureValue left, TemperatureValue right)
            {
                if (ReferenceEquals(left, null)) return !ReferenceEquals(right, null);
                return !left.Equals(right);
            }

            public static bool operator <(TemperatureValue left, TemperatureValue right)
            {
                if (left == null || right == null) throw new InvalidOperationException();

                float threshold = left.GetThresholdForComparison(right);
                float leftMax = left._temperature + threshold;
                float rightMin = right._temperature - threshold;
                return leftMax < rightMin;
            }

            public static bool operator >(TemperatureValue left, TemperatureValue right)
            {
                if (left == null || right == null) throw new InvalidOperationException();

                float threshold = left.GetThresholdForComparison(right);
                float leftMin = left._temperature - threshold;
                float rightMax = right._temperature + threshold;
                return leftMin > rightMax;
            }

            public static bool operator <=(TemperatureValue left, TemperatureValue right)
            {
                return left == right || left < right;
            }

            public static bool operator >=(TemperatureValue left, TemperatureValue right)
            {
                return left == right || left > right;
            }

            public static TemperatureValue operator +(TemperatureValue left, TemperatureValue right)
            {
                if (left == null || right == null) throw new InvalidOperationException();

                return new TemperatureValue(left.Value + right.Value, Math.Max(left._tempChangeThreshold, right._tempChangeThreshold));
            }

            public static TemperatureValue operator -(TemperatureValue left, TemperatureValue right)
            {
                if (left == null || right == null) throw new InvalidOperationException();

                return new TemperatureValue(left.Value - right.Value, Math.Max(left._tempChangeThreshold, right._tempChangeThreshold));
            }
        }
    }
}

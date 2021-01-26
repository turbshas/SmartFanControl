using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Devices
{
    internal class TemperatureSmoother : IDisposable
    {
        private const int NUM_VALUES_TO_KEEP = 4;

        private readonly Queue<float> _values;
        private bool disposedValue;

        public TemperatureSmoother()
        {
            _values = new Queue<float>(NUM_VALUES_TO_KEEP);
            Average = 0.0f;
        }

        public float Average { get; private set; }

        public void AddValue(float tempValue)
        {
            if (_values.Count == NUM_VALUES_TO_KEEP)
            {
                _values.Dequeue();
            }
            _values.Enqueue(tempValue);
            Average = ComputeAverage();
        }

        public void ClearValues()
        {
            _values.Clear();
            Average = 0.0f;
        }

        private float ComputeAverage()
        {
            float[] values = _values.ToArray();
            float total = 0.0f;
            foreach (float value in values)
            {
                total += value;
            }
            int divisor = values.Length > 0 ? values.Length : 1;
            return total / divisor;
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
}

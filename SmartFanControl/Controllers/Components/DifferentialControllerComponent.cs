using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Controllers.Components
{
    class DifferentialControllerComponent : ControllerComponentBase
    {
        private double _lastError;

        public DifferentialControllerComponent(double multiplierConstant)
            : base(multiplierConstant)
        {
        }

        public override double ComputeOutput(double error)
        {
            var newValue = MultiplierConstant * (error - _lastError);
            _lastError = error;
            return newValue;
        }
    }
}

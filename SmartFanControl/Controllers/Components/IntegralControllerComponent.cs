using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Controllers.Components
{
    class IntegralControllerComponent : ControllerComponentBase
    {
        private double _lastOutputValue;

        public IntegralControllerComponent(double multiplierConstant)
            : base(multiplierConstant)
        {
        }

        public override double ComputeOutput(double error)
        {
            _lastOutputValue += (MultiplierConstant * error);
            return _lastOutputValue;
        }
    }
}

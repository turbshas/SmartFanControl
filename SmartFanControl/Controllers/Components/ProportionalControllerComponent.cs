using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Controllers.Components
{
    class ProportionalControllerComponent : ControllerComponentBase
    {
        public ProportionalControllerComponent(double multiplierConstant)
            : base(multiplierConstant)
        {
        }

        public override double ComputeOutput(double error)
        {
            return MultiplierConstant * error;
        }
    }
}

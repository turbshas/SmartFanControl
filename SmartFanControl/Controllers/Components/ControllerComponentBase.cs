using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Controllers.Components
{
    abstract class ControllerComponentBase : IControllerComponent
    {
        public ControllerComponentBase(double multiplierConstant)
        {
            MultiplierConstant = multiplierConstant;
        }

        protected double MultiplierConstant { get; set; }

        public abstract double ComputeOutput(double error);
    }
}

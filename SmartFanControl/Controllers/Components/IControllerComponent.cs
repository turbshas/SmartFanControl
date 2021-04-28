using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Controllers.Components
{
    public interface IControllerComponent
    {
        double ComputeOutput(double error);
    }
}

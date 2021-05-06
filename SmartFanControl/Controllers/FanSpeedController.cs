using SmartFanControl.Controllers.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartFanControl.Controllers
{
    class FanSpeedController
    {
        // TODO: Find good values for these
        private const double PROPORTIONAL_CONSTANT = 0.05;
        private const double INTEGRAL_CONSTANT = 0.001;
        private const double DIFFERENTIAL_CONSTANT = 0.5;

        private readonly List<IControllerComponent> _components;

        public FanSpeedController()
        {
            _components = new List<IControllerComponent>
            {
                new ProportionalControllerComponent(PROPORTIONAL_CONSTANT),
                new IntegralControllerComponent(INTEGRAL_CONSTANT),
                new DifferentialControllerComponent(DIFFERENTIAL_CONSTANT),
            };
        }

        /// <summary>
        /// Calculates the required change to fan speed in percent based on the difference between the current temperature and the target temperature.
        /// </summary>
        /// <param name="targetTemperature">The target temperature in degrees Celsius.</param>
        /// <param name="currentTemperature">The current temperature in degrees Celsius.</param>
        /// <returns>The next fan speed to set in percent.</returns>
        public double ComputeFanSpeedChange(double targetTemperature, double currentTemperature)
        {
            var error = currentTemperature - targetTemperature;
            var sum = 0.0;
            foreach (var component in _components)
            {
                sum += component.ComputeOutput(error);
            }

            return sum;
        }
    }
}

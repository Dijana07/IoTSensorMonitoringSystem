using IoT.Shared.Enums;
using Monitoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.State
{
    public abstract class SensorState
    {
        public abstract SensorStatus StatusName { get; }
        public abstract void HandleState(SensorTelemetryContext context);
    }
}

using IoT.Shared.Enums;
using Monitoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.State
{
    public class StableState : SensorState
    {
        public override SensorStatus StatusName => SensorStatus.Stable;
        public override void HandleState(SensorTelemetryContext context) 
            => context.currentState = new AlarmState();
    }
}

using IoT.Shared.Enums;
using Monitoring.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.State
{
    public class ErrorState : SensorState
    {
        public override SensorStatus StatusName => SensorStatus.Error;
        public override void HandleState(SensorTelemetryContext context)
            => context.currentState = new UnactiveState();
    }
}

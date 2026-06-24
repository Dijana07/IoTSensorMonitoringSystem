
using IoT.Shared.Models;
using Monitoring.Interfaces;
using Monitoring.State;

namespace Monitoring.Models
{
    public class SensorTelemetryContext
    {
        public SensorState currentState;

        private SensorTelemetry telemetry;

        private readonly ISensorStateMapper stateMapper;

        public SensorTelemetry Telemetry => telemetry;

        public SensorTelemetryContext(SensorTelemetry telemetry, ISensorStateMapper mapper)
        {
            this.telemetry = telemetry;
            stateMapper = mapper;

            currentState = mapper.MapStatusToState(telemetry.Status);
        }
    }
}

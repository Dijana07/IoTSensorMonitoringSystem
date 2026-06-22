using IoT.Shared.Enums;
using Monitoring.Interfaces;
using Monitoring.State;

namespace Monitoring.Mappers
{
    public class SensorStateMapper : ISensorStateMapper
    {
        public SensorState MapStatusToState(SensorStatus status)
        {
            switch (status)
            {
                case SensorStatus.Stable:
                    return new StableState();
                case SensorStatus.Alarm:
                    return new AlarmState();
                case SensorStatus.Error:
                    return new ErrorState();
                case SensorStatus.Unactive:
                    return new UnactiveState();
                default:
                    return new StableState();
            }
        }
    }
}

using IoT.Shared.Enums;
using Monitoring.State;

namespace Monitoring.Interfaces
{
    public interface ISensorStateMapper
    {
        SensorState MapStatusToState(SensorStatus status);
    }
}

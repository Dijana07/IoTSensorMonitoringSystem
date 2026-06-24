using IoT.Shared.Models;
using Monitoring.Models;
using System.Collections.Generic;

namespace Monitoring.Interfaces
{
    public interface IDataLoader
    {
        (List<Sensor> Sensors, List<SensorTelemetryContext> Telemetries) LoadAll();
    }
}

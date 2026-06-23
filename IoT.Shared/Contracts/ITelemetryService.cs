using IoT.Shared.Models;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace IoT.Shared.Contracts
{
    [ServiceContract]
    public interface ITelemetryService
    {
        [OperationContract]
        List<SensorTelemetry> GetTelemetryData(DateTime from, DateTime to);
    }
}

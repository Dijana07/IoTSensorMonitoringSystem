using LiveChartsCore;
using Monitoring.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Monitoring.Interfaces
{
    public interface ITelemetryChartService
    {
        ObservableCollection<ISeries> BuildChart(IEnumerable<SensorTelemetryContext> telemetries);
    }
}

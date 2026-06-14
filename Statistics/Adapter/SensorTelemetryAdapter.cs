using IoT.Shared.Models;
using Statistics.Interfaces;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Adapter
{
	public class SensorTelemetryAdapter : IDataProvider
	{
		private IDataReader dataReader;

        public SensorTelemetryAdapter(IDataReader dataReader)
        {
            this.dataReader = dataReader;
        }

        public Tuple<string, List<Reading>> GetData(DateTime from, DateTime to)
        {
            List<SensorTelemetry> sensorTelemetries = dataReader.ReadData(from, to);

            if (sensorTelemetries != null && sensorTelemetries.Count > 0)
            {
                DateTime fromDate = sensorTelemetries.Min(t => t.DateTime);
                DateTime toDate = sensorTelemetries.Max(t => t.DateTime);

                string key = $"{fromDate:yyyy-MM-dd}_{toDate:yyyy-MM-dd}";

                var readings = sensorTelemetries
                    .GroupBy(t => t.SensorId)
                    .Select(s => new Reading
                    {
                        SensorId = s.Key,
                        Values = s.Select(i => i.Value).ToList(),
                        AlarmCount = s.Where(i => i.Status == IoT.Shared.Enums.SensorStatus.Alarm).Count()
                    })
                    .ToList();

                return new Tuple<string, List<Reading>>(key, readings);
            }

            return null;
        }
    }
}

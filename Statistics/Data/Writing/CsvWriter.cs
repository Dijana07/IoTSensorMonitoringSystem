using Statistics.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Data.Writing
{
	public class CsvWriter : IFileWritter
	{
		private string path;

        public CsvWriter(string path)
        {
            this.path = path;
        }

        public void Write(Dictionary<string, List<Models.Reading>> data)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Period,SensorId,Values,AlarmCount");

            foreach (var period in data)
            {
                foreach (var reading in period.Value)
                {
                    string values = string.Join(
                        ";",
                        reading.Values.Select(v =>
                            v.ToString(CultureInfo.InvariantCulture)));

                    sb.AppendLine(
                        $"{period.Key}," +
                        $"{reading.SensorId}," +
                        $"\"{values}\"," +
                        $"{reading.AlarmCount}");
                }
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}

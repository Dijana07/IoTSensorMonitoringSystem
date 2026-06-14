using IoT.Shared.Models;
using Newtonsoft.Json;
using Statistics.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Statistics.Models;

namespace Statistics.Data.Reading
{
	public class FileDataReader : IDataReader
	{
		private string path;

        public FileDataReader(string path)
        {
            this.path = path;
        }

        public List<SensorTelemetry> ReadData(DateTime from, DateTime to)
        {
            if (!File.Exists(path))
            {
                return new List<SensorTelemetry>();
            }

            string json = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<List<SensorTelemetry>>(json)
                        .Where(t => t.DateTime.Date >= from && t.DateTime.Date <= to)
                        .ToList()
                   ?? new List<SensorTelemetry>();
        }    
    }
}

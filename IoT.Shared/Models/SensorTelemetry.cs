using IoT.Shared.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Shared.Models
{
	public class SensorTelemetry
	{
		public Guid SensorId { get; set; }

		public DateTime DateTime { get; set; }

        public double Value { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public SensorStatus Status { get; set; }
    }
}

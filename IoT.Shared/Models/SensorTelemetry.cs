using IoT.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoT.Shared.Models
{
	public class SensorTelemetry
	{
		public Guid Id { get; set; }

		public DateTime DateTime { get; set; }

        public double Value { get; set; }

        public SensorStatus Status { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Models
{
    public class Reading
	{
		public Guid SensorId { get; set; }

		public List<double> Values { get; set; }

		public int AlarmCount { get; set; }
	}
}

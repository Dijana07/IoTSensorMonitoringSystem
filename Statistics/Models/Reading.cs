using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Models
{
    public class Reading
	{
		public double Value { get; set; }

		public DateTime DateTime {  get; set; }

		public bool Alarm { get; set; }
	}
}

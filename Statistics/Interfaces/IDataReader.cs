using IoT.Shared.Models;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Interfaces
{
	public interface IDataReader
	{
		Dictionary<string, Reading> ReadData(List<SensorTelemetry> list);
	}
}

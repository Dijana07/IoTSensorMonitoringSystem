using IoT.Shared.Models;
using Statistics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Data.Reading
{
    public class ConnectionDataReader : IDataReader
    {
        public List<SensorTelemetry> ReadData(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }
    }
}

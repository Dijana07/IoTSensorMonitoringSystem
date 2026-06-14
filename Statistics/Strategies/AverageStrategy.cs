using Statistics.Interfaces;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Strategies
{
    public class AverageStrategy : IStatisticsStrategy
    {
        public List<Result> Calculate(List<Reading> data)
        {
            return data
                .Select(x => new Result 
                    { 
                        SensorId = x.SensorId,
                        Value = Math.Round(x.Values.Average(), 2)
                    })
                .ToList();
        }

        public override string ToString()
        {
            return "Average";
        }
    }
}

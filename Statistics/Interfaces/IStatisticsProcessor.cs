using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Interfaces
{
    public interface IStatisticsProcessor
	{
        List<Result> ProcessData(DateTime from, DateTime to);
        Dictionary<string, List<Reading>> GetData();
	}
}

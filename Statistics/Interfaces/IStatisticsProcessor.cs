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
        Dictionary<string, List<Reading>> GetData();
        void LoadData(DateTime from, DateTime to);
        List<Result> ProcessData(DateTime from, DateTime to);
        void SetStatisticsStrategy(IStatisticsStrategy strategy);
	}
}

using Statistics.Interfaces;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Processing
{
    public class StatisticsProcessor : IStatisticsProcessor
	{
		private IStatisticsStrategy strategy;

		public StatisticsProcessor(IStatisticsStrategy strategy)
		{
			this.strategy = strategy;
		}

        public List<Result> ProcessData(DateTime from, DateTime to, Dictionary<string, List<Reading>> data)
		{
            var values = data.Values.SelectMany(x => x).ToList();
            return strategy.Calculate(values);
        }

		public void SetStatisticsStrategy(IStatisticsStrategy strategy)
		{
			this.strategy = strategy;
		}

		public string GetStatisticsStrategy()
		{
			return strategy.ToString();	
		}
	}
}

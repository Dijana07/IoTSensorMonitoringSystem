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
		private Dictionary<string, List<Reading>> data;
		private IDataProvider dataProvider;

		public StatisticsProcessor(IStatisticsStrategy strategy, IDataProvider dataProvider)
		{
			this.strategy = strategy;
			this.dataProvider = dataProvider;
			data = new Dictionary<string, List<Reading>>();
		}

        public Dictionary<string, List<Reading>> GetData()
        {
            return data;
        }

        public void LoadData(DateTime from, DateTime to)
		{
			var loadedData = dataProvider.GetData(from, to);
			if (loadedData != null) 
			{
				data.Add(loadedData.Item1, loadedData.Item2);
			}
		}

        public List<Result> ProcessData(DateTime from, DateTime to)
        {
            string key = $"{from:yyyy-MM-dd}_{to:yyyy-MM-dd}";
			var data = dataProvider.GetData(from, to);
			return strategy.Calculate(data.Item2);
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

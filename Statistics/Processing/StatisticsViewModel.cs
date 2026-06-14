using Statistics.Interfaces;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Processing
{
	public class StatisticsViewModel : IStatisticsProcessor
	{
		private IStatisticsStrategy strategy;
		private Dictionary<string, Reading> data;
		private IDataReader reader;

		public void LoadData()
		{
			throw new NotImplementedException();
		}

        public Result ProcessData()
        {
            throw new NotImplementedException();
        }

        public StatisticsViewModel()
		{
			throw new NotImplementedException();
		}
	}
}

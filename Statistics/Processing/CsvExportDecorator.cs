using Statistics.Interfaces;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Processing
{
	public class CsvExportDecorator : IStatisticsProcessor
	{
		private IStatisticsProcessor processor;
		private IFileWritter writter;

		public CsvExportDecorator(IStatisticsProcessor processor, IFileWritter writter)
		{
			this.processor = processor;
			this.writter = writter;
		}

		public void ExportData()
		{
			writter.Write(GetData());
		}

        public Dictionary<string, List<Reading>> GetData()
        {
            return processor.GetData();
        }

        public void LoadData(DateTime from, DateTime to)
        {
            processor.LoadData(from, to);
        }

        public List<Result> ProcessData(DateTime from, DateTime to)
        {
            return processor.ProcessData(from, to);
        }

        public void SetStatisticsStrategy(IStatisticsStrategy strategy)
        {
            processor.SetStatisticsStrategy(strategy);
        }
    }
}

using Statistics.Interfaces;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
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

		public void ExportData(List<Result> data, DateTime from, DateTime to)
		{
			writter.Write(data, from, to, GetStatisticsStrategy());
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
            var data = processor.ProcessData(from, to);
            ExportData(data, from, to);
            return data;
        }

        public void SetStatisticsStrategy(IStatisticsStrategy strategy)
        {
            processor.SetStatisticsStrategy(strategy);
        }

        public string GetStatisticsStrategy() {
            return processor.GetStatisticsStrategy();
        }
    }
}

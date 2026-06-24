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

        public List<Result> ProcessData(DateTime from, DateTime to, Dictionary<string, List<Reading>> data)
        {
            var processedData = processor.ProcessData(from, to, data);
            ExportData(processedData, from, to);
            return processedData;
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

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

		public CsvExportDecorator()
		{
			throw new NotImplementedException();
		}

		public void ExportData()
		{
			throw new NotImplementedException();
		}

        public Result ProcessData()
        {
            throw new NotImplementedException();
        }
    }
}

using Statistics.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Data.Writing
{
	public class CsvWriter : IFileWritter
	{
		private string path;

        public void Write(Dictionary<string, Models.Reading> data)
        {
            throw new NotImplementedException();
        }
    }
}

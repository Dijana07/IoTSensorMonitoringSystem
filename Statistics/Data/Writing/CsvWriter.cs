using Statistics.Interfaces;
using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Data.Writing
{
	public class CsvWriter : IFileWritter
	{
		private string path;

        public CsvWriter(string path)
        {
            this.path = path;
        }

        public void Write(List<Result> data, DateTime from, DateTime to, string action)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Period,SensorId,{action}");

            foreach (var result in data)
            {
                sb.AppendLine(
                    $"{from.ToShortDateString()} - {to.ToShortDateString()}," +
                    $"{result.SensorId}," +
                    $"{result.Value}");
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}

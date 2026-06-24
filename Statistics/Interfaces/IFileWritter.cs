using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Interfaces
{
    public interface IFileWritter
	{
		void Write(List<Result> data, DateTime from, DateTime to, string action);
	}
}

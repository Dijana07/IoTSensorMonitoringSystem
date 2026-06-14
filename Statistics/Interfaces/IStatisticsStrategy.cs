using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Interfaces
{
    public interface IStatisticsStrategy
	{
		Result Calculate(Dictionary<string, Reading> data);
	}
}

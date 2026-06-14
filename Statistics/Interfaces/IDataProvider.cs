using Statistics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statistics.Interfaces
{
    public interface IDataProvider
    {
        Tuple<string, List<Reading>> GetData(DateTime from, DateTime to);
    }
}

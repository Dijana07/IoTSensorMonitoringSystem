using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.Validations
{
    public class SensorValidator
    {
        public bool Validate(string name, string type, string location, string minText, string maxText,
            out string errorMessage, out double min, out double max)
        {
            min = 0;
            max = 0;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(location))
            {
                errorMessage = "All text fields must be filled!";
                return false;
            }

            if (!double.TryParse(minText, out min) || !double.TryParse(maxText, out max))
            {
                errorMessage = "Min and Max fields must be numbers!";
                return false;
            }

            if (min >= max)
            {
                errorMessage = "Minimum value must be less than maximum value!";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}

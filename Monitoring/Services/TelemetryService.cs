using IoT.Shared.Contracts;
using IoT.Shared.Enums;
using IoT.Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Monitoring.Services
{
    [ServiceBehavior]
    public class TelemetryService : ITelemetryService
    {

        public List<SensorTelemetry> GetTelemetryData(DateTime from, DateTime to)
        {
            var telemetryList = new List<SensorTelemetry>();
            string telemetryXmlPath = "telemetryHistory.xml"; 

            if (File.Exists(telemetryXmlPath))
            {
                try
                {
                    XDocument doc = XDocument.Load(telemetryXmlPath);
                    foreach (XElement el in doc.Root.Elements("Telemetry"))
                    {
                        double value = double.Parse(el.Element("Value").Value, CultureInfo.InvariantCulture);
                        var telemetry = new SensorTelemetry
                        {
                            SensorId = Guid.Parse(el.Element("SensorId").Value),
                            DateTime = DateTime.Parse(el.Element("DateTime").Value),
                            Value = value,
                            Status = (SensorStatus)Enum.Parse(typeof(SensorStatus), el.Element("Status").Value)
                        };

                        if (telemetry.DateTime >= from && telemetry.DateTime <= to)
                        {
                            telemetryList.Add(telemetry);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error reading telemetry file", ex);
                }
            }
            return telemetryList;
        }
    }
}

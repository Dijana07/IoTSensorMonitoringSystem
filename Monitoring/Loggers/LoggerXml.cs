using IoT.Shared.Models;
using Monitoring.Interfaces;
using Monitoring.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Monitoring.Loggers
{
    public class LoggerXml : ILogger
    {
        private readonly string path;

        public LoggerXml(string path)
        {
            this.path = path;
        }

        public void Log(Sensor sensor, string message)
        {
            XDocument doc = File.Exists(path) ? XDocument.Load(path) : new XDocument(new XElement("Logs"));

            doc.Root.Add(new XElement("LogEntry",
                new XElement("Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new XElement("Sensor", sensor.Name),
                new XElement("Message", message)
            ));
            doc.Save(path);
        }

        public void SaveState(IEnumerable<Sensor> sensors, IEnumerable<SensorTelemetryContext> telemetries, string sensorsPath, string telemetryPath)
        {
            new XDocument(
                new XElement("Sensors",
                    sensors.Select(s => new XElement("Sensor",
                        new XElement("Id", s.Id.ToString()),
                        new XElement("Name", s.Name),
                        new XElement("Type", s.Type),
                        new XElement("Location", s.Location),
                        new XElement("MinValue", s.MinValue.ToString()),
                        new XElement("MaxValue", s.MaxValue.ToString())
                    ))
                )
            ).Save(sensorsPath);

            new XDocument(
                new XElement("Telemetries",
                    telemetries.Select(t => new XElement("Telemetry",
                        new XElement("SensorId", t.Telemetry.SensorId.ToString()),
                        new XElement("DateTime", t.Telemetry.DateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                        new XElement("Value", t.Telemetry.Value.ToString()),
                        new XElement("Status", t.Telemetry.Status.ToString())
                    ))
                )
            ).Save(telemetryPath);
        }

        public void SaveSingleTelemetryHistory(SensorTelemetry telemetry, string filePath)
        {
            XDocument doc;
            if (File.Exists(filePath))
            {
                doc = XDocument.Load(filePath);
            }
            else
            {
                doc = new XDocument(new XElement("TelemetryHistory"));
            }

            doc.Root.Add(
                new XElement("Telemetry",
                    new XElement("SensorId", telemetry.SensorId.ToString()),
                    new XElement("DateTime", telemetry.DateTime.ToString("yyyy-MM-dd HH:mm:ss")),
                    new XElement("Value", telemetry.Value),
                    new XElement("Status", telemetry.Status.ToString())
                ));

            doc.Save(filePath);
        }
    }
}
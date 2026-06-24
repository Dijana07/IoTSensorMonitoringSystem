using IoT.Shared.Enums;
using IoT.Shared.Models;
using Monitoring.Interfaces;
using Monitoring.Mappers;
using Monitoring.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
namespace Monitoring.Data
{
    public class DataLoader:IDataLoader
    {
        private readonly string sensorsPath;
        private readonly string telemetryPath;
        private readonly SensorStateMapper stateMapper;

        public DataLoader(string sensorsPath, string telemetryPath, SensorStateMapper stateMapper)
        {
            this.sensorsPath = sensorsPath;
            this.telemetryPath = telemetryPath;
            this.stateMapper = stateMapper;
        }

        public (List<Sensor> Sensors, List<SensorTelemetryContext> Telemetries) LoadAll()
        {
            var sensors = LoadSensors();
            var telemetries = LoadTelemetries();

            return (sensors, telemetries);
        }

        private List<Sensor> LoadSensors()
        {
            if (!File.Exists(sensorsPath))
                return new List<Sensor>();

            var doc = XDocument.Load(sensorsPath);

            return doc.Root.Elements("Sensor")
                .Select(el => new Sensor
                {
                    Id = Guid.Parse(el.Element("Id").Value),
                    Name = el.Element("Name").Value,
                    Type = el.Element("Type").Value,
                    Location = el.Element("Location").Value,
                    MinValue = double.Parse(el.Element("MinValue").Value),
                    MaxValue = double.Parse(el.Element("MaxValue").Value)
                })
                .ToList();
        }

        private List<SensorTelemetryContext> LoadTelemetries()
        {
            if (!File.Exists(telemetryPath))
                return new List<SensorTelemetryContext>();

            var doc = XDocument.Load(telemetryPath);

            return doc.Root.Elements("Telemetry")
                .Select(el =>
                {
                    var raw = new SensorTelemetry
                    {
                        SensorId = Guid.Parse(el.Element("SensorId").Value),
                        DateTime = DateTime.Parse(el.Element("DateTime").Value),
                        Value = double.Parse(el.Element("Value").Value),
                        Status = (SensorStatus)Enum.Parse(typeof(SensorStatus), el.Element("Status").Value)
                    };

                    return new SensorTelemetryContext(raw, stateMapper);
                })
                .ToList();
        }
    }
}

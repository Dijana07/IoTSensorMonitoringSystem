using IoT.Shared.Models;
using Monitoring.Interfaces;
using System;
using System.IO;
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
            XDocument doc;
            if (File.Exists(path))
            {
                try
                {
                    doc = XDocument.Load(path);
                }
                catch
                {
                    doc = new XDocument(new XElement("Logs"));
                }
            }
            else
            {
                doc = new XDocument(new XElement("Logs"));
            }

            XElement newLog = new XElement("LogEntry",
                new XElement("Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                new XElement("Sensor", sensor.Name),
                new XElement("Message", message)
            );

            doc.Root.Add(newLog);
            doc.Save(path);
        }
    }
}
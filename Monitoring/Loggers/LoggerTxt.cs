using IoT.Shared.Models;
using Monitoring.Interfaces;
using System;
using System.IO;

namespace Monitoring.Loggers
{
    public class LoggerTxt:ILogger

    {
        private readonly string path;

        public LoggerTxt(string path)
        {
            this.path = path;
        }

        public void Log(Sensor sensor, string message)
        {
            File.AppendAllText(path,
                $"{DateTime.Now} [Sensor: {sensor.Name}] [Message: {message}]{Environment.NewLine}");
        }
    }
}

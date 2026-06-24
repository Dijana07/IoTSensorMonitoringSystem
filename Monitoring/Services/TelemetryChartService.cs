using IoT.Shared.Enums;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Monitoring.Interfaces;
using Monitoring.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Monitoring.Services
{
    public class TelemetryChartService:ITelemetryChartService
    {
        public ObservableCollection<ISeries> BuildChart(IEnumerable<SensorTelemetryContext> telemetries)
        {
            int stable = telemetries.Count(t => t.Telemetry.Status == SensorStatus.Stable);
            int alarm = telemetries.Count(t => t.Telemetry.Status == SensorStatus.Alarm);
            int error = telemetries.Count(t => t.Telemetry.Status == SensorStatus.Error);
            int unactive = telemetries.Count(t => t.Telemetry.Status == SensorStatus.Unactive);

            return new ObservableCollection<ISeries>
            {
                CreateSeries("Stable", stable, SKColors.Green),
                CreateSeries("Alarm", alarm, SKColors.Orange),
                CreateSeries("Error", error, SKColors.Red),
                CreateSeries("Unactive", unactive, SKColors.Gray),
            };
        }

        private PieSeries<int> CreateSeries(string name, int value, SKColor color)
            => new PieSeries<int>
            {
                Name = name,
                Values = new[] { value },
                Fill = new SolidColorPaint(color)
            };
    }
}

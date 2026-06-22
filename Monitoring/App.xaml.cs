using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Windows;

public partial class App : Application
{
    public App()
    {
        LiveCharts.Configure(config =>
            config
                .AddSkiaSharp()
                .AddDefaultMappers()
        );
    }
}
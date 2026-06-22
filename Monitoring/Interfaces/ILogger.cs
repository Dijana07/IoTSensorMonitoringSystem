using IoT.Shared.Models;

namespace Monitoring.Interfaces
{
    public interface ILogger
    {
        void Log(Sensor sensor, string message = null);
    }
}

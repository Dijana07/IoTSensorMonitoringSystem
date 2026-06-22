using IoT.Shared.Models;
using Monitoring.Interfaces;
using System.Collections.ObjectModel;

namespace Monitoring.Commands
{
    public class AddSensorCommand : ICommand
    {
        public ObservableCollection<Sensor> collection;
        private readonly Sensor sensor;

        public AddSensorCommand(ObservableCollection<Sensor> collection, Sensor sensor)
        {
            this.collection = collection;
            this.sensor = sensor;
        }
        public void Execute()
        {
            collection.Add(sensor);
        }

        public void Undo()
        {
            collection.Remove(sensor);
        }
    }
}

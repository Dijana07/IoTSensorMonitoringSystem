using IoT.Shared.Models;
using Monitoring.Interfaces;
using System.Collections.ObjectModel;

namespace Monitoring.Commands
{
    public class RemoveSensorCommand : ICommand
    {
        public ObservableCollection<Sensor> collection;
        private Sensor sensor;
        private int index;

        public RemoveSensorCommand(ObservableCollection<Sensor> collection, Sensor sensor)
        {
            this.collection = collection;
            this.sensor = sensor;
            this.index = collection.IndexOf(sensor);
        }

        public void Execute()
        {
            collection.Remove(sensor);
        }

        public void Undo()
        {
            collection.Insert(index, sensor);
        }
    }
}
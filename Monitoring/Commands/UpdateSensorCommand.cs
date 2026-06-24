using IoT.Shared.Models;
using Monitoring.Interfaces;

namespace Monitoring.Commands
{
    public class UpdateSensorCommand : ICommand
    {
        private Sensor oldSensor; 
        private Sensor originalState; 
        private Sensor newState; 

        public UpdateSensorCommand(Sensor oldSensor, Sensor newSensor)
        {
            this.oldSensor = oldSensor;

            this.originalState = new Sensor
            {
                Id = oldSensor.Id,
                Name = oldSensor.Name,
                Type = oldSensor.Type,
                Location = oldSensor.Location,
                MinValue = oldSensor.MinValue,
                MaxValue = oldSensor.MaxValue
            };

            this.newState = new Sensor
            {
                Id = newSensor.Id,
                Name = newSensor.Name,
                Type = newSensor.Type,
                Location = newSensor.Location,
                MinValue = newSensor.MinValue,
                MaxValue = newSensor.MaxValue
            };
        }

        public void Execute()
        {
            Copy(newState, oldSensor);
        }

        public void Undo()
        {
            Copy(originalState, oldSensor);
        }

        private void Copy(Sensor source, Sensor target)
        {
            target.Name = source.Name;
            target.Type = source.Type;
            target.Location = source.Location;
            target.MinValue = source.MinValue;
            target.MaxValue = source.MaxValue;
        }
    }
}
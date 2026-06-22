using Monitoring.Interfaces;
using Monitoring.Models;
using Monitoring.State;

namespace Monitoring.Commands
{
    public class ChangeStateCommand : ICommand
    {
        private readonly SensorTelemetryContext context;
        private readonly SensorState oldState;
        private readonly SensorState newState;

        public ChangeStateCommand(SensorTelemetryContext context)
        {
            this.context = context;
            this.oldState = context.currentState;
            oldState.HandleState(context);

            newState = context.currentState;
        }

        public void Execute()
        {
            context.currentState = newState;
            context.Telemetry.Status = newState.StatusName;
        }

        public void Undo()
        {
            context.currentState = oldState;
            context.Telemetry.Status = oldState.StatusName;
        }
    }
}
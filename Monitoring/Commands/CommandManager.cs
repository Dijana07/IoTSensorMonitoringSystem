using IoT.Shared.Models;
using Monitoring.Interfaces;
using System.Collections.Generic;

namespace Monitoring.Commands
{
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack = new Stack<ICommand>();
        private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

        private readonly ILogger logger;
        private Sensor sensor;

        public CommandManager(ILogger logger, Sensor sensor)
        {
            this.logger = logger;
            this.sensor = sensor;

        }

        public void ExecuteCommand(ICommand command)
        {
            command.Execute();
            logger.Log(sensor, $"Executed command: {command.GetType().Name}");
            _undoStack.Push(command);
            _redoStack.Clear(); 
        }

        public void Undo()
        {
            if (_undoStack.Count > 0)
            {
                ICommand command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (_redoStack.Count > 0)
            {
                ICommand command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
            }
        }
    }
}

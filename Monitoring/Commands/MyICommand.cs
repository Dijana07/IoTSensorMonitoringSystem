using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monitoring.Commands
{
    public class MyICommand : System.Windows.Input.ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public MyICommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();
        public void Execute(object parameter) => _execute();

        public event EventHandler CanExecuteChanged
        {
            add => System.Windows.Input.CommandManager.RequerySuggested += value;
            remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
        }
    }
}

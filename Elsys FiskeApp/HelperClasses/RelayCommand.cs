using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Elsys_FiskeApp.ViewModel
{
    public class RelayCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }

        }

        Action<object> execute;
        Func<object, bool> canExecute;

        public RelayCommand(Action<object> _execute, Func<object,bool> _canExecute = null){
            execute = _execute;
            canExecute = _canExecute;

            }

        public bool CanExecute(object? parameter)
        {
            return canExecute == null || canExecute(parameter); // assume that we can execute if no requirement is made.
        }

        public void Execute(object? parameter)
        {
            execute(parameter);
        }

    }
}

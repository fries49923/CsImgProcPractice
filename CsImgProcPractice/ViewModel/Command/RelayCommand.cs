using System;
using System.Windows.Input;

namespace CsImgProcPractice
{
    public class RelayCommand : ICommand
    {
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        private Action<object> executeMethod;
        private Func<object, bool> canExecuteMethod;

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            this.executeMethod = execute;
            this.canExecuteMethod = canExecute;
        }

        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {

        }

        public bool CanExecute(object parameter)
        {
            if (this.canExecuteMethod == null)
            {
                return true;
            }
            else
            {
                return this.canExecuteMethod.Invoke(parameter);
            }
        }

        public void Execute(object parameter)
        {
            this.executeMethod?.Invoke(parameter);
        }
    }
}

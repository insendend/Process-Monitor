using System;
using System.Windows.Input;

namespace ProcessMonitorV2.ViewModels.Commands
{
    abstract class BaseCommand : ICommand
    {
        #region ICommand Implementation

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public abstract bool CanExecute(object parameter);
        public abstract void Execute(object parameter);

        #endregion
    }
}

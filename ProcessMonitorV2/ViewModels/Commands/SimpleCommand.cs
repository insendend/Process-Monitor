using System;

namespace ProcessMonitorV2.ViewModels.Commands
{
    class SimpleCommand : BaseCommand
    {
        #region Fields

        private readonly Action execute;
        private readonly Func<bool> predicate;

        #endregion

        #region Ctor

        public SimpleCommand(Action execute, Func<bool> predicate = null)
        {
            this.execute = execute ?? throw new ArgumentException(nameof(execute));
            this.predicate = predicate;
        }

        #endregion

        #region BaseCommand implementation

        public override bool CanExecute(object parameter)
        {
            return predicate?.Invoke() ?? true;
        }

        public override void Execute(object parameter)
        {
            execute();
        }

        #endregion
    }
}

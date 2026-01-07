using System;
using System.Windows.Input;

namespace GuessWhoClient.Presentation.ViewModels.Base
{
    public sealed class RelayCommandWithParam<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;

        public RelayCommandWithParam(Action<T> execute, Func<T, bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return parameter is T typed && canExecute(typed);
        }

        public void Execute(object parameter)
        {
            if (parameter is T typed)
            {
                execute(typed);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

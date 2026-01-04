using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GuessWhoClient.Presentation.ViewsModels.Base
{
    public sealed class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> executeAsync; 
        private readonly Func<bool> canExecute;
        private bool isExecuting;

        public event EventHandler CanExecuteChanged;

        public AsyncRelayCommand(Func<Task> executeAsync, Func<bool> canExecute)
        {
            this.executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this.canExecute = canExecute ?? throw new ArgumentNullException(nameof(canExecute));
        }

        public bool CanExecute(object parameter)
        {
            return !isExecuting && canExecute();
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
            {
                return;
            }
            try
            {
                isExecuting = true;
                RaiseCanExecuteChanged();
                await executeAsync();
            }
            finally
            {
                isExecuting = false;
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

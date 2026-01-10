using GuessWhoClient.Presentation.ViewModels.Error;
using GuessWhoClient.Presentation.Views.Windows;
using System.Windows;

namespace GuessWhoClient.Presentation.Dialogs
{
    public sealed class GameConfirmDialogService : IGameConfirmDialogService
    {
        private const string EMPTY = "";

        public bool Confirm(string title, string message, string confirmButtonText, string cancelButtonText)
        {
            string safeTitle = title ?? EMPTY;
            string safeMessage = message ?? EMPTY;
            string safeConfirm = confirmButtonText ?? EMPTY;
            string safeCancel = cancelButtonText ?? EMPTY;

            System.Windows.Application app = System.Windows.Application.Current;

            if (app == null)
            {
                return false;
            }

            bool result = false;

            app.Dispatcher.Invoke(() =>
            {
                Window owner = app.MainWindow;

                var viewModel = new GameMessageViewModel(
                    safeTitle,
                    safeMessage,
                    primaryButtonText: safeConfirm,
                    secondaryButtonText: safeCancel);

                var window = new GameMessageWindow(viewModel)
                {
                    Owner = owner
                };

                bool? dialogResult = window.ShowDialog();
                result = dialogResult == true;
            });

            return result;
        }
    }
}

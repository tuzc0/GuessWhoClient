using GuessWhoClient.Presentation.ViewModels.Error;
using GuessWhoClient.Presentation.Views.Windows;
using System.Windows;

namespace GuessWhoClient.Presentation.Dialogs
{
    public sealed class GameMessageDialogService : IGameMessageDialogService
    {
        private const string EMPTY = "";

        public void Show(string title, string message)
        {
            string safeTitle = title ?? EMPTY;
            string safeMessage = message ?? EMPTY;

            System.Windows.Application app = System.Windows.Application.Current;

            if (app == null)
            {
                return;
            }

            app.Dispatcher.Invoke(() =>
            {
                Window owner = app.MainWindow;

                var viewModel = new GameMessageViewModel(safeTitle, safeMessage);

                var window = new GameMessageWindow(viewModel)
                {
                    Owner = owner
                };

                window.ShowDialog();
            });
        }
    }
}
